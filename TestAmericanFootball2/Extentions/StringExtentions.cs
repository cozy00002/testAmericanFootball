using System.Collections.Generic;
using TestAmericanFootball2.Enums;

namespace TestAmericanFootball2.Extentions
{
    public static class StringExtentions
    {
        public static OffenceModeEnum GetOffenceMode(this string value)
        {
            string str = value ?? "";
            var dic = new Dictionary<string, OffenceModeEnum>()
            {
                { "ラン", OffenceModeEnum.Run },
                { "ショートパス", OffenceModeEnum.ShortPass },
                { "ロングパス", OffenceModeEnum.LongPass },
                { "パント", OffenceModeEnum.Pant },
                { "キック", OffenceModeEnum.Kick },
                { "初期化", OffenceModeEnum.Initialize },
                { "", OffenceModeEnum.Empty },
                { "コンピューター", OffenceModeEnum.Cpu },
                { "コンピューター(スキップ)", OffenceModeEnum.Cpu },
            };
            if (!dic.ContainsKey(str)) { throw new System.ArgumentException($"不正なOffenceMode。:{value}"); }
            return dic[str];
        }
    }
}