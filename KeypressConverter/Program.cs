using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace KeypressConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check if KeyConfig.txt exists. If it doesn't, create it.
            if (!File.Exists(Config.KeyConfigPath))
            {
                Config.CreateKeyConfig();
            }

            // Read Config
            Config.ConvertedKeys = Config.ReadKeyConfig();

            if (Config.ConvertedKeys.Length == 0)
            {
                Console.WriteLine("No valid key definitions found in KeyConfig.txt.");
                Console.WriteLine("\nPress ENTER to exit application...");
                Console.ReadLine();
                Environment.Exit(0);
            }

            Console.WriteLine("--Keys To Modify--");
            foreach (var key in Config.ConvertedKeys)
            {
                Console.WriteLine(key.ToString());
            }

            // Console Applications don't have a message loop, create one by creating a hidden form on a separate thread
            HiddenForm form = new HiddenForm();

            Thread messageThread = new Thread(() => Application.Run(form));
            messageThread.Start();

            Console.WriteLine("\nPress ENTER to exit application...");
            Console.ReadLine();

            // Can't close form on separate thread from this thread, invoke it instead
            form.Invoke((MethodInvoker)delegate { form.Close(); });
        }
    }
}
