using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BuildViewEditorControl
{
    public class Controller
    {
        private Model model;

        public Controller(Model m_model)
        {
            model = m_model;
        }

        public BuildViewEditorControl Controls
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public void ZoomIn()
        {
            model.WorldMatrix *= Matrix.CreateScale(1.01f);
        }

        public void ZoomOut()
        {
            model.WorldMatrix *= Matrix.CreateScale(1 / 1.01f);
        }

        public void RotateX(float angle)
        {
            model.ViewMatrix *= Matrix.CreateRotationX(MathHelper.ToRadians(angle));
        }

        public void RotateY(float angle)
        {
            model.ViewMatrix *= Matrix.CreateRotationY(MathHelper.ToRadians(angle));
        }

        public void RotateZ(float angle)
        {
            model.ViewMatrix *= Matrix.CreateRotationZ(MathHelper.ToRadians(angle));
        }

        public void MoveUp()
        {
            model.ViewMatrix *= Matrix.CreateTranslation(new Vector3(0, 0, 0.005f));
        }

        public void MoveDown()
        {
            model.ViewMatrix *= Matrix.CreateTranslation(new Vector3(0, 0, -0.005f));
        }

        public void MoveLeft()
        {
            model.ViewMatrix *= Matrix.CreateTranslation(new Vector3(0.005f, 0, 0));
        }

        public void MoveRight()
        {
            model.ViewMatrix *= Matrix.CreateTranslation(new Vector3(-0.005f, 0, 0));
        }

        public void View1()
        {
            model.ViewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
        }

        public void View2()
        {
            model.ViewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up) * Matrix.CreateScale(10);
        }
    }
}
