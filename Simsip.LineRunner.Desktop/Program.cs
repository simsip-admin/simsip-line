

namespace Simsip.LineRunner
{

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (TheGame game = new TheGame())
            {
                game.Run();
            }
        }

        public static void RateApp()
        {
            // No-op
        }

    }


}

