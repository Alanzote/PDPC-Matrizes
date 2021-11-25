using System;
using System.IO;
using System.Linq;

namespace PDPC_Matrizes.Generic {

    /**
     * The Matrix Class.
     * Just a wrapper for reading/saving/serializing data.
     */
    internal class Matrix {

        // Our Actual Matrix.
        double[,] matrix;

        // Size in Y.
        public int SizeY => matrix.GetLength(0);

        // Size in X.
        public int SizeX => matrix.GetLength(1);

        // The Amount of Indexes.
        public int Indexes => SizeY * SizeX;

        // Crates a Random Matrix (or just an empty one).
        public Matrix(int Y, int X, bool Random) {
            // Create Matrix.
            matrix = new double[Y, X];

            // Stop if we aren't meant to be random.
            if (!Random)
                return;

            // Create Random.
            Random Rand = new Random();

            // Loop Y and X...
            for (int y = 0; y < Y; y++)
                for (int x = 0; x < X; x++)
                    matrix[y, x] = Rand.NextDouble();
        }

        // Constructor from a Packet.
        public Matrix(BinaryReader Packet) {
            // Create Matrix.
            matrix = new double[Packet.ReadInt32(), Packet.ReadInt32()];

            // Loop in Y and X, setting data.
            for (int y = 0; y < SizeY; y++)
                for (int x = 0; x < SizeX; x++)
                    matrix[y, x] = Packet.ReadDouble();
        }

        // Constructs from a CSV File.
        public Matrix(string Path) {
            // Make sure File Exists.
            if (!File.Exists(Path))
                throw new FileNotFoundException(null, Path);

            // Read all Text, split by lines.
            string[] Lines = File.ReadAllLines(Path);

            // Make sure we have enough lines...
            if (Lines.Length <= 0)
                throw new FormatException("File is not on the correct format.");

            // Get First Line, split by commas.
            string[] FirstLine = Lines.First().Split(',');

            // Check First Line Length.
            if (FirstLine.Length != 2)
                throw new FormatException("File Header is not Size Y, X.");

            // It is, then, parse the values.
            int[] YXValues = FirstLine.Select(x => int.Parse(x)).ToArray();

            // Create new Matrix.
            matrix = new double[YXValues[0], YXValues[1]];

            // Get Remaining Lines.
            Lines = Lines.Skip(1).ToArray();

            // Check for Y Size.
            if (Lines.Length != SizeY)
                throw new FormatException($"Not enough Rows File, expected: {SizeY}, got: {Lines.Length}.");

            // Loop all Remaining Lines...
            for (int l = 0; l < Lines.Length; l++) {
                // Split this Line.
                string[] Splt = Lines[l].Split(',');

                // Check for Size.
                if (Splt.Length != SizeX)
                    throw new FormatException($"Not enough Columns on Line {l + 2}, expected: {SizeX}, got: {Splt.Length}.");

                // L is our Y, so now we need an X.
                for (int x = 0; x < Splt.Length; x++)
                    matrix[l, x] = double.Parse(Splt[x]);
            }
        }

        // Serializes into a Packet.
        public void Serialize(BinaryWriter Writer) {
            // Write Lengths.
            Writer.Write(SizeY);
            Writer.Write(SizeX);

            // Loop in Y and X, writing data.
            for (int y = 0; y < SizeY; y++)
                for (int x = 0; x < SizeX; x++)
                    Writer.Write(matrix[y, x]);
        }

        // Saves into a CSV file.
        public void Save(string Path) {
            // Delete File if Exists.
            if (File.Exists(Path))
                File.Delete(Path);

            // Create Stream.
            TextWriter Stream = new StreamWriter(Path);

            // Write Header.
            Stream.WriteLine($"{SizeY},{SizeX}");

            // For Each Matrix in Y...
            for (int y = 0; y < SizeY; y++) {
                // For Each Matrix in X...
                for (int x = 0; x < SizeX; x++) {
                    // Write the Value.
                    Stream.Write(matrix[y, x]);

                    // If we are going to pass the amount of columns, don't write the ','.
                    if (x + 1 >= SizeX)
                        continue;

                    // Write the ',' to separate values.
                    Stream.Write(',');
                }

                // If we are going to pass the amount of lines, don't write end line.
                if (y + 1 >= SizeY)
                    continue;

                // Write End Line.
                Stream.WriteLine();
            }

            // Close the Stream.
            Stream.Close();
        }

        // Access the Matrix.
        public double this[int y, int x] {
            get => matrix[y, x];
            set => matrix[y, x] = value;
        }

        // Also Access the Matrix, but with a single Index.
        public double this[int i] {
            // Get.
            get {
                // Calculate Y and X.
                int Y = i % SizeY;
                int X = i / SizeY;

                // Return Data.
                return this[Y, X];
            }

            // Set.
            set {
                // Calculate Y and X.
                int Y = i % SizeY;
                int X = i / SizeY;

                // Set Data.
                this[Y, X] = value;
            }
        }
    }
}
