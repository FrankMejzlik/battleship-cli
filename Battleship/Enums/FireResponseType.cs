using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Enums
{
    public enum FireResponseType
    {
        /// <summary>
        /// There was no ship in the field
        /// </summary>        
        Water = 0,

        /// <summary>
        /// There was a ship in the field
        /// </summary>
        Hit = 1,

        /// <summary>
        /// There was a last part of the whole ship in the field
        /// </summary>
        HitAndSunk = 2
    }

    public static class FireResponseExtensions
    {
        public static string ToFriendlyString(this FireResponseType type)
        {
            switch (type)
            {
                case FireResponseType.Water:
                    return "WATER!";
                case FireResponseType.Hit:
                    return "HIT!";
                case FireResponseType.HitAndSunk:
                    return "HIT AND SUNK!";
                default:
                    return string.Empty;
            }
        }
    }
}
