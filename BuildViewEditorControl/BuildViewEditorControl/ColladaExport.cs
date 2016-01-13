using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace BuildViewEditorControl
{
    public class ColladaExport
    {
        #region Fields
        private VertexPositionColor[] Cloud;
        private long Count;
        private int[] MeshIndexes;
        private long MeshCount;
        #endregion

        public void ExportKeyData(string keyName, StreamWriter w)
        {
            switch (keyName)
            {
                case "VertexCount":
                    w.Write(Count/2 * 3);
                    break;
                case "VertexData":
                    for (int i = 0; i < Count; i += 2)
                    {
                        w.Write(Math.Round(Cloud[i].Position.X, 3));
                        w.Write(" ");
                        w.Write(Math.Round(Cloud[i].Position.Y, 3));
                        w.Write(" ");
                        w.Write(Math.Round(Cloud[i].Position.Z, 3));
                        w.Write(" ");
                    }
                    break;
                case "VertexDataSize":
                    w.Write(Count / 2);
                    break;
                case "NormalsCount":
                    w.Write((Count / 2) * 3);
                    break;
                case "NormalsData":
                    for (int i = 0; i < Count; i += 2)
                    {
                        w.Write("0 0 1 ");
                    }
                    break;
                case "NormalsDataSize":
                    w.Write(Count / 2);
                    break;
                case "ColorCount":
                    w.Write((Count / 2) * 3);
                    break;
                case "ColorData":
                    for (int i = 0; i < Count; i += 2)
                    {
                        w.Write(Cloud[i].Color.R / 255);
                        w.Write(" ");
                        w.Write(Cloud[i].Color.G / 255);
                        w.Write(" ");
                        w.Write(Cloud[i].Color.B / 255);
                        w.Write(" ");
                        w.Write(Cloud[i].Color.A / 255);
                        w.Write(" ");
                    }
                    break;
                case "ColorDataSize":
                    w.Write(Count / 2);
                    break;
                case "TrianglesCount":
                    w.Write(MeshCount / 3);
                    break;
                case "VertexCountTriangles":
                    for (int k = 0; k < (MeshCount / 3); k++)
                    {
                        w.Write(3);
                        w.Write(" ");
                    }
                    break;
                case "TrianglesIndexes":
                    for (int j = 0; j < MeshCount; j++)
                    {
                        w.Write(MeshIndexes[j] / 2);
                        w.Write(" ");
                        w.Write(MeshIndexes[j] / 2);
                        w.Write(" ");
                        w.Write(MeshIndexes[j] / 2);
                        w.Write(" ");
                    }
                    break;
            };
        }

        public void ExportTemplatedData(string templateFileName, string exportFileName)
        {
            using (StreamWriter w = File.CreateText(exportFileName))
            {
                using (StreamReader r = File.OpenText(templateFileName))
                {
                    string t = r.ReadToEnd();
                    int i1 = 0;
                    int i = 0;
                    int l = t.Length;

                    while (i < l)
                    {
                        while (i < l && t[i] != '@')
                        {
                            i++;
                        }

                        if (i < l)
                        {
                            w.Write(t.ToCharArray(), i1, i - i1);
                            int i2 = t.IndexOf('@', i + 1);
                            string key = t.Substring(i + 1, i2 - i - 1);
                            ExportKeyData(key, w);
                            i1 = i = i2 + 1;
                        }
                        else
                        {
                            w.Write(t.ToCharArray(), i1, i - i1);
                        }
                    }
                }
            }
        }

        public void Export(VertexPositionColor[] aCloud, long aCount, int[] aMeshIndexes, long aMeshCount, string name)
        {
            Cloud = aCloud;
            Count = aCount;
            MeshIndexes = aMeshIndexes;
            MeshCount = aMeshCount;

            ExportTemplatedData("Template.txt", name);
        }
    }
}
