using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControl;

namespace BuildViewEditorControl
{
    /// <summary>
    /// This is the part, where I read the information, which was created before.
    /// </summary>
    /// 
    public class BuildViewEditorControl : XNAControl.XNAControlGame
    {
        public BasicEffect basicEffect;
        public Effect effect;

        public Model model = new Model();
        public Controller controller;
        public View view;

        public BuildViewEditorControl(IntPtr handle): base(handle, "Content")
        {
            controller = new Controller(model);
            view = new View(model, this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();

            model.WorldMatrix = Matrix.Identity;
            model.ViewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up) * Matrix.CreateScale(10);
            model.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.00001f, 0xFFFF);

            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.View = model.ViewMatrix;
            basicEffect.Projection = model.ProjectionMatrix;
            basicEffect.World = model.WorldMatrix;
            basicEffect.VertexColorEnabled = true;

            try
            {
                model.CloudIndexesBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, model.CloudIndexes.Length, BufferUsage.WriteOnly);
                model.CloudIndexesBuffer.SetData(model.CloudIndexes);
            }
            catch(Exception)
            {
                System.Windows.Forms.MessageBox.Show("There is a problem in importing files. Please, try again!", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);

            for (int i = 0; i < Model.PartSize; i++)
            {
                model.CloudIndexes[i * 2] = i * 2;
                model.CloudIndexes[i * 2 + 1] = i * 2 + 1;
            }

            effect = Content.Load<Effect>("Shaders");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                this.Exit();

            Microsoft.Xna.Framework.Input.KeyboardState keys = Keyboard.GetState();

            //--Zoom
            if (keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.O))
            {
                controller.ZoomIn();
            }
            else if (keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.P))
            {
                controller.ZoomOut();
            }
            //--

            //--Rotate
            if (keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
            {
                controller.RotateX(MathHelper.ToRadians(keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) ? -10 : 10));
            }
            else if (keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
            {
                controller.RotateY(MathHelper.ToRadians(keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) ? -10 : 10));
            }
            else if (keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
            {
                controller.RotateZ(MathHelper.ToRadians(keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) ? -10 : 10));
            }
            //--

            //View
            if (keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D1))
            {
                controller.View1();
            }
            else if (keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D2))
            {
                controller.View2();
            }
            //--

            //Move
            if (keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
            {
                controller.MoveUp();
            }
            else if (keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
            {
                controller.MoveDown();
            }
            else if (keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
            {
                controller.MoveLeft();
            }
            else if (keys.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
            {
                controller.MoveRight();
            }
            //--

            base.Update(gameTime);
        }

        public void Buffers()
        {
            if (model.Cloud.Length == 0)
            {
                return;
            }

            model.AsyncFlag = true;

            if (model.CloudBuffer != null)
            {
                model.CloudBuffer.Dispose();
            }

            model.CloudBuffer = new VertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, model.Cloud.Length, BufferUsage.WriteOnly); //TODO: Clear Buffer 
            model.CloudBuffer.SetData(model.Cloud);
            model.AsyncFlag = false;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (model.AsyncFlag) return;
            view.DrawElements();
            base.Draw(gameTime);
        }
    }
}