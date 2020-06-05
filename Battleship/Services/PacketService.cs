
using Battleship.Common;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;


namespace Battleship.Services
{
    /**
     * Abstraction layer for communication with the other side (Client/Server).
     */
    public class PacketService
    {
        /**
         * Sends the provided packet using the provided client.
         * 
         * \parem packet        Packet to be sent.
         * \param networkClient Client to be used for sending.
         * \param errHandler    Hanler to be called if not successfull.
         */
        public static void SendPacket(Packet packet, TcpClient networkClient, Action errHandler)
        {
            try
            {
                var stream = networkClient.GetStream();
                var jsonBuffer = Encoding.UTF8.GetBytes(packet.ToString());
                var lengthBuffer = BitConverter.GetBytes(Convert.ToUInt16(jsonBuffer.Length));
                var messageBuffer = new byte[jsonBuffer.Length + lengthBuffer.Length];

                lengthBuffer.CopyTo(messageBuffer, 0);
                jsonBuffer.CopyTo(messageBuffer, lengthBuffer.Length);

                stream.Write(messageBuffer, 0, messageBuffer.Length);
            }
            catch (Exception ex)
            {
                Logger.LogE($"Send packet failed with the message '{ex.Message}'.");

                errHandler();
            }
        }

        /**
         * Recieves packet from the provided client.
         * 
         * \param networkClient Client to be used for sending.
         * \param errHandler    Hanler to be called if not successfull.
         * \return  The recieved packet.
         */
        public static Packet ReceivePacket(TcpClient client, Action errHandler)
        {
            if (client.Available == 0)
            {
                return null;
            }

            var stream = client.GetStream();
            var lengthBuffer = new byte[2];

            // Read the lengths first (we use 2B uint)
            stream.Read(lengthBuffer, 0, 2);

            // Get the byte lengths of the packet
            var packetByteSize = BitConverter.ToUInt16(lengthBuffer, 0);
            var jsonBuffer = new byte[packetByteSize];

            try
            {
                stream.Read(jsonBuffer, 0, jsonBuffer.Length);
            }
            // Handle error with provided handler
            catch (Exception ex)
            {
                Logger.LogE($"Reading packet failed with the message '{ex.Message}'.");
                errHandler();
            }

            // Get JSON string from it
            var jsonString = Encoding.UTF8.GetString(jsonBuffer);

            // Try deserialize it
            Packet resPacket = new Packet(PacketType.ERROR, "Error recieving a packet.");
            try
            {
                resPacket = JsonConvert.DeserializeObject<Packet>(jsonString);
            }
            // Handle error with provided handler
            catch (Exception ex)
            {
                Logger.LogE($"Recieve packet failed with the message '{ex.Message}'.");
                errHandler();
            }

            return resPacket;
        }
    }
}
