
using System;

namespace Battleship.Models
{
    public class Field
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Coords { get; set; }
        public bool IsRevealed { get; set; } = false;
        public bool IsShip { get; set; } = false;

        public Field(int x, int y)
        {
            X = x;
            Y = y;
            Coords = Utils.GetCoords(x, y);
        }
    }
}