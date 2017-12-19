using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Enums
    {
        public static class Chars
        {
            public static Char[] SpecialChar()
            {
                return new Char[] { '_', '-', '*', '"', '('};
            }
            public static Char[] WhiteSpace()
            {
                return new Char[] {' '};
            }
        }
        public static class RegexStrings
        {
            public const string Alpha = "^[a-zA-Z]+$";
            public const string AlphaNumeric = "^[a-zA-Z0-9]+$";
            public const string AlphaNumericUnderscore = "^[a-zA-Z0-9_]+$";
        }
    }
}
