using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship
{
    public static class Utils
    {
        public static string GetCoords(int x, int y)
        {
            var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            return $"{alphabet[x]}{y + 1}";
        }
    }
}
