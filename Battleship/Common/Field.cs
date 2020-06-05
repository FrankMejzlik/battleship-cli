
namespace Battleship.Common
{
    /** Represents one field in the whole  playfield. */
    public class Field
    {
        public Field(int x, int y)
        {
            X = x;
            Y = y;
            Coords = Utils.ToExcelCoords(x, y);
        }

        /** X coord. */
        public int X { get; }

        /** Y coord. */
        public int Y { get; }

        /** Excel coords. */
        public string Coords { get; set; }

        /** If this field was shot at. */
        public bool IsRevealed { get; set; } = false;

        /** If this field represents a ship. */
        public bool IsShip { get; set; } = false;
    }
}