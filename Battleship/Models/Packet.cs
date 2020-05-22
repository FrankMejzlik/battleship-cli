using Battleship.Enums;
using Newtonsoft.Json;

namespace Battleship.Models
{
    public class Packet
    {
        public PacketType Type { get; set; }
        public string Data { get; set; }

        public Packet(PacketType type, string data = "")
        {
            Type = type;
            Data = data;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}