using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace BuildViewEditorControl
{
    public class View
    {
        private Model model;
        private BuildViewEditorControl control;
        private Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color();

        public View(Model m_model, BuildViewEditorControl m_control)
        {
            model = m_model;
            control = m_control;
        }

        public BuildViewEditorControl DrawMeshPoints
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public Microsoft.Xna.Framework.Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        public void DrawElements()
        {
            control.GraphicsDevice.Clear(color);

            RasterizerState rs = new RasterizerState();
            if (model.MeshCount > 0)
            {
                rs.FillMode = FillMode.Solid;
            }
            else
            {
                rs.FillMode = FillMode.WireFrame;
            }
            rs.FillMode = FillMode.Solid; //--
            rs.CullMode = CullMode.None;
            rs.MultiSampleAntiAlias = true;
            control.GraphicsDevice.RasterizerState = rs;

            control.basicEffect.CurrentTechnique.Passes[0].Apply();

            control.effect.CurrentTechnique = control.effect.Techniques["Technique1"];
            control.effect.Parameters["World"].SetValue(model.WorldMatrix);
            control.effect.Parameters["View"].SetValue(model.ViewMatrix);
            control.effect.Parameters["Projection"].SetValue(model.ProjectionMatrix);

            foreach (EffectPass pass in control.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                control.GraphicsDevice.SetVertexBuffer(model.CloudBuffer);

                if (model.MeshCount > 0)
                {
                    int baseIndex = 0;
                    while (baseIndex < model.MeshCount / 3)
                    {
                        int length = (int)Math.Min(Model.PartSize, (model.MeshCount / 3) - baseIndex);

                        using (model.MeshIndexesBuffer =
                            new IndexBuffer(control.GraphicsDevice,
                                IndexElementSize.ThirtyTwoBits, length * 3, BufferUsage.WriteOnly))
                        {
                            model.MeshIndexesBuffer.SetData(model.MeshIndexes, baseIndex, length * 3);
                            control.GraphicsDevice.Indices = model.MeshIndexesBuffer;
                            control.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, length * 3, 0, length);
                        }

                        baseIndex += Model.PartSize;
                    }
                }
                else
                {
                    control.GraphicsDevice.Indices = model.CloudIndexesBuffer;

                    int baseIndex = 0;
                    while (baseIndex < model.Cloud.Length)
                    {
                        int length = Math.Min(Model.PartSize, model.Cloud.Length - baseIndex);
                        control.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, baseIndex, 0, length * 2, 0, length);
                        baseIndex += Model.PartSize;
                    }
                }
            }
        }
    }
}
