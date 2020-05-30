using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship
{
    public static class Utils
    {
        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";


        public static string GetCoords(int x, int y)
        {
            string str = string.Empty;
            while (x > 0)
            {
                str = ALPHABET[(x - 1) % 26] + str;
                x /= 26;
            }

            return str + y.ToString();
        }

        public static (int, int) ToNumericCoordinates(string coordinates)
        {
            string first = string.Empty;
            string second = string.Empty;

            CharEnumerator ce = coordinates.GetEnumerator();
            while (ce.MoveNext())
                if (char.IsLetter(ce.Current))
                    first += ce.Current;
                else
                    second += ce.Current;

            int i = 0;
            ce = first.GetEnumerator();
            while (ce.MoveNext())
                i = (26 * i) + ALPHABET.IndexOf(ce.Current) + 1;


            return (int.Parse(first), int.Parse(second));
        }
    }
}
