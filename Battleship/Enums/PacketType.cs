using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Enums
{
    public enum PacketType
    {
        /// <summary>
        /// Something went wrong
        /// </summary>
        Error = 0,

        /// <summary>
        /// Send message to the enemy
        /// </summary>
        Message = 1,

        /// <summary>
        /// Send client ships configuration to the server
        /// </summary>
        SetClientShips = 2,

        /// <summary>
        /// Fire to the enemy field
        /// </summary>
        Fire = 3,

        /// <summary>
        /// Response after fire to the enemy field
        /// </summary>
        FireResponse = 4,
    }
}
