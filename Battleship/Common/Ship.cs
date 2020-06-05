
using System.Collections.Generic;

namespace Battleship.Common
{
    public class Ship
    {
        public List<Field> Fields { get; set; } = new List<Field>();
        public bool IsSunk { get; set; } = false;
    }
}
