using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BuildViewMPU6050
{
    public partial class Game1
    {
        #region Constants
        const int number_of_vertices = 8;
        const int number_of_indices = 36;
        #endregion

        #region Backreference 1 - Creating a basic VertexBuffer


        VertexBuffer vertices;

        void CreateCubeVertexBuffer()
        {
            VertexPositionColor[] cubeVertices = new VertexPositionColor[number_of_vertices];

            cubeVertices[0].Position = new Vector3(-1, -1, -1);
            cubeVertices[1].Position = new Vector3(-1, -1, 1);
            cubeVertices[2].Position = new Vector3(1, -1, 1);
            cubeVertices[3].Position = new Vector3(1, -1, -1);
            cubeVertices[4].Position = new Vector3(-1, 1, -1);
            cubeVertices[5].Position = new Vector3(-1, 1, 1);
            cubeVertices[6].Position = new Vector3(1, 1, 1);
            cubeVertices[7].Position = new Vector3(1, 1, -1);

            cubeVertices[0].Color = Color.Black;
            cubeVertices[1].Color = Color.Red;
            cubeVertices[2].Color = Color.Yellow;
            cubeVertices[3].Color = Color.Green;
            cubeVertices[4].Color = Color.Blue;
            cubeVertices[5].Color = Color.Magenta;
            cubeVertices[6].Color = Color.White;
            cubeVertices[7].Color = Color.Cyan;

            vertices = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, number_of_vertices, BufferUsage.WriteOnly);
            vertices.SetData<VertexPositionColor>(cubeVertices);
        }
        #endregion

        #region Backreference 2 - Creating a basic IndexBuffer

        IndexBuffer indices;

        void CreateCubeIndexBuffer()
        {
            UInt16[] cubeIndices = new UInt16[number_of_indices];

            //bottom face
            cubeIndices[0] = 0;
            cubeIndices[1] = 2;
            cubeIndices[2] = 3;
            cubeIndices[3] = 0;
            cubeIndices[4] = 1;
            cubeIndices[5] = 2;

            //top face
            cubeIndices[6] = 4;
            cubeIndices[7] = 6;
            cubeIndices[8] = 5;
            cubeIndices[9] = 4;
            cubeIndices[10] = 7;
            cubeIndices[11] = 6;

            //front face
            cubeIndices[12] = 5;
            cubeIndices[13] = 2;
            cubeIndices[14] = 1;
            cubeIndices[15] = 5;
            cubeIndices[16] = 6;
            cubeIndices[17] = 2;

            //back face
            cubeIndices[18] = 0;
            cubeIndices[19] = 7;
            cubeIndices[20] = 4;
            cubeIndices[21] = 0;
            cubeIndices[22] = 3;
            cubeIndices[23] = 7;

            //left face
            cubeIndices[24] = 0;
            cubeIndices[25] = 4;
            cubeIndices[26] = 1;
            cubeIndices[27] = 1;
            cubeIndices[28] = 4;
            cubeIndices[29] = 5;

            //right face
            cubeIndices[30] = 2;
            cubeIndices[31] = 6;
            cubeIndices[32] = 3;
            cubeIndices[33] = 3;
            cubeIndices[34] = 6;
            cubeIndices[35] = 7;

            indices = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, number_of_indices, BufferUsage.WriteOnly);
            indices.SetData<UInt16>(cubeIndices);

        }
        #endregion
    }
}
