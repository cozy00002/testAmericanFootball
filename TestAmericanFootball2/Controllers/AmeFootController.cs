using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAmericanFootball2.Enums;
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

        // POST: AmeFoot
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Game(string player1Id,
                                             string player2Id,
                                             string method)
        {
            AmeFootViiewModel ameFootVM;
            Game game;
            game = await _context.Game.Where(x =>
            x.Player1Id == player1Id && x.Player2Id == player2Id)
            .SingleOrDefaultAsync();
            switch (method)
            {
                case "ラン":
                    _Offence(OffenceModeEnum.Run, game);
                    break;

                case "ショートパス":
                    _Offence(OffenceModeEnum.ShortPass, game);
                    break;

                case "ロングパス":
                    _Offence(OffenceModeEnum.LongPass, game);
                    break;

                case null:
                case "初期化":
                default:
                    if (game != null)
                    {
                        _context.Remove(game);
                    }
                    game = _InitializeGame();
                    game.Player1Id = player1Id;
                    game.Player2Id = player2Id;

                    break;
            }
            _context.Update(game);
            await _context.SaveChangesAsync();
            ameFootVM = Mapper.Map<AmeFootViiewModel>(game);
            return View(ameFootVM);
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

            int seconds = 0;
            var resultStr = new StringBuilder();
            string method = string.Empty;

            var result = _IsOffenceSuccess(mode, game.RemainYards);
            switch (mode)
            {
                case OffenceModeEnum.Run:
                    seconds = 15;
                    method = "ラン";
                    break;

                case OffenceModeEnum.ShortPass:
                    seconds = 5;
                    method = "ショートパス";
                    break;

                case OffenceModeEnum.LongPass:
                    seconds = 7;
                    method = "ロングパス";
                    break;
            }

            if (result.gain > 0)
            {
                game.RemainYards -= result.gain;
                game.GainYards += result.gain;
            }
            game.RemainSeconds -= seconds;
            game.RemainOffenceNum = game.RemainOffenceNum - 1;

            resultStr.Append($"{method}{result.result}。");
            if (result.gain > 0) resultStr.Append($"{result.gain} ヤードゲイン。");
            resultStr.Append($"{seconds}秒経過。");

            // タッチダウン
            if (game.RemainYards <= 0)
            {
                resultStr.Append($"タッチダウン。");

                game.P1Score += game.CurrentPlayer == 1 ? 7 : 0;
                game.P2Score += game.CurrentPlayer == 2 ? 7 : 0;
                game.CurrentPlayer = game.CurrentPlayer == 1 ? 2 : 1;
                _ResetOffenceData(game, 10);
            }
            // タイムアップ
            else if (game.RemainSeconds <= 0)
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
            // 攻撃成功
            else if (game.GainYards >= 10)
            {
                game.GainYards = 0;
                game.RemainOffenceNum = 4;
                resultStr.Append($"攻撃成功。攻撃回数が4にリセットされます。");
            }

            // チェンジ
            else if (game.RemainOffenceNum <= 0 || result.gain < 0)
            {
                string changeStr = (result.gain < 0) ? "インターセプト" : "チェンジ";
                game.CurrentPlayer = game.CurrentPlayer == 1 ? 2 : 1;
                _ResetOffenceData(game, game.RemainYards);
                string newPlayer = game.CurrentPlayer == 1 ? game.Player1Id : game.Player2Id;

                resultStr.Append($"{changeStr}＞{newPlayer}");
            }

            game.Result = resultStr.ToString();
        }

        /// <summary>
        /// 攻撃成功判定
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        [NonAction]
        private (decimal gain, string result) _IsOffenceSuccess(OffenceModeEnum mode, decimal remainYards)
        {
            int result = new Random().Next(0, 100);
            int percent = 0;
            bool boastPass = remainYards >= (Const.ALL_YARDS / 2);
            List<int> percents;
            List<decimal> gains;

            switch (mode)
            {
                case OffenceModeEnum.Run:
                    percent = 50;
                    percents = new List<int>() { 10, 50, 95 };
                    gains = new List<decimal>() { 10, 5, 2, 1 };
                    break;

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

                    percent = boastPass ? 40 : 20;
                    break;

                case OffenceModeEnum.LongPass:
                    gains = new List<decimal>() { 40, 20, 0, -1 };
                    if (boastPass)
                    {
                        percents = new List<int>() { 2, 25, 90 };
                    }
                    else
                    {
                        percents = new List<int>() { 2, 15, 85 };
                    }

                    percent = boastPass ? 30 : 15;
                    break;

                default:
                    throw new ArgumentException("OffenceModeが不正");
            }

            if (percents[0] > result)
            {
                return (gains[0], "大成功");
            }
            else if (percents[1] > result)
            {
                return (gains[1], "成功");
            }
            else if (percents[2] > result)
            {
                return (gains[2], "失敗");
            }
            else
            {
                return (gains[3], "大失敗");
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