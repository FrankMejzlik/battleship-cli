
using System.Collections.Generic;

namespace Battleship.Common
{
    /** Represents one ship. */
    public class Ship
    {
        /** Fields this ship occupies. */
        public List<Field> Fields { get; set; } = new List<Field>();

        /** If this ship is down already. */
        public bool IsSunk { get; set; } = false;
    }
}
