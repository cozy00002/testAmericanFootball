using System;
using System.Collections.Generic;
using TestAmericanFootball2.Enums;

namespace TestAmericanFootball2.Extentions
{
    public static class OffenceModeEnumExtentions
    {
        public static (string method, int seconds) GetOffenceModeData(this OffenceModeEnum value)
        {
            var dic = new Dictionary<OffenceModeEnum, ValueTuple<string, int>>()
            {
                { OffenceModeEnum.Run, ("ラン", 15) },
                { OffenceModeEnum.ShortPass,("ショートパス" ,5)},
                { OffenceModeEnum.LongPass,("ロングパス",7) },
                { OffenceModeEnum.Pant,("パント" ,0)},
                { OffenceModeEnum.Kick,("キック",0) },
                { OffenceModeEnum.Gamble,("ギャンブル",5) },
                { OffenceModeEnum.Cpu,("コンピューター",0) },
            };

            return dic[value];
        }
    }
}