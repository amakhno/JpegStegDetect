using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegTest
{
    class DCMatrix
    {
        public short[][] Y;

        public short[][] Cb;

        public short[][] Cr;

        public short[][] B;

        int numOfMatrix = 0;

        int c00, c01, c10, c11;

        private short[][] Matrix(int index)
        {
            switch (index)
            {
                case 0:
                    {
                        return Y;
                    }
                case 1:
                    {
                        return Cb;
                    }
                case 2:
                    {
                        return Cr;
                    }
                default:
                    {
                        throw new IndexOutOfRangeException();
                    }
            }
        }

        public DCMatrix(string path)
        {
            BitMiracle.LibJpeg.Classic.jpeg_decompress_struct oJpegDecompress = new BitMiracle.LibJpeg.Classic.jpeg_decompress_struct();
            System.IO.FileStream oFileStreamImage;
            try
            {
                oFileStreamImage = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            }
            catch
            {
                throw new Exception("Error reading");
            }
            oJpegDecompress.jpeg_stdio_src(oFileStreamImage);
            oJpegDecompress.jpeg_read_header(true);
            BitMiracle.LibJpeg.Classic.jvirt_array<BitMiracle.LibJpeg.Classic.JBLOCK>[] JBlock = oJpegDecompress.jpeg_read_coefficients();
            if (JBlock[2] != null)
            {
                ExtractChannel(oJpegDecompress, JBlock, 0, ref Y);
                ExtractChannel(oJpegDecompress, JBlock, 1, ref Cb);
                ExtractChannel(oJpegDecompress, JBlock, 2, ref Cr);
            }
            else
            {
                ExtractChannel(oJpegDecompress, JBlock, 0, ref B);
            }
        }

        private void ExtractChannel(BitMiracle.LibJpeg.Classic.jpeg_decompress_struct oJpegDecompress,
            BitMiracle.LibJpeg.Classic.jvirt_array<BitMiracle.LibJpeg.Classic.JBLOCK>[] JBlock,
            int c, ref short[][] Target)
        {
            var lengthOfChannel = oJpegDecompress.Comp_info[c].Width_in_blocks;
            var reader = JBlock[c].Access(0, 1);
            int realWidth = 0;
            for (int i = 0; i < lengthOfChannel; i++)
            {
                try
                {
                    reader = JBlock[c].Access(i, 1);
                }
                catch
                {
                    realWidth = i;
                    break;
                }
            }
            realWidth = (realWidth != 0) ? realWidth : lengthOfChannel;
            reader = JBlock[c].Access(0, realWidth);
            numOfMatrix = 0;
            int sizeX = reader.Length;
            Target = new short[reader.Length * reader[0].Length][];
            for (int i = 0; i < reader.Length; i++)
            {
                for (int j = 0; j < reader[i].Length; j++)
                {
                    Target[numOfMatrix] = new short[64];
                    for (int k = 0; k < 64; k++)
                    {
                        Target[numOfMatrix][k] = reader[i][j][k];
                    }
                    ++numOfMatrix;
                }
            }
        }

        public double[] GetStat()
        {
            //-------------------GLOBAL-HISTOGRAM-------------------//
            WriteToFile("outC#.txt");

            return new double[] { 0 }; 
        }

        public double[] GetStatBOnly()
        {
            return new double[] { 0 };
        }

        private int TakeLastTwo(short input)
        {
            return (input & 3);
        }

        private void checkLastTwo(int lastTwo)
        {
            switch (lastTwo)
            {
                case 0:
                    {
                        c00++;
                        break;
                    }
                case 1:
                    {
                        c01++;
                        break;
                    }
                case 2:
                    {
                        c10++;
                        break;
                    }
                case 3:
                    {
                        c11++;
                        break;
                    }
                default:
                    {
                        throw new Exception("ERROR!");
                    }
            }
        }

        private double[] Hist(short[][] input, int begin, int end)
        {
            int size = end - begin;
            double[] result = new double[size];
            for(int i = 0; i<input.Length; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int c = 0; c < result.Length; c++)
                    {
                        if (input[i][j] == begin+c)
                        {
                            result[c]++;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        private double[] DualHist(short[][] input)
        {
            int[][] modes = new int[][] { new int[] { 1, 0},
            new int[] { 0, 1},
            new int[] { 2, 0},
            new int[] { 1, 1},
            new int[] { 0, 2},
            new int[] { 3, 0},
            new int[] { 2, 1},
            new int[] { 1, 2},
            new int[] { 0, 3}};
            var a = new short[][] { new short[] { 1, 2, 3 },
            new short[] { 4, 5, 6 },
            new short[] { 7, 8, 9 }};
            TakePart(a, 0,2,2,0,2,2);
            for ( int i = -5 ; i<=5 ; i++ )
            {
                int[] T = new int[9];
                for (int j = 0; j < 9; j++)
                {
                    ;//T[j] +=
                }
            }
            return new double[] { 0 };
        }

        private double[][] TakePart(short[][] input,int a1, int s1, int b1, int a2, int s2, int b2)
        {
            b1++;
            b2++;
            double[][] result = new double[((b1 - a1) / s1) + 1][];
            for (int i = 0; a1 + (i*s1)<b1; i++)
            {
                result[i] = new double[( (b2 - a2) / s2 ) + 1];
                for (int j = 0; a1 + (j * s2) < b2; j++)
                {
                    result[a1 + s1 * i][a2 + s2 * j] = input[a1 + s1 * i][a2 + s2 * j];
                }
            }
            return result;

        }

        public void WriteToFile(string fileName)
        {
            int end = (Matrix(1)[0][0] == 0) ? 1 : 3;
            StreamWriter writer = new StreamWriter(fileName);
            for (int i = 0; i<end; i++)
            {
                for (int j = 0; j < Matrix(i).Length; j++)
                {
                    for (int k = 0; k < Matrix( i )[j].Length; k++)
                    {
                        writer.Write(Matrix( i )[j][k] + " ");
                    }
                }
            }
            writer.Close();
        }
    }
}


#region OldCode
//if (Matrix(1)[0]
//[0] == 0)
//            {
//                return GetStatBOnly();
//            }
//            List<double> list = new List<double>();
//            for (int c = 0; c< 3; c++)
//            {
//                int countOfElements = 0;
//c00 = c01 = c10 = c11 = 0;
//                int c0x = 0, c1x = 0;
//int cx0 = 0, cx1 = 0;
//int cXor0 = 0, cXor1 = 0;
//var currentMatrix = Matrix(c);
//                for (int k = 0; k<currentMatrix.Length; k++)
//                {
//                    for (int i = 0; i< 8; i++)
//                    {
//                        for (int j = 0; j< 8; j++)
//                        {
//                            if ((currentMatrix[k][i, j] == 0) || (currentMatrix[k][i, j] == 1))
//                            {
//                                continue;
//                            }
//                            countOfElements++;
//                            int lastTwo = TakeLastTwo(currentMatrix[k][i, j]);
//                            checkLastTwo(lastTwo);
//c1x += (currentMatrix[k][i, j] >> 1) & 1;
//                            c0x += ~(currentMatrix[k][i, j] >> 1) & 1;
//                            cx1 += (currentMatrix[k][i, j]) & 1;
//                            cx0 += ~(currentMatrix[k][i, j]) & 1;
//                            cXor0 += (~((currentMatrix[k][i, j] >> 1) ^ currentMatrix[k][i, j])) & 1;
//                            cXor1 += (((currentMatrix[k][i, j] >> 1) ^ currentMatrix[k][i, j]) & 1) ^ 0;
//                        }
//                    }
//                }

//                //Хи от 2 последних
//                double[] pXX = new double[4];
//pXX[0] = (float)c00 / countOfElements;
//                pXX[1] = (float)c01 / countOfElements;
//                pXX[2] = (float)c10 / countOfElements;
//                pXX[3] = (float)c11 / countOfElements;
//                double HiXX = 0;
//                for (int i = 0; i<pXX.Length; i++)
//                {
//                    HiXX += (pXX[i] - 0.5) * (pXX[i] - 0.5);
//                }
//                HiXX /= pXX.Length;

//                //Хи от предпоследнего 
//                double[] pXx = new double[2];
//pXx[0] = (float)c0x / countOfElements;
//                pXx[1] = (float)c1x / countOfElements;
//                double HiXx = 0;
//                for (int i = 0; i<pXx.Length; i++)
//                {
//                    HiXx += (pXx[i] - 0.5) * (pXx[i] - 0.5);
//                }
//                HiXx /= pXx.Length;

//                //Хи от последнего 
//                double[] pxX = new double[2];
//pxX[0] = (float)cx0 / countOfElements;
//                pxX[1] = (float)cx1 / countOfElements;
//                double HixX = 0;
//                for (int i = 0; i<pxX.Length; i++)
//                {
//                    HixX += (pxX[i] - 0.5) * (pxX[i] - 0.5);
//                }
//                HixX /= pxX.Length;

//                //Хи от Ксора предпоследнего и последнего 
//                double[] pXorX = new double[2];
//pXorX[0] = (float)cXor0 / countOfElements;
//                pXorX[1] = (float)cXor1 / countOfElements;
//                double HiXorX = 0;
//                for (int i = 0; i<pXorX.Length; i++)
//                {
//                    HiXorX += (pXorX[i] - 0.5) * (pXorX[i] - 0.5);
//                }
//                HiXorX /= pXorX.Length;


//                list.AddRange(pXX);
//                list.Add(HiXX);
//                list.AddRange(pXx);
//                list.Add(HiXx);
//                list.AddRange(pxX);
//                list.Add(HixX);
//                list.AddRange(pXorX);
//                list.Add(HiXorX);
//            }

//            int parameters = (list.Count / 3);
//            for (int i = 0; i<parameters; i++)
//            {
//                list[i] += list[i + parameters] + list[i + parameters];
//                list[i] /= 3;
//            }

//            if (list.Take(parameters).ToArray()[0] == Double.NaN)
//            {
//                ;
//            }
//            return list.Take(parameters).ToArray();
#endregion