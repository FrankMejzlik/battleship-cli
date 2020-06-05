
using Battleship.Common;
using Battleship.Logic;

namespace Battleship.UI
{
    /** Common interface for each UI for the Battleship game. 
     * 
     * Currently we support only command line interface but 
     * e.g. WinForms variant can be easily added.
     * 
     * \see CmdUi
     */
    public interface IUi
    {

        /** Returns true if the UI is in interstate. */
        public bool IsInInterstate { get; }

        /** Launchg the UI. */
        public void Launch();

        /** Shuts down the UI with all the cleaning. */
        public void Shutdown();

        /** Changes state of the UI to provided state. */
        public void GotoState(UiState state, string msg = "");

        /** Sets the reference to the server this UI is working with. */
        public void SetLogic(ILogic s);

        /** Handle event that we HIT the enemy at the given coordinates. */
        public void HandleHitHimAt(int x, int y);

        /** Handle event that we MISS the enemy at the given coordinates. */
        public void HandleMissHimtAt(int x, int y);

        /** Handle event that the enemy HIT at the given coordinates. */
        public void HandleHitMe(int x, int y);

        /** Handle event that the enemy MISSED at the given coordinates. */
        public void HandleMissedMe(int x, int y);

        /** Handle event of placing the ship. */
        public void HandlePlaceShipAt(int x, int y);
    }
}
