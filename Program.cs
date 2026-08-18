using System;
using System.Windows.Forms;

namespace TRpkgTools
{
    internal static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            if (args != null && args.Length >= 2)
            {
                try
                {
                    if (args[0] == "--unpack")
                    {
                        Console.WriteLine(PkgJobs.Unpack(args[1], PkgJobs.ExeDir(), null));
                        return 0;
                    }
                    if (args[0] == "--repack")
                    {
                        Console.WriteLine(PkgJobs.Repack(args[1], PkgJobs.ExeDir(), null));
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return 1;
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            return 0;
        }
    }
}
