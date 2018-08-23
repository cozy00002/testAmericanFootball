using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAmericanFootball2.Enums;
using TestAmericanFootball2.Extentions;
using TestAmericanFootball2.Models;
using TestAmericanFootball2.Service.Interface;

namespace TestAmericanFootball2.Service
{
    /// <summary>
    /// GameService
    /// </summary>
    public class GameService : IGameService
    {
        private readonly AFDbContext _context;

        #region Init

        public GameService(AFDbContext context, IConfiguration config)
        {
            _context = context;
            // var myStringValue = config["MyStringKey"];
        }

        #endregion Init

        #region Method

        /// <summary>
        /// Game初期化
        /// </summary>
        /// <returns></returns>
        public Game InitializeGame()
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
        /// ゲームのメインルーチン
        /// </summary>
        /// <param name="player1Id"></param>
        /// <param name="player2Id"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public async Task<Game> GameAsync(
            string player1Id,
            string player2Id,
            string method)
        {
            if (string.IsNullOrEmpty(player2Id))
            {
                player2Id = Const.COM_NAME_DB;
            }

            bool isAIAuto = true;
            if (method == "コンピューター") { isAIAuto = false; }

            Game game = await _context.Game.Where(x =>
            x.Player1Id == player1Id && x.Player2Id == player2Id)
            .SingleOrDefaultAsync();
            OffenceModeEnum offenceMode = method.GetOffenceMode();
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
                        game = InitializeGame();
                        game.Player1Id = player1Id;
                        game.Player2Id = player2Id;
                        _context.Update(game);
                        await _context.SaveChangesAsync();
                    }
                    break;
            }

            return game;
        }

        #endregion Method

        #region Helper

        /// <summary>
        /// AI思考
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
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
        /// Get Gain Yards.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="resultCode"></param>
        /// <param name="remainYards"></param>
        /// <returns></returns>
        private decimal _getGain(OffenceModeEnum mode, int resultCode, decimal remainYards)
        {
            var dicGain = new Dictionary<int, decimal>();

            switch (mode)
            {
                // ラン
                case OffenceModeEnum.Run:
                    dicGain[0] = 10;
                    dicGain[1] = 5;
                    dicGain[2] = 2;
                    dicGain[3] = -1;

                    break;

                // ショートパス
                case OffenceModeEnum.ShortPass:
                    dicGain[0] = 20;
                    dicGain[1] = 10;
                    dicGain[2] = 0;
                    dicGain[3] = -1;
                    break;

                // ロングパス
                case OffenceModeEnum.LongPass:
                    dicGain[0] = 50;
                    dicGain[1] = 20;
                    dicGain[2] = 0;
                    dicGain[3] = -1;
                    break;

                // キック
                case OffenceModeEnum.Kick:
                    dicGain[0] = 0;
                    dicGain[1] = 50;
                    dicGain[2] = -10;
                    dicGain[3] = 0;
                    break;

                case OffenceModeEnum.Pant:
                default:
                    throw new ArgumentException($"OffenceModeが不正 mode:{mode}");
            }

            decimal retValue = dicGain[resultCode];
            return retValue > remainYards ? remainYards : retValue;
        }

        /// <summary>
        /// 攻撃ルーチン
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
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
        private (decimal gain, string result, bool intercept) _IsOffenceSuccess(OffenceModeEnum mode, decimal remainYards)
        {
            #region init valuables.

            int resultCode;
            bool boastPass = remainYards >= (Const.ALL_YARDS / 2);
            bool interCeptUse = false;

            var dicResult = new Dictionary<int, string>() {
                { 0, "大成功"},
                { 1, "成功"},
                { 2, "失敗"},
                { 3, "大失敗"},
            };
            var dicGain = new Dictionary<int, decimal>();

            #endregion init valuables.

            #region init randamValues.

            var runResult = _GetRandamValue(
                new List<(int probablity, int result)>(){
                    (10,0),
                    (30,1),
                    (55,2),
                    (55,3),
                });

            var spResult = _GetRandamValue(
                new List<(int probablity, int result)>(){
                    (5, 0),
                    (20,1),
                    (70,2),
                    (5, 3),
                });

            var spbResult = _GetRandamValue(
                new List<(int probablity, int result)>(){
                    (5, 0),
                    (35,1),
                    (58,2),
                    (2, 3),
                });

            var lpResult = _GetRandamValue(
                new List<(int probablity, int result)>(){
                    (2, 0),
                    (13,1),
                    (70,2),
                    (15,3),
                });

            var lpbResult = _GetRandamValue(
                new List<(int probablity, int result)>(){
                    (2, 0),
                    (23,1),
                    (65,2),
                    (10,3),
                });

            var kickProbability = remainYards >= 25 ? 20 : 90;
            var kkRresult = _GetRandamValue(
                new List<(int probablity, int result)>(){
                    (kickProbability,1),
                    (100 - kickProbability,2),
                });

            #endregion init randamValues.

            #region get resultCode and interceptUse by mode.

            switch (mode)
            {
                // ラン
                case OffenceModeEnum.Run:
                    resultCode = runResult;

                    break;

                // ショートパス
                case OffenceModeEnum.ShortPass:
                    if (boastPass)
                    {
                        resultCode = spbResult;
                    }
                    else
                    {
                        resultCode = spResult;
                    }
                    interCeptUse = true;
                    break;

                // ロングパス
                case OffenceModeEnum.LongPass:
                    if (boastPass)
                    {
                        resultCode = lpbResult;
                    }
                    else
                    {
                        resultCode = lpResult;
                    }
                    interCeptUse = true;
                    break;

                // パント
                case OffenceModeEnum.Pant:
                    return (20, "実行", false);

                // キック
                case OffenceModeEnum.Kick:
                    resultCode = kkRresult;
                    break;

                default:
                    throw new ArgumentException("OffenceModeが不正");
            }

            #endregion get resultCode and interceptUse by mode.

            #region get gains by resultCode.

            var gain = _getGain(mode, resultCode, remainYards);

            #endregion get gains by resultCode.

            return (gain, dicResult[resultCode], interCeptUse);
        }

        /// <summary>
        /// OffenceDataリセット
        /// </summary>
        /// <returns></returns>
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
        private async Task<Game> _GetGameAsync(string player1Id, string player2Id)
        {
            var game = await _context.Game.Where(x =>
            x.Player1Id == player1Id && x.Player2Id == player2Id)
            .SingleOrDefaultAsync();
            if (game == null)
            {
                game = InitializeGame();
                game.Player1Id = player1Id;
                game.Player2Id = player2Id;
                _context.Add(game);
                await _context.SaveChangesAsync();
            }
            return game;
        }

        #endregion Helper
    }
}