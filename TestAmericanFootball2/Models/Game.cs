namespace TestAmericanFootball2.Models
{
    /// <summary>
    /// ゲーム
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Player1Id
        /// </summary>
        public string Player1Id { get; set; }

        /// <summary>
        /// Player2Id
        /// </summary>
        public string Player2Id { get; set; }

        /// <summary>
        /// 現在のクオーター
        /// </summary>
        public int CurrentQuarter { get; set; }

        /// <summary>
        /// 残り秒数
        /// </summary>
        public int RemainSeconds { get; set; }

        /// <summary>
        /// 残りヤード数
        /// </summary>
        public decimal RemainYards { get; set; }

        /// <summary>
        /// 獲得ヤード数
        /// </summary>
        public decimal GainYards { get; set; }

        /// <summary>
        /// 残り攻撃回数
        /// </summary>
        public int RemainOffenceNum { get; set; }

        /// <summary>
        /// P1スコア
        /// </summary>
        public int P1Score { get; set; }

        /// <summary>
        /// P2スコア
        /// </summary>
        public int P2Score { get; set; }

        /// <summary>
        /// 現在攻撃中のプレイヤー
        /// </summary>
        public int CurrentPlayer { get; set; }

        /// <summary>
        /// 攻撃結果
        /// </summary>
        public string Result { get; set; }
    }
}