using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JpegTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DCMatrix matrix = new DCMatrix("cat.jpg");
            DirectoryInfo d = new DirectoryInfo(@".\images");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.jpg"); //Getting Text files
            var stat = matrix.GetStat();
            DCMatrix matrix2 = new DCMatrix("cat(1).jpg");
            Console.ReadKey(true);
        }

        static void PrintChannel(short[][,] Y)
        {
            string output = "";

            for(int k = 0; k<((Y.Length>10)?10:Y.Length) ; k++)
            {
                for (int i = 0; i < Y[k].GetLength(0); i++)
                {
                    for (int j = 0; j < Y[k].GetLength(1); j++)
                    {
                        output += Y[k][i, j] + " ";
                    }
                    output = output.Remove(output.Length - 1, 1);
                    output += '\n';
                }
                output = output.Remove(output.Length - 1, 1);
                output += "\n\n";
            }

            output = output.Remove(output.Length - 1, 1);

            Console.Write(output);
        }
    }
}
