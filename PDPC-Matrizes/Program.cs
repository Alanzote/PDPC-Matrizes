using PDPC_Matrizes.Main;

namespace PDPC_Matrizes {

    /**
     * The Main Program Class.
     * Main Method.
     */
    internal class Program {

        // Main Method.
        static void Main(string[] args) {
#if COORDINATOR
            // Call the Main from the Coordinator.
            Coordinator.Main(args);
#else
            // Call the Main from the Calculator.
            Calculator.Main(args);
#endif
        }
    }
}
