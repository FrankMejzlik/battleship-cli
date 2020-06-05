using Battleship.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Battleship.Logic
{
    public interface ILogic
    {
        /** This should gracefully terminate the logic instance. */
        public void Shutdown();

        /** It shots at the enemy. */
        void FireAt(int x, int y);

        /** Place the ship at the provided coordinates. */
        void PlaceShip(int x, int y, Ship ship);

        public void PlaceShips();

        /** Flag showing if the instance should be still running. */
        public bool ShouldRun { get; set; }
    }
}
