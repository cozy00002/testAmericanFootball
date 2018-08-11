using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAmericanFootball2.Enums;
using TestAmericanFootball2.Extentions;
using TestAmericanFootball2.Models;
using TestAmericanFootball2.ViewModels;

namespace TestAmericanFootball2.Controllers
{
    public class AmeFootController : Controller
    {
        private readonly TestAmericanFootball2Context _context;

        public AmeFootController(TestAmericanFootball2Context context)
        {
            _context = context;
        }

        #region 作成中

        // GET: AmeFoot
        public ActionResult Index()
        {
            var game = _InitializeGame();
            var ameFootVM = Mapper.Map<AmeFootViiewModel>(game);
            return View(ameFootVM);
        }

        // POST: AmeFoot/Game
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Game(string player1Id,
            string player2Id,
            string method
            )
        {
            AmeFootViiewModel ameFootVM;
            Game game;
            OffenceModeEnum offenceMode;

            if (string.IsNullOrEmpty(player2Id))
            {
                player2Id = Const.COM_NAME_DB;
            }

            bool isAIAuto = true;
            if (method == "コンピューター") { isAIAuto = false; }

            game = await _context.Game.Where(x =>
            x.Player1Id == player1Id && x.Player2Id == player2Id)
            .SingleOrDefaultAsync();
            offenceMode = method.GetOffenceMode();
            switch (offenceMode)
            {
                case OffenceModeEnum.Run:
                case OffenceModeEnum.ShortPass:
                case OffenceModeEnum.LongPass:
                case OffenceModeEnum.Pant:
                case OffenceModeEnum.Kick:
                case OffenceModeEnum.Cpu:
                    if (offenceMode == OffenceModeEnum.Cpu)
                    {
                        int i = 0;
                        int currentQ = game.CurrentQuarter;
                        while (i < 1000 &&
                            game.CurrentPlayer == 2 &&
                            game.CurrentQuarter == currentQ &&
                            game.RemainSeconds > 0)
                        {
                            i++;
                            offenceMode = _AIThink(game);
                            _Offence(offenceMode, game);
                            if (!isAIAuto) break;
                        }
                    }
                    else
                    {
                        _Offence(offenceMode, game);
                    }
                    _context.Update(game);
                    await _context.SaveChangesAsync();
                    break;

                case OffenceModeEnum.Empty:
                case OffenceModeEnum.Initialize:
                default:
                    if (game != null && offenceMode == OffenceModeEnum.Initialize)
                    {
                        _context.Remove(game);
                        game = null;
                    }
                    if (game == null)
                    {
                        game = _InitializeGame();
                        game.Player1Id = player1Id;
                        game.Player2Id = player2Id;
                        _context.Update(game);
                        await _context.SaveChangesAsync();
                    }
                    break;
            }
            ameFootVM = Mapper.Map<AmeFootViiewModel>(game);
            //ameFootVM.IsAIAuto = isAIAuto;
            return View(ameFootVM);
        }

        /// <summary>
        /// AI思考
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        [NonAction]
        private OffenceModeEnum _AIThink(Game game)
        {
            if (game.RemainOffenceNum >= 2)
            {
                var probablity = new List<ValueTuple<int, OffenceModeEnum>>()
                    {
                        (1, OffenceModeEnum.Run),
                        (1, OffenceModeEnum.ShortPass),
                        (1, OffenceModeEnum.LongPass),
                    };
                return _GetRandamValue<OffenceModeEnum>(probablity);
            }
            else
            {
                bool shortDistance = game.RemainYards <= 24;
                var probablity = new List<ValueTuple<int, OffenceModeEnum>>()
                {
                    (1, OffenceModeEnum.Run),
                };
                if (shortDistance)
                {
                    probablity.Add((1, OffenceModeEnum.ShortPass));
                    probablity.Add((1, OffenceModeEnum.Kick));
                }
                else
                {
                    probablity.Add((1, OffenceModeEnum.LongPass));
                    probablity.Add((1, OffenceModeEnum.Pant));
                }
                return _GetRandamValue<OffenceModeEnum>(probablity);
            }
        }

        /// <summary>
        /// ランダム値取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prms"></param>
        /// <returns></returns>
        [NonAction]
        private T _GetRandamValue<T>(List<(int probablity, T value)> prms)
        {
            var sum = prms.Select(p => p.probablity).Sum();
            int randam = new Random().Next(1, sum + 1);
            int count = 0;
            foreach (var prm in prms)
            {
                count += prm.probablity;
                if (count >= randam)
                {
                    return (prm.value);
                }
            }
            return prms[prms.Count() - 1].value;
        }

        /// <summary>
        /// 攻撃ルーチン
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        [NonAction]
        private void _Offence(OffenceModeEnum mode, Game game)
        {
            if (game.CurrentQuarter == Const.QUARTERS && game.RemainSeconds <= 0) return;

            var resultStr = new StringBuilder();

            var result = _IsOffenceSuccess(mode, game.RemainYards);
            var modeData = mode.GetOffenceModeData();

            if (result.gain > game.RemainYards) result.gain = game.RemainYards;
            if (game.RemainYards - result.gain > Const.ALL_YARDS) result.gain = game.RemainYards - Const.ALL_YARDS;

            game.RemainYards -= result.gain;
            game.GainYards += result.gain;
            game.RemainSeconds -= modeData.seconds;
            game.RemainOffenceNum = game.RemainOffenceNum - 1;

            resultStr.Append($"{modeData.method}{result.result}。");

            (bool flg, decimal remainYards) changeData = (false, 0);
            resultStr.Append($"{result.gain:0} ヤードゲイン。");

            // インターセプト
            if (result.intercept)
            {
                resultStr.Append("インターセプト。");
                changeData = (true, game.RemainYards);
            }
            // タッチダウン
            else if (game.RemainYards <= 0)
            {
                resultStr.Append($"{(mode != OffenceModeEnum.Kick ? "タッチダウン。" : "")}");

                int addScore = mode == OffenceModeEnum.Kick ? 3 : 7;
                if (game.CurrentPlayer == 1)
                {
                    game.P1Score += addScore;
                }
                else
                {
                    game.P2Score += addScore;
                }
                changeData = (true, 10);
            }

            // パント
            else if (mode == OffenceModeEnum.Pant)
            {
                changeData = (true, game.RemainYards);
            }

            // 攻撃成功
            else if (game.GainYards >= 10)
            {
                game.GainYards = 0;
                game.RemainOffenceNum = 4;
                resultStr.Append($"ファーストダウン。");
            }

            // 攻撃失敗
            else if (game.RemainOffenceNum <= 0)
            {
                changeData = (true, game.RemainYards);
            }

            resultStr.Append($"{modeData.seconds}秒経過。");

            // タイムアップ
            if (game.RemainSeconds <= 0)
            {
                // クオーターチェンジ
                if (game.CurrentQuarter < Const.QUARTERS)
                {
                    resultStr.Append($"{game.CurrentQuarter}Q終了。");
                    game.CurrentQuarter++;
                    game.CurrentPlayer = game.CurrentQuarter % 2 == 0 ? 2 : 1;
                    string newPlayer = game.CurrentPlayer == 1 ? game.Player1Id : game.Player2Id;
                    _ResetOffenceData(game, 10);
                    game.RemainSeconds = Const.QUARTER_SECONDS;
                    resultStr.Append($"＞{newPlayer}");
                }
                // ゲームセット
                else
                {
                    resultStr.Append($"ゲームセット。");
                    game.RemainSeconds = 0;
                    var cmpScore = game.P1Score.CompareTo(game.P2Score);
                    if (cmpScore == 0)
                    {
                        resultStr.Append($"ドロー。");
                    }
                    else
                    {
                        resultStr.Append($"{(cmpScore > 0 ? game.Player1Id : game.Player2Id)}の勝ち。");
                    }
                }
            }

            // チェンジフラグ
            else if (changeData.flg)
            {
                game.CurrentPlayer = game.CurrentPlayer == 1 ? 2 : 1;
                string newPlayer = game.CurrentPlayer == 1 ? game.Player1Id : game.Player2Id;
                resultStr.Append($"チェンジ。＞{newPlayer}");
                _ResetOffenceData(game, changeData.remainYards);
            }

            game.Result = resultStr.ToString();
        }

        /// <summary>
        /// 攻撃成功判定
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        [NonAction]
        private (decimal gain, string result, bool intercept) _IsOffenceSuccess(OffenceModeEnum mode, decimal remainYards)
        {
            int result = new Random().Next(0, 100);
            int resultIc;
            bool boastPass = remainYards >= (Const.ALL_YARDS / 2);
            List<int> percents;
            List<decimal> gains;
            bool interCept = false;
            bool interCeptUse = false;

            switch (mode)
            {
                // ラン
                case OffenceModeEnum.Run:
                    percents = new List<int>() { 10, 50, 95 };
                    gains = new List<decimal>() { 10, 5, 1, -1 };
                    break;

                // ショートパス
                case OffenceModeEnum.ShortPass:
                    gains = new List<decimal>() { 20, 10, 0, -1 };
                    if (boastPass)
                    {
                        percents = new List<int>() { 5, 40, 98 };
                    }
                    else
                    {
                        percents = new List<int>() { 5, 25, 95 };
                    }
                    interCeptUse = true;

                    break;

                // ロングパス
                case OffenceModeEnum.LongPass:
                    gains = new List<decimal>() { 40, 20, 0, -5 };
                    if (boastPass)
                    {
                        percents = new List<int>() { 2, 25, 90 };
                    }
                    else
                    {
                        percents = new List<int>() { 2, 15, 85 };
                    }
                    interCeptUse = true;

                    break;

                // パント
                case OffenceModeEnum.Pant:
                    return (20, "実行", false);

                // キック
                case OffenceModeEnum.Kick:
                    var kickProbability = remainYards >= 25 ? 20 : 90;
                    percents = new List<int>() { 0, kickProbability, 100 };
                    gains = new List<decimal>() { 0, remainYards, -10, 0 };
                    break;

                default:
                    throw new ArgumentException("OffenceModeが不正");
            }

            if (percents[0] > result)
            {
                return (gains[0], "大成功", false);
            }
            else if (percents[1] > result)
            {
                return (gains[1], "成功", false);
            }
            else if (percents[2] > result)
            {
                // インターセプト判定
                if (interCeptUse)
                {
                    resultIc = new Random().Next(0, 10);
                    interCept = resultIc <= 0;
                }

                return (gains[2], "失敗", interCept);
            }
            else
            {
                if (interCeptUse)
                {
                    resultIc = new Random().Next(0, 5);
                    interCept = resultIc <= 0;
                }

                return (gains[3], "大失敗", interCept);
            }

            //return result < percent;
        }

        /// <summary>
        /// Game初期化
        /// </summary>
        /// <returns></returns>
        [NonAction]
        private Game _InitializeGame()
        {
            var game = new Game()
            {
                P1Score = 0,
                P2Score = 0,
                RemainSeconds = Const.QUARTER_SECONDS,
                CurrentQuarter = 1,
                CurrentPlayer = 1,
            };
            _ResetOffenceData(game, 10);
            return game;
        }

        /// <summary>
        /// OffenceDataリセット
        /// </summary>
        /// <returns></returns>
        [NonAction]
        private void _ResetOffenceData(Game game, decimal remainYards)
        {
            game.RemainYards = Const.ALL_YARDS - remainYards;
            game.GainYards = 0;
            game.RemainOffenceNum = 4;
        }

        /// <summary>
        /// Game取得
        /// </summary>
        /// <returns></returns>
        [NonAction]
        private async Task<Game> _GetGameAsync(string player1Id, string player2Id)
        {
            var game = await _context.Game.Where(x =>
            x.Player1Id == player1Id && x.Player2Id == player2Id)
            .SingleOrDefaultAsync();
            if (game == null)
            {
                game = _InitializeGame();
                game.Player1Id = player1Id;
                game.Player2Id = player2Id;
                _context.Add(game);
                await _context.SaveChangesAsync();
            }
            return game;
        }

        #endregion 作成中

        #region ごみ

        //    // GET: AmeFoot/Details/5
        //    public ActionResult Details(int id)
        //    {
        //        return View();
        //    }

        //    // GET: AmeFoot/Create
        //    public ActionResult Create()
        //    {
        //        return View();
        //    }

        //    // POST: AmeFoot/Create
        //    [HttpPost]
        //    [ValidateAntiForgeryToken]
        //    public ActionResult Create(IFormCollection collection)
        //    {
        //        try
        //        {
        //            // TODO: Add insert logic here

        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch
        //        {
        //            return View();
        //        }
        //    }

        //    // GET: AmeFoot/Edit/5
        //    public ActionResult Edit(int id)
        //    {
        //        return View();
        //    }

        //    // POST: AmeFoot/Edit/5
        //    [HttpPost]
        //    [ValidateAntiForgeryToken]
        //    public ActionResult Edit(int id, IFormCollection collection)
        //    {
        //        try
        //        {
        //            // TODO: Add update logic here

        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch
        //        {
        //            return View();
        //        }
        //    }

        //    // GET: AmeFoot/Delete/5
        //    public ActionResult Delete(int id)
        //    {
        //        return View();
        //    }

        //    // POST: AmeFoot/Delete/5
        //    [HttpPost]
        //    [ValidateAntiForgeryToken]
        //    public ActionResult Delete(int id, IFormCollection collection)
        //    {
        //        try
        //        {
        //            // TODO: Add delete logic here

        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch
        //        {
        //            return View();
        //        }
        //    }
        //}

        #endregion ごみ
    }
}