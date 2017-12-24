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
using Accord.MachineLearning.VectorMachines;
using System.Collections.Generic;

namespace JpegTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DCMatrix matrix;
            DirectoryInfo d = new DirectoryInfo(@".\images");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.jpg"); //Getting Text files
            double[][] inputs1 = new double[Files.Length][];
            double[] outputs1 = new double[Files.Length];
            //Accord
            var teacher = new SequentialMinimalOptimization<Gaussian>()
            {
                UseComplexityHeuristic = true,
                UseKernelEstimation = true // estimate the kernel from the data
            };

            List<int> errorIndexesOriginal = new List<int>();
            List<int> errorIndexesNew = new List<int>();

            Console.WriteLine("Original Images");
            for (int i = 0; i < inputs1.Length; i++)
            {
                Console.WriteLine("Image: " + i);
                try
                {
                    matrix = new DCMatrix(Files[i].FullName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    errorIndexesOriginal.Add(i);
                    continue;
                }
                var stat = matrix.GetStat();
                inputs1[i] = stat;
                outputs1[i] = 0;
            }

            d = new DirectoryInfo(@".\outimages");//Assuming Test is your Folder
            Files = d.GetFiles("*.jpg"); //Getting Text files
            double[][] inputs2 = new double[Files.Length][];
            double[] outputs2 = new double[Files.Length];
            Console.WriteLine("New Images");
            for (int i = 0; i < inputs2.Length; i++)
            {
                Console.WriteLine("Image: " + i);
                try
                {
                    matrix = new DCMatrix(Files[i].FullName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    errorIndexesNew.Add(i);
                    continue;
                }
                var stat = matrix.GetStat();
                inputs2[i] = stat;
                outputs2[i] = 1;
            }

            foreach (int index in errorIndexesOriginal)
            {
                inputs1 = inputs1.RemoveAt(index);
                outputs1 = outputs1.RemoveAt(index);
            }

            foreach (int index in errorIndexesNew)
            {
                inputs2 = inputs2.RemoveAt(index);
                outputs2 = outputs2.RemoveAt(index);
            }

            var inputs = ConcatArrays(inputs1, inputs2);
            var outputs = ConcatArrays(outputs1, outputs2);

            SupportVectorMachine<Gaussian> nb = teacher.Learn(inputs, outputs);

            d = new DirectoryInfo(@".\test");//Assuming Test is your Folder
            Files = d.GetFiles("*.jpg"); //Getting Text files
            double[][] inputsTest = new double[Files.Length][];
            Console.WriteLine("Test Images");
            for (int i = 0; i < inputsTest.Length; i++)
            {
                Console.Write("Image \"" + Files[i].Name + "\":");
                matrix = new DCMatrix(Files[i].FullName);
                var stat = matrix.GetStat();
                Console.WriteLine(nb.Decide(stat));
            }

            nb.Decide(inputs1[0]);
            Console.ReadKey(true);
        }

        static double[][] ConcatArrays(double[][] x, double[][] y)
        {
            var z = new double[x.GetLength(0) + y.GetLength(0)][];
            x.CopyTo(z, 0);
            y.CopyTo(z, x.Length);
            return z;
        }

        static double[] ConcatArrays(double[] x, double[] y)
        {
            var z = new double[x.GetLength(0) + y.GetLength(0)];
            x.CopyTo(z, 0);
            y.CopyTo(z, x.Length);
            return z;
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
