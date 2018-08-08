using System;

namespace TestAmericanFootball2.Extentions
{
    public static class IntExtentions
    {
        public static string ConvertMinSec(this int value)
        {
            var time = new TimeSpan(0, 0, 0, value, 0);
            return $"{Math.Floor(time.TotalMinutes)}分{time.Seconds}秒";
        }
    }
}