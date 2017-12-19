using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Tools
    {
        public static bool IsAlpha(string input)
        {
            return Regex.IsMatch(input, Enums.RegexStrings.Alpha);
        }
        public static bool IsAlphaNumeric(string input)
        {
            return Regex.IsMatch(input, Enums.RegexStrings.AlphaNumeric);
        }
        public static bool IsAlphaNumericWithUnderscore(string input)
        {
            return Regex.IsMatch(input, Enums.RegexStrings.AlphaNumericUnderscore);
        }
        public static string TrimSpecialCharacters(string input)
        {
            if (String.IsNullOrEmpty(input))
                return "";

            int indexOf = input.IndexOfAny(Enums.Chars.SpecialChar());
            return (indexOf < 0) ? input : input.Remove(indexOf).Trim();
        }
        public static string TrimWhiteSpace(string input)
        {
            if (String.IsNullOrEmpty(input))
                return "";

            int indexOf = input.IndexOfAny(Enums.Chars.WhiteSpace());
            return (indexOf < 0) ? input : input.Remove(indexOf).Trim();
        }
    }
}
