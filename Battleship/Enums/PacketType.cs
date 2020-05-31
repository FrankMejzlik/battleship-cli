using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Enums
{
    public enum PacketType
    {
        ERROR = 0,

        MESSAGE = 1,

        SET_CLEINT_SHIPS = 2,

        FIRE = 3,

        FIRE_REPONSE = 4,

        YOUR_TURN = 5,
         
        OPPONENTS_TURN = 6,
        SetClientShips = 7
    }
}
