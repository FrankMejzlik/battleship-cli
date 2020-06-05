
using System;

namespace Battleship
{
    /** General utility functions. */
    public static class Utils
    {
        /** Converts the provided xy integer coordinates to Excel coordinates. */
        public static string ToExcelCoords(int x, int y)
        {
            ++x;
            string str = string.Empty;
            while (x > 0)
            {
                str = ALPHABET[(x - 1) % 26] + str;
                x /= 26;
            }

            // <LETTER from x><y + 1>
            return  str + (y + 1).ToString();
        }

        /** Converts the provided Excel coordinates to integer xy coordinates. */
        public static (int, int) FromExcelCoords(string coordinates)
        {
            string first = string.Empty;
            string second = string.Empty;

            CharEnumerator it = coordinates.GetEnumerator();
            while (it.MoveNext())
            {
                if (char.IsLetter(it.Current))
                {
                    first += it.Current;
                }
                else
                {
                    second += it.Current;
                }
            }

            int i = 0;
            it = first.GetEnumerator();
            while (it.MoveNext())
            {
                i = (26 * i) + ALPHABET.IndexOf(it.Current);
            }

            return (i, int.Parse(second) - 1);
        }

        /** Alphabet for Excel coordinates conversion */
        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    }
}
