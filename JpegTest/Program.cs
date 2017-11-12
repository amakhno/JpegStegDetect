using System;
using System.IO;
using Accord.Controls;
using Accord.IO;
using Accord.Math;
using Accord.Statistics.Distributions.Univariate;
using Accord.MachineLearning.Bayes;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using System.Data;

namespace JpegTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DCMatrix matrix;
            DirectoryInfo d = new DirectoryInfo(@".\images");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.jp?g"); //Getting Text files
            double[][] inputs = new double[Files.Length][];
            double[] outputs = new double[Files.Length];
            //Accord
            var teacher = new SequentialMinimalOptimization<Gaussian>()
            {
                UseComplexityHeuristic = true,
                UseKernelEstimation = true // estimate the kernel from the data
            };

            for (int i = 0; i < inputs.Length; i++)
            {
                matrix = new DCMatrix(Files[i].FullName);
                var stat = matrix.GetStat();
                inputs[i] = stat;
                outputs[i] = 0;
            }


            var svm = teacher.Learn(inputs, outputs);
            Console.ReadKey(true);
        }

        static void PrintChannel(short[][,] Y)
        {
            string output = "";

            for (int k = 0; k < ((Y.Length > 10) ? 10 : Y.Length); k++)
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
