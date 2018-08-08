using System;

namespace TestAmericanFootball2.Enums
{
    public static class EnumHelper
    {
        /// <summary>
        /// Converts string to enum value (opposite to Enum.ToString()).
        /// </summary>
        /// <typeparam name="T">Type of the enum to convert the string into.</typeparam>
        /// <param name="s">string to convert to enum value.</param>
        public static T ToEnum<T>(this string s) where T : struct
        {
            T newValue;
            return Enum.TryParse(s, out newValue) ? newValue : default(T);
        }
    }
}