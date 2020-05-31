using Battleship.Enums;
using Battleship.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Services
{
    public class PacketService
    {
        public static bool SendPacket(Packet packet, TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                var jsonBuffer = Encoding.UTF8.GetBytes(packet.ToString());
                var lengthBuffer = BitConverter.GetBytes(Convert.ToUInt16(jsonBuffer.Length));
                var messageBuffer = new byte[jsonBuffer.Length + lengthBuffer.Length];

                lengthBuffer.CopyTo(messageBuffer, 0);
                jsonBuffer.CopyTo(messageBuffer, lengthBuffer.Length);

                stream.Write(messageBuffer, 0, messageBuffer.Length);
                return true;
            }
            catch (Exception ex)
            {
                // TODO: log
                return false;
            }
        }

        public static Packet ReceivePacket(TcpClient client)
        {
            if (client.Available == 0)
            {
                return null;
            }

            var stream = client.GetStream();
            var lengthBuffer = new byte[2];

            stream.Read(lengthBuffer, 0, 2);

            var packetByteSize = BitConverter.ToUInt16(lengthBuffer, 0);
            var jsonBuffer = new byte[packetByteSize];

            stream.Read(jsonBuffer, 0, jsonBuffer.Length);

            var jsonString = Encoding.UTF8.GetString(jsonBuffer);
            
            try
            {
                var packet = JsonConvert.DeserializeObject<Packet>(jsonString);
                return packet;
            }
            catch (Exception ex)
            {
                // TODO: log    
                return new Packet(PacketType.ERROR, ex.Message);
            }            
        }
    }
}
