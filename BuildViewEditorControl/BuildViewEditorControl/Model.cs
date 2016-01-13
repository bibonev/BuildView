using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BuildViewEditorControl
{
    partial class Model
    {
        public VertexPositionColor[] Cloud = new VertexPositionColor[0];
        public VertexPositionColor[] Triangle = new VertexPositionColor[3];
        public const int PartSize = 1000000;
        public const int PartSizeMesh = 999999;
        public const float Eps = 0.001f;
        public int[] CloudIndexes = new int[PartSize * 2];
        public int[] MeshIndexes = new int[0];
        public long Count = 0, MeshCount = 0;

        public VertexBuffer CloudBuffer;
        public IndexBuffer CloudIndexesBuffer;
        public IndexBuffer MeshIndexesBuffer;

        public Matrix WorldMatrix, ViewMatrix, ProjectionMatrix;

        public bool AsyncFlag;
    }
}
