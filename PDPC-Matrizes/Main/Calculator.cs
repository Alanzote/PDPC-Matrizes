using PDPC_Matrizes.Generic;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace PDPC_Matrizes.Main {

    /**
     * The Calculator.
     * Calculates a portion of a matrix.
     */
    internal static class Calculator {

        // Our Tcp Client.
        private static TcpClient Tcp;

        // The Main Method for the Calculator.
        public static void Main(string[] args) {
            // Try...
            try {
                // To Connect...
                Tcp = new TcpClient(IPAddress.Loopback.ToString(), Messaging.TcpPort);
            } catch (Exception ex) {
                // If it fails, notify.
                Console.WriteLine($"Failed to Connect to Matrix Server: {ex.Message}.");
            }

            // Notify.
            Console.WriteLine("Connected, awaiting server information...");

            // Wait for the Matrix Information.
            while (Tcp.Available <= 0) { }

            // Get Packet.
            BinaryReader Packet = Messaging.ReceivePacket(Tcp);

            // Get Matrix A and B.
            Matrix MatrixA = new Matrix(Packet);
            Matrix MatrixB = new Matrix(Packet);

            // Get Start and End Indices.
            int StartIndex = Packet.ReadInt32();
            int EndIndex = Packet.ReadInt32();
            int MatrixC = Packet.ReadInt32();

            // Close Packet.
            Packet.Close();

            // Notify.
            Console.WriteLine($"Received Request for Matrix Calculation with {StartIndex} sidx and {EndIndex} eidx.");

            // Create our Result Array.
            double[] Results = new double[EndIndex - StartIndex];

            // Loop...
            for (int i = 0; i < Results.Length; i++) {
                // Calculate Real I.
                int Ri = i + StartIndex;

                // Calculate Matrix Indices.
                int Cx = Ri % MatrixC;
                int Cy = Ri / MatrixC;

                // Reset Value.
                Results[i] = 0;

                // Calculate.
                for (int v = 0; v < MatrixC; v++)
                    Results[i] += MatrixA[Cx, v] * MatrixB[v, Cy];
            }

            // Notify.
            Console.WriteLine("Calculation Complete, sending data...");

            // Create Result Packet.
            BinaryWriter Result = Messaging.CreatePacket();

            // Write the Start Index and End Index.
            Result.Write(StartIndex);
            Result.Write(EndIndex);

            // Write the Values...
            for (int v = 0; v < Results.Length; v++)
                Result.Write(Results[v]);

            // Send the Packet.
            Messaging.SendPacket(Tcp, Result);

            // Notify.
            Console.WriteLine("Data send complete, operation successful.");

            // Stop the Client.
            Tcp.Close();
        }
    }
}
