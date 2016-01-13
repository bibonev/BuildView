using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Text;

namespace BuildViewMPU6050
{
    public partial class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        KeyboardState currentKeys;

        Effect simpleColorEffect;
        Quaternion exam = new Quaternion();

        Matrix World, Projection, View;

        private SerialPort comport = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);
        StringBuilder serialData = new StringBuilder();

        Vector3 aaReal = new Vector3(0, 0, 0);
        Vector3 oldDisplacment = new Vector3(0, 0, 0);
        Vector3 center = new Vector3(0, 0, 0);
        Vector3 celAaReal = new Vector3(0, 0, 0);

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        long oldSw;

        float u = 0.0f;
        const int maxCalib = 500;
        int flag = maxCalib;
        float maxCx = float.MinValue;
        float minCx = float.MaxValue;
        float maxCy = float.MinValue;
        float minCy = float.MaxValue;
        float noiseX, noiseY;

        float averageX = 0;
        float averageY = 0;
        bool oneTimeFlag;

        //TheFilter filterY = new TheFilter(0.005f);
        //TheFilter filterX = new TheFilter(0.005f);
        //TheFilter filterZ = new TheFilter(0.005f);

        KalmanFilter k1 = new KalmanFilter();
        KalmanFilter k2 = new KalmanFilter();
        KalmanFilter k3 = new KalmanFilter();

        float sum = 0;
        int counter = 99;
        List<float> averageListX = new List<float>();
        List<float> averageListY = new List<float>();
        List<float> averageListZ = new List<float>();

        Vector3 averageAA = new Vector3();
        Vector3 eulerAngles = new Vector3();
        Vector3 averageEulerAngles = new Vector3();

        float averageListXAv;
        float averageListYAv;
        float averageListZAv;

        VertexPositionColor[] aaRealDraw = new VertexPositionColor[2];
        VertexPositionColor[] averageAADraw = new VertexPositionColor[2];
        VertexPositionColor[] newGDraw = new VertexPositionColor[2];
        VertexPositionColor[] newADraw = new VertexPositionColor[2];
        Quaternion quat = new Quaternion();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            comport.Open();

            sw.Start();
            oldSw = sw.ElapsedMilliseconds;

            k1.Reset(5.0, 5.0, 0.1, 0.0, 0);
            k2.Reset(5.0, 5.0, 0.1, 0.0, 0);
            k3.Reset(5.0, 5.0, 0.1, 0.0, 0);
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //try
            //{
            string data = "";
            data = comport.ReadExisting();

            serialData.Append(data);
            string s = serialData.ToString();

            int p = s.IndexOf('\n');
            if (p >= 0)
            {
                if (p > 0)
                {
                    s = s.Substring(0, p - 1);
                    RecivedData(s);
                }
                serialData.Remove(0, p + 1);
            }
            //}
            /*catch (Exception ex)
            {
                
            }*/
        }

        private void RecivedData(string data)
        {
            char[] array1 = { '\t' };
            string[] s = data.Split(array1);

            if (s[0] == "quat") // get the quaternion of arduino
            {
                exam.W = Convert.ToSingle(s[1]);
                exam.X = Convert.ToSingle(s[2]);
                exam.Y = Convert.ToSingle(s[3]);
                exam.Z = Convert.ToSingle(s[4]);
                Console.WriteLine(exam);
            }
            else if (s[0] == "data")
            {
                try
                {
                    aaReal = new Vector3(Convert.ToSingle(s[1]), Convert.ToSingle(s[2]), Convert.ToSingle(s[3]));
                    quat = new Quaternion(FloatPointing(Convert.ToInt32(s[4])), FloatPointing(Convert.ToInt32(s[5])), FloatPointing(Convert.ToInt32(s[6])), FloatPointing(Convert.ToInt32(s[7])));
                    Vector3 gravity = new Vector3(0.0f, 0.0f, 0.0f);
                    float t = Convert.ToInt32(s[8]) / 1000000f;

                    //averageListX.Add(aaReal.X);
                    //averageListY.Add(aaReal.Y);
                    //averageListZ.Add(aaReal.Z);

                    //if (averageListX.Count() > 100) 
                    //{
                    //    averageListX.RemoveAt(0);
                    //    averageListXAv = averageListX.Average();

                    //    averageListY.RemoveAt(0);
                    //    averageListYAv = averageListY.Average();

                    //    averageListZ.RemoveAt(0);
                    //    averageListZAv = averageListZ.Average();
                    //}

                    //Remove gravity from the raw accel data
                    /*gravity.X = 2 * (quat.X * quat.Z - quat.W * quat.Y);
                    gravity.Y = 2 * (quat.W * quat.X + quat.Y * quat.Z);
                    gravity.Z = quat.W * quat.W - quat.X * quat.X - quat.Y * quat.Y + quat.Z * quat.Z;*/

                    eulerAngles.X = (float)Math.Atan2(2 * quat.X * quat.Y - 2 * quat.W * quat.Z, 2*quat.W*quat.W + 2*quat.X*quat.X - 1);
                    eulerAngles.Y = -(float)Math.Asin(2 * quat.X * quat.Z + 2 * quat.W * quat.Y);
                    eulerAngles.Z = (float)Math.Atan2(2 * quat.Y * quat.Z - 2 * quat.W * quat.X, 2 * quat.W * quat.W + 2 * quat.Z * quat.Z - 1); 

                    if (flag == 0)
                    {
                        Vector3 newA = Vector3.Transform(averageAA, Matrix.CreateRotationX(eulerAngles.X) * Matrix.CreateRotationY(eulerAngles.Y) * Matrix.CreateRotationZ(eulerAngles.Z));
                        Matrix newQ = Matrix.CreateFromQuaternion(quat);
                        newQ = Matrix.Invert(newQ);
                        Vector3 newG = Vector3.Transform(averageAA, newQ);

                            newA.Normalize();   
                            newA *= 2;

                            newADraw[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Green);
                            newADraw[1] = new VertexPositionColor(newA, Color.Green);

                            averageAA.Normalize();
                            averageAA *= 2;

                            averageAADraw[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Red);
                            averageAADraw[1] = new VertexPositionColor(averageAA, Color.Red);

                            newG.Normalize();
                            newG *= 2;

                            newGDraw[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Yellow);
                            newGDraw[1] = new VertexPositionColor(newG, Color.Yellow);

                        //aaReal -= newG;
                        eulerAngles -= averageEulerAngles;

                        //aaReal -= new Vector3(0, 0, 1);

                        aaReal.X = (float)k1.Update((double)aaReal.X, (double)t);
                        aaReal.Y = (float)k2.Update((double)aaReal.Y, (double)t);
                        aaReal.Z = (float)k3.Update((double)aaReal.Z, (double)t);

                            aaReal.Normalize();
                            aaReal *= 2;

                            aaRealDraw[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
                            aaRealDraw[1] = new VertexPositionColor(aaReal, Color.White);

                        //Console.WriteLine(aaReal);

                        //Console.Write("Areal:\t" + "X:\t" + aaReal.X + "\t" + "Y:\t" + aaReal.Y + "\t" + "Z:\t" + aaReal.Z + "\t");
                        //Console.WriteLine("Gravity:\t" + "X:\t" + gravity.X + "\t" + "Y:\t" + gravity.Y + "\t" + "Z:\t" + gravity.Z + "\t");

                        float v = u + (aaReal.Length() * t); //get the speed
                        float roadFl = ((u + v) * t) / 2; //physics formula

                        Vector3 displacement;
                        if (v > u)
                        {
                            aaReal.Normalize();
                            displacement = aaReal * roadFl;
                        }
                        else if (v < u)
                        {
                            aaReal.Normalize();
                            displacement = aaReal * (-roadFl);
                        }
                        else
                        {
                            if (oldDisplacment.Length() > 0)
                            {
                                oldDisplacment.Normalize();
                            }

                            displacement = oldDisplacment * roadFl;
                        }
                        center += displacement;

                        u = v;
                        oldDisplacment = displacement;
                    }
                    else //Calibration
                    {
                        flag--;

                        if (aaReal.X > maxCx) { maxCx = aaReal.X; }
                        if (aaReal.X < minCx) { minCx = aaReal.X; }

                        if (aaReal.Y > maxCy) { maxCy = aaReal.Y; }
                        if (aaReal.Y < minCy) { minCy = aaReal.Y; }

                        averageAA += aaReal;
                        averageEulerAngles += eulerAngles;

                        if (flag == 0)
                        {
                            noiseX = (maxCx - minCx) / 2;
                            noiseY = (maxCy - minCy) / 2;

                            noiseX *= 1.2f;
                            noiseY *= 1.2f;

                            averageAA /= maxCalib;
                            averageEulerAngles /= maxCalib;
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private float FloatPointing(int num)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(num), 0);
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
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            simpleColorEffect = Content.Load<Effect>("SimpleColor");

            World = Matrix.Identity;
            View = Matrix.CreateLookAt(new Vector3(0, 15, 0), Vector3.Zero, Vector3.Forward) * Matrix.CreateScale(0.5f);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 20);
            //--the last parametars is using to locate the camera
            CreateCubeVertexBuffer();
            CreateCubeIndexBuffer();
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
            currentKeys = Keyboard.GetState();

            //Press Esc To Exit
            if (currentKeys.IsKeyDown(Keys.Escape))
                this.Exit();

            World = Matrix.CreateFromQuaternion(quat);
            //World = Matrix.CreateTranslation(center);
            // using quaternion and accelerometar

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.SetVertexBuffer(vertices);
            GraphicsDevice.Indices = indices;

            simpleColorEffect.Parameters["WVP"].SetValue(World * View * Projection);
            simpleColorEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, number_of_vertices, 0, number_of_indices / 3);
            GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, aaRealDraw, 0, 1);
            GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, averageAADraw, 0, 1);
            GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, newADraw, 0, 1);
            GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, newGDraw, 0, 1);

            base.Draw(gameTime);
        }
    }
}
