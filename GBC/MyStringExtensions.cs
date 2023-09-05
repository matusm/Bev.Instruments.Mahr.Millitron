using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;

namespace GBC
{
    public static class MyStringExtensions
    {

        public static void ToConsole(this string value)
        {
            Console.Clear();
            Console.WriteLine(value);
        }

        public static void ToFile(this string value, string fileName)
        {
            try
            {
                StreamWriter streamWriter = File.CreateText(fileName);
                //string newText = value.Replace("\n", "\r\n");
                string newText = value;
                streamWriter.WriteLine(newText);
                streamWriter.Close();
            }
            catch
            {
                Console.WriteLine($"Could not write to file {fileName}"); ;
            }
        }

        public static void ToPrinter(this string value)
        {
            PrintDocument p = new PrintDocument();
            p.PrintPage += delegate (object sender1, PrintPageEventArgs e1)
            {
                e1.Graphics.DrawString(value, new Font("Courier New", 12), new SolidBrush(Color.Black), new RectangleF(0, 0, p.DefaultPageSettings.PrintableArea.Width, p.DefaultPageSettings.PrintableArea.Height));
            };
            try { p.Print(); }
            catch { Console.WriteLine($"Could not print report"); }
        }

    }
}
