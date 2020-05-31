using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Models
{
    public class Ship
    {
        public List<Field> Fields { get; set; }
        public bool IsSunk { get; set; } = false;
    }
}
