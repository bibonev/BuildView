using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Drawing;

namespace BuildViewEditorControl
{
    public partial class Model
    {
        public void ExportPcd(string name)
        {
            AsyncFlag = true;
            System.IO.FileStream fs = null;
            try
            {
                using (fs = System.IO.File.Create(name))
                {
                    BinaryWriter writeInfo = new BinaryWriter(fs);
                    string s = "# .PCD v0.7 - Point Cloud Data file format\n" +
                                    "VERSION 0.7\n" +
                                    "FIELDS x y z rgba\n" +
                                    "SIZE 4 4 4 4\n" +
                                    "TYPE F F F U\n" +
                                    "COUNT 1 1 1 1\n" +
                                    string.Format("WIDTH {0}\n", Count >> 1) +
                                    "HEIGHT 1\n" +
                                    "VIEWPOINT 0 0 0 1 0 0 0\n" +
                                    string.Format("POINTS {0}\n", Count >> 1) +
                                    "DATA binary\n";

                    writeInfo.Write(Encoding.ASCII.GetBytes(s));

                    for (int i = 0; i < Count; i += 2)
                    {
                        writeInfo.Write(Cloud[i].Position.X);
                        writeInfo.Write(Cloud[i].Position.Y);
                        writeInfo.Write(Cloud[i].Position.Z);

                        writeInfo.Write(Cloud[i].Color.R);
                        writeInfo.Write(Cloud[i].Color.G);
                        writeInfo.Write(Cloud[i].Color.B);
                        writeInfo.Write(Cloud[i].Color.A);
                    }
                }
            }
            catch (Exception)
            {
            }
            AsyncFlag = false;
        }

        public void ExportPcdRgb(string name)
        {
            AsyncFlag = true;
            System.IO.FileStream fs = null;
            try
            {
                using (fs = System.IO.File.Create(name))
                {
                    BinaryWriter writeInfo = new BinaryWriter(fs);
                    string s = "# .PCD v0.7 - Point Cloud Data file format\n" +
                                    "VERSION 0.7\n" +
                                    "FIELDS x y z rgb\n" +
                                    "SIZE 4 4 4 4\n" +
                                    "TYPE F F F F\n" +
                                    "COUNT 1 1 1 1\n" +
                                    string.Format("WIDTH {0}\n", Count >> 1) +
                                    "HEIGHT 1\n" +
                                    "VIEWPOINT 0 0 0 1 0 0 0\n" +
                                    string.Format("POINTS {0}\n", Count >> 1) +
                                    "DATA binary\n";

                    writeInfo.Write(Encoding.ASCII.GetBytes(s));

                    for (int i = 0; i < Count; i += 2)
                    {
                        writeInfo.Write(Cloud[i].Position.X);
                        writeInfo.Write(Cloud[i].Position.Y);
                        writeInfo.Write(Cloud[i].Position.Z);

                        writeInfo.Write(Cloud[i].Color.R);
                        writeInfo.Write(Cloud[i].Color.G);
                        writeInfo.Write(Cloud[i].Color.B);
                        writeInfo.Write(Cloud[i].Color.A);
                    }
                }
            }
            catch (Exception)
            {
            }
            AsyncFlag = false;
        }

        public void ImportPcd(string name)
        {
            AsyncFlag = true;
            try
            {
                using (FileStream inFile = File.Open(name, FileMode.Open))
                {
                    BinaryReader readInfo = new BinaryReader(inFile);
                    StringBuilder headerRow;
                    char currentSymbol;

                    do
                    {
                        headerRow = new StringBuilder();
                        do
                        {
                            currentSymbol = readInfo.ReadChar();
                            if (currentSymbol == '\r')
                            {
                                continue;
                            }
                            headerRow.Append(currentSymbol);
                        } while (currentSymbol != '\n');

                        string s = headerRow.ToString();

                        if (s.StartsWith("POINTS "))
                        {
                            Count = Convert.ToInt64(s.Substring(7, s.Length - 8)) * 2;
                        }
                    } while (headerRow.ToString() != "DATA binary\n");

                    Cloud = new VertexPositionColor[Count];

                    for (long i = 0; i < Count; i += 2)
                    {
                        Cloud[i].Position.X = readInfo.ReadSingle();
                        Cloud[i].Position.Y = readInfo.ReadSingle();
                        Cloud[i].Position.Z = readInfo.ReadSingle();

                        Cloud[i].Color.R = readInfo.ReadByte();
                        Cloud[i].Color.G = readInfo.ReadByte();
                        Cloud[i].Color.B = readInfo.ReadByte();
                        Cloud[i].Color.A = readInfo.ReadByte();

                        Cloud[i + 1] = Cloud[i];
                        Cloud[i + 1].Position.X += Eps;
                    }
                }
            }

            catch (Exception)
            {
            }
            AsyncFlag = false;
        }

        public void ImportMesh(string name)
        {
            AsyncFlag = true;
            try
            {
                using (FileStream inFile = File.Open(name, FileMode.Open))
                {
                    BinaryReader readInfo = new BinaryReader(inFile);
                    MeshCount = inFile.Length / sizeof(int);
                    MeshIndexes = new int[MeshCount];

                    for (long i = 0; i < MeshCount; i++)
                    {
                        MeshIndexes[i] = readInfo.ReadInt32() * 2;
                    }
                }
            }

            catch (Exception)
            {
            }

            AsyncFlag = false;
        }

        public void Export(string name)
        {
            ColladaExport ex = new ColladaExport();
            ex.Export(Cloud, Count, MeshIndexes, MeshCount, name);
        }

        public void Open(GZipStream gz)
        {
            BinaryReader readInfo = new BinaryReader(gz);
            Count = readInfo.ReadInt64();

            Cloud = new VertexPositionColor[Count];

            for (long i = 0; i < Count; i++)
            {
                Cloud[i].Position.X = readInfo.ReadSingle();
                Cloud[i].Position.Y = readInfo.ReadSingle();
                Cloud[i].Position.Z = readInfo.ReadSingle();

                Cloud[i].Color.R = readInfo.ReadByte();
                Cloud[i].Color.G = readInfo.ReadByte();
                Cloud[i].Color.B = readInfo.ReadByte();
                Cloud[i].Color.A = readInfo.ReadByte();
            }
        }

        public void Save(GZipStream gz)
        {
            BinaryWriter writeInfo = new BinaryWriter(gz);
            writeInfo.Write(Count);

            for (int i = 0; i < Count; i++)
            {
                writeInfo.Write(Cloud[i].Position.X);
                writeInfo.Write(Cloud[i].Position.Y);
                writeInfo.Write(Cloud[i].Position.Z);

                writeInfo.Write(Cloud[i].Color.R);
                writeInfo.Write(Cloud[i].Color.G);
                writeInfo.Write(Cloud[i].Color.B);
                writeInfo.Write(Cloud[i].Color.A);
            }
        }

        public void ImportCloud(string path)
        {
            AsyncFlag = true;
            string[] files = Directory.GetFiles(path, "*-color.jpg");
            foreach (string file in files)
            {
                ImportKinectFile(file.Substring(0, file.Length - 10));
            }
            AsyncFlag = false;
        }

        private void ImportKinectFile(string name)
        {
            Matrix M = GetMatrix(name + "-trans.txt");
            Bitmap image = new Bitmap(name + "-color.jpg");
            byte[] depthData = GetDepthData(name + "-depth.gz", image.Width, image.Height);

            AddPointsToCloud(image.Width, image.Height, M, image, depthData);
        }

        private void AddPointsToCloud(int imageX, int imageY, Matrix M, Bitmap image, byte[] depthData)
        {
            if ((Cloud.Length - Count) < 2 * imageX * imageY)
            {
                VertexPositionColor[] expandedCloud = new VertexPositionColor[Count + (imageX * imageY) * 2];
                Cloud.CopyTo(expandedCloud, 0);
                Cloud = expandedCloud;
            }

            Matrix M1 = Matrix.CreateScale(1.0f / 10000, (25.4f / 96.0f) / 1000, (-25.4f / 96.0f) / 1000) * M;
            float d;

            for (int x = 0; x < imageX; x++)
            {
                for (int y = 0; y < imageY; y++)
                {
                    d = depthData[(imageX * y + x) * 2] + (depthData[(imageX * y + x) * 2 + 1] << 8);

                    if (d != 0x7FFF && (x % 2 == 0)) 
                    {
                        Cloud[Count].Color = ConvertColor(image.GetPixel(x, y));
                        Cloud[Count].Position = Vector3.Transform(new Vector3(d, (float)x - 320, (float)y - 240), M1);
                        Count++;

                        Cloud[Count].Color = ConvertColor(image.GetPixel(x, y));
                        Cloud[Count].Position = Vector3.Transform(new Vector3(d, (float)x - 320, (float)y - 240), M1);
                        Cloud[Count].Position.X += Eps;
                        Count++;
                    }
                }
            }
        }

        private byte[] GetDepthData(string path, int imageX, int imageY)
        {
            byte[] depthData = new byte[imageX * imageY * 2];
            using (FileStream outFile = File.OpenRead(path))
            {
                using (GZipStream decompress = new GZipStream(outFile, CompressionMode.Decompress))
                {
                    decompress.Read(depthData, 0, imageX * imageY * 2);
                    return depthData;
                }
            }
        }

        private Matrix GetMatrix(String pathTrans)
        {
            char[] array1 = { ' ' };

            using (StreamReader readTransInfo = new StreamReader(pathTrans))
            {
                try
                {
                    string[] information = readTransInfo.ReadLine().Split(array1);
                    Matrix MM = new Matrix(
                        Convert.ToSingle(information[0]),
                        Convert.ToSingle(information[1]),
                        Convert.ToSingle(information[2]),
                        Convert.ToSingle(information[3]),
                        Convert.ToSingle(information[4]),
                        Convert.ToSingle(information[5]),
                        Convert.ToSingle(information[6]),
                        Convert.ToSingle(information[7]),
                        Convert.ToSingle(information[8]),
                        Convert.ToSingle(information[9]),
                        Convert.ToSingle(information[10]),
                        Convert.ToSingle(information[11]),
                        Convert.ToSingle(information[12]),
                        Convert.ToSingle(information[13]),
                        Convert.ToSingle(information[14]),
                        Convert.ToSingle(information[15]));
                    return MM;
                }
                catch (Exception)
                {
                    return Matrix.Identity;
                }
            }
        }

        public Microsoft.Xna.Framework.Color ConvertColor(System.Drawing.Color value)
        {
            return new Microsoft.Xna.Framework.Color(value.R, value.G, value.B, value.A);
        }
    }
}
