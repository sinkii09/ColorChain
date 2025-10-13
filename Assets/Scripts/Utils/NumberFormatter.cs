using System;

namespace ColorChain.Utils
{
    /// <summary>
    /// Utility class for formatting numbers with K, M, B suffixes
    /// </summary>
    public static class NumberFormatter
    {
        /// <summary>
        /// Formats a number with K (thousand), M (million), B (billion) suffixes
        /// </summary>
        /// <param name="number">The number to format</param>
        /// <param name="decimals">Number of decimal places (default: 1)</param>
        /// <returns>Formatted string (e.g., "1.5K", "12.3M", "2.1B")</returns>
        public static string FormatNumber(int number, int decimals = 1)
        {
            return FormatNumber((long)number, decimals);
        }

        /// <summary>
        /// Formats a long number with K, M, B, T suffixes
        /// </summary>
        public static string FormatNumber(long number, int decimals = 1)
        {
            if (number == 0)
                return "0";

            // Handle negative numbers
            bool isNegative = number < 0;
            number = Math.Abs(number);

            string result;

            if (number >= 1_000_000_000_000) // Trillion
            {
                result = FormatWithSuffix(number, 1_000_000_000_000, "T", decimals);
            }
            else if (number >= 1_000_000_000) // Billion
            {
                result = FormatWithSuffix(number, 1_000_000_000, "B", decimals);
            }
            else if (number >= 1_000_000) // Million
            {
                result = FormatWithSuffix(number, 1_000_000, "M", decimals);
            }
            else if (number >= 1_000) // Thousand
            {
                result = FormatWithSuffix(number, 1_000, "K", decimals);
            }
            else
            {
                result = number.ToString();
            }

            return isNegative ? "-" + result : result;
        }

        private static string FormatWithSuffix(long number, long divisor, string suffix, int decimals)
        {
            double value = (double)number / divisor;

            // If the value is a whole number, don't show decimals
            if (Math.Abs(value - Math.Floor(value)) < 0.01)
            {
                return ((long)value).ToString() + suffix;
            }

            // Otherwise, show with specified decimal places
            string format = decimals > 0 ? $"F{decimals}" : "F0";
            return value.ToString(format) + suffix;
        }
    }
}
