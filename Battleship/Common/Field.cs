
using System;

namespace Battleship.Common
{
    public class Field
    {
        /*
         * Methods
         */
        public Field(int x, int y)
        {
            X = x;
            Y = y;
            Coords = Utils.ToExcelCoords(x, y);
        }

        /*
         * Member variables
         */
        public int X { get; }
        public int Y { get; }
        public string Coords { get; set; }
        public bool IsRevealed { get; set; } = false;
        public bool IsShip { get; set; } = false;
    }
}