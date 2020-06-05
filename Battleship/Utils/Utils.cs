using Battleship.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship
{
    public static class Utils
    {
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

        public static (int, int) FromExcelCoords(string coordinates)
        {
            string first = string.Empty;
            string second = string.Empty;

            CharEnumerator ce = coordinates.GetEnumerator();
            while (ce.MoveNext())
            {
                if (char.IsLetter(ce.Current))
                {
                    first += ce.Current;
                }
                else
                {
                    second += ce.Current;
                }
            }

            int i = 0;
            ce = first.GetEnumerator();
            while (ce.MoveNext())
            {
                i = (26 * i) + ALPHABET.IndexOf(ce.Current);
            }

            return (i, int.Parse(second) - 1);
        }


        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    }
}
