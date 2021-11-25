using PDPC_Matrizes.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace PDPC_Matrizes.Main {

    /**
     * The Coordinator.
     * Coordinates the operation.
     */
    internal static class Coordinator {

        // Default Matrix Path.
        private static readonly string MatrixPath = "./Assets/Matrix{0}.csv";
        private static readonly string MatrixPathA = string.Format(MatrixPath, 'A');
        private static readonly string MatrixPathB = string.Format(MatrixPath, 'B');
        private static readonly string MatrixPathC = string.Format(MatrixPath, 'C');

        // Our Matrixes.
        private static Matrix MatrixA, MatrixB, MatrixC;

        // The Tcp Listener.
        private static TcpListener Listener;

        // Our List of Clients.
        private static List<TcpClient> ClientList = new List<TcpClient>();

        // The Amount of Clients.
        private static int AmountOfClients = 0;

        // The Main Method for the Coordinator.
        public static void Main(string[] args) {
            // Check Parameters.
            if (args.Length != 1)
                throw new ArgumentOutOfRangeException($"{nameof(args)} needs at least a single argument specifying amount of Clients to wait for.");

            // Read Amount of Clients.
            AmountOfClients = int.Parse(args[0]);

            // Read Matrix A and B.
            MatrixA = new Matrix(MatrixPathA);
            MatrixB = new Matrix(MatrixPathB);

            // Create C Matrix.
            MatrixC = new Matrix(MatrixA.SizeY, MatrixB.SizeX, false);

            // Notify.
            Console.WriteLine($"Matrix A [{MatrixA.SizeY},{MatrixA.SizeX}] and Matrix B [{MatrixB.SizeY},{MatrixB.SizeX}] lodaded, created Result Matrix C [{MatrixC.SizeY},{MatrixC.SizeX}].");

            // Create Listener.
            Listener = new TcpListener(IPAddress.Loopback, Messaging.TcpPort);

            // Start Listener.
            Listener.Start();

            // Notify.
            Console.WriteLine("Waiting for Clients...");

            // Wait for Clients...
            while (ClientList.Count < AmountOfClients) {
                // Wait for Pending Client.
                if (!Listener.Pending())
                    continue;

                // Accept it and add to Clients List.
                ClientList.Add(Listener.AcceptTcpClient());

                // Notify.
                Console.WriteLine($"Client {ClientList.Count} has connected.");
            }

            // We have all Clients, notify.
            Console.WriteLine($"All Clients Connected.");

            // Get Total Count of Portions.
            int Count = Convert.ToInt32(Math.Floor(MatrixC.Indexes / (float) AmountOfClients));

            // For Each Client...
            for (int c = 0; c < ClientList.Count; c++) {
                // Get Client.
                var Client = ClientList[c];

                // Create Packet.
                BinaryWriter Packet = Messaging.CreatePacket();

                // Serialize Matrix A and B.
                MatrixA.Serialize(Packet);
                MatrixB.Serialize(Packet);

                // Calculate Start and End Indexes.
                int StartIndex = c * Count;
                int EndIndex = (c + 1) == AmountOfClients ? MatrixC.Indexes : StartIndex + Count;

                // Add Start Index and End Index.
                Packet.Write(StartIndex);
                Packet.Write(EndIndex);
                Packet.Write(MatrixC.SizeY);

                // Send Packet.
                Messaging.SendPacket(Client, Packet);
            }

            // Notify Data Sent.
            Console.WriteLine("All Data sent to clients, waiting for response...");

            // Loop all Clients...
            foreach (TcpClient Client in ClientList) {
                // Wait for Packet. (Clients will wait for packets to be processed)
                while (Client.Available <= 0) { }

                // Receive Packet.
                BinaryReader Packet = Messaging.ReceivePacket(Client);

                // Read the Start and End Indexes.
                int StartIndex = Packet.ReadInt32();
                int EndIndex = Packet.ReadInt32();

                // Read the data, set value.
                for (int d = StartIndex; d < EndIndex; d++)
                    MatrixC[d] = Packet.ReadDouble();

                // Close Packet.
                Packet.Close();

                // Notify.
                Console.WriteLine("Received Data from Client, setting MatrixC...");
            }

            // Matrix is ready, stop Tcp Listener.
            Listener.Stop();

            // Save Matrix C.
            MatrixC.Save(MatrixPathC);

            // Notify.
            Console.WriteLine("Operation Completed.");
        }

        // Creates Matrixes (used once).
        private static void CreateMatrixes() {
            // Create new Random.
            var Rand = new Random();

            // Generate K, M and N.
            int k = Rand.Next(200) + 400;
            int m = Rand.Next(200) + 400;
            int n = Rand.Next(200) + 400;

            // Create Matrix M, K.
            var Mat = new Matrix(m, k, true);

            // Save it as Matrix A.
            Mat.Save(MatrixPathA);

            // Create Matrix K, N.
            Mat = new Matrix(k, n, true);

            // Save it as Matrix B.
            Mat.Save(MatrixPathB);
        }
    }
}
