using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
        private const int QUARTER_SECONDS = 900;
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

                case "パス":
                    _Offence(OffenceModeEnum.Pass, game);
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
            if (game.CurrentQuarter == 4 && game.RemainSeconds <= 0) return;

            decimal gain = 0;
            int seconds = 0;
            var result = new StringBuilder();

            bool isSuccess = _IsOffenceSuccess(mode);
            switch (mode)
            {
                case OffenceModeEnum.Run:
                    gain = isSuccess ? 5 : 1;
                    seconds = 20;

                    break;

                case OffenceModeEnum.Pass:
                    gain = isSuccess ? 20 : 0;
                    seconds = 5;
                    break;
            }
            game.RemainYards -= gain;
            game.GainYards += gain;
            game.RemainSeconds -= seconds;
            game.RemainOffenceNum = game.RemainOffenceNum - 1;
            //game.RemainOffenceNum = isSuccess ? 4 : game.RemainOffenceNum - 1;

            result.Append($"{(isSuccess ? "成功" : "失敗")}。");
            result.Append($"{gain}ヤードゲイン。");
            result.Append($"{seconds}秒経過。");

            // タッチダウン
            if (game.RemainYards <= 0)
            {
                result.Append($"タッチダウン。");

                game.P1Score += game.CurrentPlayer == 1 ? 7 : 0;
                game.P2Score += game.CurrentPlayer == 2 ? 7 : 0;
                game.CurrentPlayer = game.CurrentPlayer == 1 ? 2 : 1;
                _ResetOffenceData(game);
            }
            // 攻撃成功
            else if (game.GainYards >= 10)
            {
                game.GainYards = 0;
                game.RemainOffenceNum = 4;
                result.Append($"攻撃成功。攻撃回数が4にリセットされます。");
            }
            // チェンジ
            else if (game.RemainOffenceNum <= 0)
            {
                game.CurrentPlayer = game.CurrentPlayer == 1 ? 2 : 1;
                _ResetOffenceData(game);
                string newPlayer = game.CurrentPlayer == 1 ? game.Player1Id : game.Player2Id;
                result.Append($"チェンジ。＞{newPlayer}");
            }
            // タイムアップ
            else if (game.RemainSeconds <= 0)
            {
                // クオーターチェンジ
                if (game.CurrentQuarter < 4)
                {
                    result.Append($"{game.CurrentQuarter}Q終了。");
                    game.CurrentQuarter++;
                    game.CurrentPlayer = game.CurrentQuarter % 2 == 0 ? 2 : 1;
                    string newPlayer = game.CurrentPlayer == 1 ? game.Player1Id : game.Player2Id;
                    _ResetOffenceData(game);
                    game.RemainSeconds = QUARTER_SECONDS;
                    result.Append($"＞{newPlayer}");
                }
                // ゲームセット
                else
                {
                    result.Append($"ゲームセット。");
                    game.RemainSeconds = 0;
                    var cmpScore = game.P1Score.CompareTo(game.P2Score);
                    if (cmpScore == 0)
                    {
                        result.Append($"ドロー。");
                    }
                    else
                    {
                        result.Append($"{(cmpScore > 0 ? game.Player1Id : game.Player2Id)}の勝ち。");
                    }
                }
            }

            game.Result = result.ToString();
        }

        /// <summary>
        /// 攻撃成功判定
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        [NonAction]
        private bool _IsOffenceSuccess(OffenceModeEnum mode)
        {
            int result;
            switch (mode)
            {
                case OffenceModeEnum.Run:
                    result = new Random().Next(0, 2);
                    return result == 0;

                case OffenceModeEnum.Pass:
                    result = new Random().Next(0, 4);
                    return result == 0;
            }
            return true;
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
                RemainSeconds = QUARTER_SECONDS,
                CurrentQuarter = 1,
                CurrentPlayer = 1,
            };
            _ResetOffenceData(game);
            return game;
        }

        /// <summary>
        /// OffenceDataリセット
        /// </summary>
        /// <returns></returns>
        [NonAction]
        private void _ResetOffenceData(Game game)
        {
            game.RemainYards = 100;
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