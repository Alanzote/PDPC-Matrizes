using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace PDPC_Matrizes.Generic {

    /**
     * The Messaging Helper Class.
     * Helps Receiving and Sending Data.
     */
    internal static class Messaging {

        // The Port to Use.
        public static readonly int TcpPort = 7769;

        // Creates a Packet.
        public static BinaryWriter CreatePacket() {
            // Return Result.
            return new BinaryWriter(new MemoryStream());
        }

        // Converts a Packet from the Binary Writer into a Byte Array.
        public static byte[] ConvertPacket(BinaryWriter Writer) {
            // Reset Stream Position.
            Writer.BaseStream.Seek(0, SeekOrigin.Begin);

            // Get the Buffer.
            byte[] Buffer = new byte[Writer.BaseStream.Length];

            // Read to the Buffer.
            Writer.BaseStream.Read(Buffer, 0, Buffer.Length);

            // Return it.
            return Buffer;
        }
        
        // Converts a Packet from the Byte Array into a Binary Reader.
        public static BinaryReader ConvertPacket(byte[] Buffer) {
            // Simple, just create it!
            return new BinaryReader(new MemoryStream(Buffer));
        }

        // Sends a Tcp Packet.
        public static void SendPacket(TcpClient Client, BinaryWriter Writer) {
            // Convert the Previous Packet.
            byte[] Converted = ConvertPacket(Writer);

            // Create Actual Packet we will be sending.
            BinaryWriter NewPacket = CreatePacket();

            // Write the Length of our Packet and the Actual Data.
            NewPacket.Write(Converted.Length);
            NewPacket.Write(Converted);

            // Send it.
            Client.GetStream().Write(ConvertPacket(NewPacket));

            // Close Writers.
            Writer.Close();
            NewPacket.Close();
        }

        // Receives a Tcp Packet.
        public static BinaryReader ReceivePacket(TcpClient Client) {
            // Check for Data.
            if (Client.Available < 4)
                return null;

            // Get Buffer.
            List<byte> Buffer = new List<byte>();

            // Create a Buffer to Read the Length of a Packet.
            byte[] LenBuf = new byte[4];

            // Read the Length.
            Client.GetStream().Read(LenBuf, 0, LenBuf.Length);

            // Convert it to Int.
            int Length = BitConverter.ToInt32(LenBuf, 0);

            // While our Buffer hasn't reached that length...
            while (Buffer.Count < Length) {
                // Wait for data.
                while (Client.Available <= 0) { }

                // Create new Result Buffer.
                byte[] CurrentBuffer = new byte[Client.Available];

                // Read Data.
                Client.GetStream().Read(CurrentBuffer, 0, CurrentBuffer.Length);

                // Add to Result Buffer.
                Buffer.AddRange(CurrentBuffer);
            }

            // Return a Reader.
            return ConvertPacket(Buffer.ToArray());
        }
    }
}
