
using Newtonsoft.Json;

namespace Battleship.Common
{
    /** Unit of communication between the server and the client. */
    public class Packet
    {
        /** Constructs packet of the given type and containing the provided data. */
        public Packet(PacketType type, string data = "")
        {
            Type = type;
            Data = data;
        }

        /** Serializes the object into the JSON string. */
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /*
         * Member variables.
         */

        /** Type of this packet - determines what the partner is saying. */
        public PacketType Type { get; set; }

        /** Data it holds. */
        public string Data { get; set; }
    }
}