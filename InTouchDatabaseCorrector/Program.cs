using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InTouchDatabaseCorrector
{
    class Program
    {
        public static void Main(string[] args)
        {
            string infile, outfile;
            if (Console.LargestWindowWidth >= 200)
                Console.WindowWidth = 200;
            ConsoleColor temp = Console.ForegroundColor;
            try
            {
                if (args.Contains("-r"))
                {
                    outfile = args[args.Length - 1];
                    infile = args[args.Length - 2];
                    try
                    {
                        Console.WriteLine("Reading Database...");
                        Database db = Database.CreateFromCSV(infile);
                        Console.WriteLine("Attempting repair...");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        db.Repair();
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Writing database...");
                        db.Write(outfile);
                        Console.WriteLine("Done.");
                        Console.ReadKey();
                    }
                    catch(Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.ToString());
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Press any key to continue...");
                        Console.ReadKey();
                    }
                }
                else if (args.Contains("-v"))
                {
                    infile = args[args.Length - 1];
                    try
                    {
                        Console.WriteLine("Reading Database...");
                        Database db = Database.CreateFromCSV(infile);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        db.Validate();
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Done.");
                        Console.ReadKey();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.ToString());
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Press any key to continue...");
                        Console.ReadKey();
                    }
                }

            }
            catch
            {
                Console.WriteLine("Options: -r -v <infile> [outfile]");
                Console.WriteLine("-r: Repair infile, outfile must be specified");
                Console.WriteLine("-v: Validate file");
                Console.ReadKey();
            }
            Console.ForegroundColor = temp;
        }

    }
}
