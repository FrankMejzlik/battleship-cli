﻿
using Newtonsoft.Json;

namespace Battleship.Models
{
    public class Packet
    {
        public ePacketType Type { get; set; }
        public string Data { get; set; }

        public Packet(ePacketType type, string data = "")
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