﻿using System.ComponentModel.DataAnnotations;
using TestAmericanFootball2.Extentions;

namespace TestAmericanFootball2.ViewModels
{
    public class AmeFootViiewModel
    {
        private string _player2Id;

        /// <summary>
        /// Player1Id
        /// </summary>
        [Display(Name = "プレイヤー１")]
        public string Player1Id { get; set; }

        /// <summary>
        /// Player2Id
        /// </summary>
        [Display(Name = "プレイヤー２")]
        public string Player2Id
        {
            get
            {
                return _player2Id == Const.COM_NAME_DB ? Const.COM_NAME_DISPLAY : _player2Id;
            }
            set
            {
                _player2Id = value;
            }
        }

        /// <summary>
        /// 現在のクオーター
        /// </summary>
        [Display(Name = "クオーター")]
        public int CurrentQuarter { get; set; }

        /// <summary>
        /// 総クオーター
        /// </summary>
        public int TotalQuarter
        {
            get { return Const.QUARTERS; }
        }

        /// <summary>
        /// 残り時間
        /// </summary>
        [Display(Name = "残り時間")]
        public string RemainTime
        {
            get { return RemainSeconds.ConvertMinSec(); }
        }

        /// <summary>
        /// 残り時間
        /// </summary>
        public int RemainSeconds { get; set; }

        /// <summary>
        /// 残りヤード数
        /// </summary>
        [Display(Name = "残りヤード数")]
        public string RemainYards { get; set; }

        /// <summary>
        /// 獲得ヤード数
        /// </summary>
        public string GainYards { get; set; }

        /// <summary>
        /// ファーストダウンまでのヤード数
        /// </summary>
        [Display(Name = "ファーストダウンまであと")]
        public string Remain1stDownYards
        {
            get
            {
                return GainYards != null ? (10 - int.Parse(GainYards)).ToString() : "";
            }
        }

        /// <summary>
        /// 現在の攻撃回数
        /// </summary>
        [Display(Name = "残り攻撃回数")]
        public int RemainOffenceNum { get; set; }

        /// <summary>
        /// P1スコア
        /// </summary>
        [Display(Name = "P1スコア")]
        public int P1Score { get; set; }

        /// <summary>
        /// P2スコア
        /// </summary>
        [Display(Name = "P2スコア")]
        public int P2Score { get; set; }

        /// <summary>
        /// 現在攻撃中のプレイヤー
        /// </summary>
        public int CurrentPlayer { get; set; }

        /// <summary>
        /// 攻撃結果
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// AIオート実行
        /// </summary>
        [Display(Name = "CPU自動実行")]
        public bool IsAIAuto { get; set; }
    }
}