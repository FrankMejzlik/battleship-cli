
using Battleship.Common;

namespace Battleship.Logic
{
    /** Common interface for both Server and Client. */
    public interface ILogic
    {
        /** This should gracefully terminate the logic instance. */
        public void Shutdown();

        /** It shots at the enemy. */
        public void FireAt(int x, int y);

        /** Place the ship at the provided coordinates. */
        public void PlaceShip(int x, int y, Ship ship);

        /** This is final command that terminates the placing ships stage */
        public void PlaceShips();

        /** Flag showing if the instance should be still running. */
        public bool ShouldRun { get; set; }
    }
}
