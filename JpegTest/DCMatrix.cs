using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegTest
{
    class DCMatrix
    {
        public short[][,] Y;

        public short[][,] Cb;

        public short[][,] Cr;

        public short[][,] B;

        int numOfMatrix = 0;

        int c00, c01, c10, c11;

        private short[][,] Matrix(int index)
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
            System.IO.FileStream oFileStreamImage = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
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
            int c, ref short[][,] Target)
        {
            var lengthOfChannel = oJpegDecompress.Comp_info[c].Width_in_blocks;
            //var reader = JBlock[c].Access(0, lengthOfChannel); // accessing the element
            var reader = JBlock[c].Access(0, 1);
            int realWidth = 0;
            for(int i = 0; i<lengthOfChannel; i++)
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
            Target = new short[reader.Length * reader[0].Length][,];
            for (int i = 0; i < reader.Length; i++)
            {
                for (int j = 0; j < reader[i].Length; j++)
                {
                    Target[numOfMatrix] = new short[8, 8];
                    for (int k = 0; k < 64; k++)
                    {
                        Target[numOfMatrix][k / 8, k % 8] = reader[i][j][k];
                    }
                    ++numOfMatrix;
                }
            }
        }

        public double[] GetStat()
        {            
            List<double> list = new List<double>();
            for (int c = 0; c<3; c++)
            {
                int countOfElements = 0;
                c00 = c01 = c10 = c11 = 0;
                int c0x = 0, c1x = 0;
                int cx0 = 0, cx1 = 0;
                int cXor0 = 0, cXor1 = 0;
                var currentMatrix = Matrix(c);
                for(int k = 0; k < currentMatrix.Length; k++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if ((currentMatrix[k][i, j] == 0)||(currentMatrix[k][i, j] == 1))
                            {
                                continue;
                            }
                            countOfElements++;
                            int lastTwo = TakeLastTwo(currentMatrix[k][i, j]);                            
                            checkLastTwo(lastTwo);
                            c1x += (currentMatrix[k][i, j] >> 1) & 1;
                            c0x += ~(currentMatrix[k][i, j] >> 1) & 1;
                            cx1 += (currentMatrix[k][i, j]) & 1;
                            cx0 += ~(currentMatrix[k][i, j]) & 1;
                            cXor0 += (~((currentMatrix[k][i, j] >> 1) ^ currentMatrix[k][i, j])) & 1;
                            cXor1 += (((currentMatrix[k][i, j] >> 1) ^ currentMatrix[k][i, j]) & 1) ^ 0;
                        }
                    }
                }

                //Хи от 2 последних
                double[] pXX = new double[4];
                pXX[0] = (float)c00 / countOfElements;
                pXX[1] = (float)c01 / countOfElements;
                pXX[2] = (float)c10 / countOfElements;
                pXX[3] = (float)c11 / countOfElements;
                double HiXX = 0;
                for (int i = 0; i < pXX.Length; i++)
                {
                    HiXX += (pXX[i] - 0.5) * (pXX[i] - 0.5);
                }
                HiXX /= pXX.Length;

                //Хи от предпоследнего 
                double[] pXx = new double[2];
                pXx[0] = (float)c0x / countOfElements;
                pXx[1] = (float)c1x / countOfElements;
                double HiXx = 0;
                for (int i = 0; i < pXx.Length; i++)
                {
                    HiXx += (pXx[i] - 0.5) * (pXx[i] - 0.5);
                }
                HiXx /= pXx.Length;

                //Хи от последнего 
                double[] pxX = new double[2];
                pxX[0] = (float)cx0 / countOfElements;
                pxX[1] = (float)cx1 / countOfElements;
                double HixX = 0;
                for (int i = 0; i < pxX.Length; i++)
                {
                    HixX += (pxX[i] - 0.5) * (pxX[i] - 0.5);
                }
                HixX /= pxX.Length;

                //Хи от Ксора предпоследнего и последнего 
                double[] pXorX = new double[2];
                pXorX[0] = (float)cXor0 / countOfElements;
                pXorX[1] = (float)cXor1 / countOfElements;
                double HiXorX = 0;
                for (int i = 0; i < pXorX.Length; i++)
                {
                    HiXorX += (pXorX[i] - 0.5) * (pXorX[i] - 0.5);
                }
                HiXorX /= pXorX.Length;

                
                list.AddRange(pXX);
                list.Add(HiXX);
                list.AddRange(pXx);
                list.Add(HiXx);
                list.AddRange(pxX);
                list.Add(HixX);
                list.AddRange(pXorX);
                list.Add(HiXorX);                
            }

            int parameters = (list.Count / 3);
            for (int i = 0; i< parameters; i++)
            {
                list[i] += list[i + parameters] + list[i + parameters];
                list[i] /= 3;
            }

            return list.Take(parameters).ToArray();
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
    }
}
