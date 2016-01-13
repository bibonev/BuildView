using System;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Xna.Framework;
using System.IO.Ports;
using System.Windows.Forms;

namespace BuildView3DScan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables

        private KinectSensor sensor;

        //ImageStream
        KinectSensor sensorKinect;
        private byte[] colorPixelData;
        private WriteableBitmap outputImage;

        // color divisors for tinting depth pixels 
        private static readonly int[] intensityShiftByPlayerR = { 1, 2, 0, 2, 0, 0, 2, 0 };
        private static readonly int[] intensityShiftByPlayerG = { 1, 2, 2, 0, 2, 0, 0, 1 };
        private static readonly int[] intensityShiftByPlayerB = { 1, 0, 2, 2, 0, 2, 0, 2 };

        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;

        int counter = 0;
        int countWhile = 0;
        string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create), "BuildViewScan");
        byte[] binaryDepth = new byte[0];
        private DepthImagePixel depthImagePixel;

        DateTime now;

        //Quaternion
        string comportName = "COM3";
        private SerialPort comport; 
        StringBuilder serialData = new StringBuilder();
        Quaternion exam = new Quaternion();
        Microsoft.Xna.Framework.Matrix endData = new Microsoft.Xna.Framework.Matrix();


        #endregion

        public MainWindow()
        {
            InitializeComponent();

            capturePath.Text = savePath;

            //ConnectCheck
            if (KinectSensor.KinectSensors.Count == 0)
            {
                System.Windows.MessageBox.Show("There is a device that is not connected successfully!\nPlease, try again after connect the device.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    comportBox.Items.Add(port);
                }
                comportBox.Items.Add("COM1"); //Demo COM1 - tests
                if (ports.Length > 0)
                {
                    comportName = comportBox.Items[0].ToString();
                    comportBox.Text = comportName;
                    comport = new SerialPort(comportName, 115200, Parity.None, 8, StopBits.One);
                }

                sensorKinect = KinectSensor.KinectSensors[0];
                sensorKinect.DepthStream.Range = DepthRange.Default;
                
                //ImageStream
                sensorKinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                sensorKinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(Sensor_ColorFrameReady);

                //DepthStream
                sensorKinect.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                sensorKinect.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(Sensor_DepthFrameReady);
                sensorKinect.Start();

                //Qaternion
                comport.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);
                comport.Open();
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = "";
                data = comport.ReadExisting();

                serialData.Append(data);
                string s = serialData.ToString();

                int p = s.IndexOf('\n');
                if (p >= 0)
                {
                    s = s.Substring(0, p - 1);
                    RecivedData(s);
                    serialData.Remove(0, p + 1);
                }
            }
            catch (Exception ex) { }
        }

        private void RecivedData(string data)
        {
            char[] array1 = { '\t' };
            string[] s = data.Split(array1);
            Vector3 pitchYawRoll = new Vector3();
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (s[0] == "quat") // get the quaternion of arduino
                {
                    try
                    {
                        exam.X = FloatPointing(s[1]);
                        exam.Y = FloatPointing(s[2]);
                        exam.Z = FloatPointing(s[3]);
                        exam.W = FloatPointing(s[4]);

                        double sqw = exam.W * exam.W;
                        double sqx = exam.X * exam.X;
                        double sqy = exam.Y * exam.Y;
                        double sqz = exam.Z * exam.Z;

                        endData = Microsoft.Xna.Framework.Matrix.CreateFromQuaternion(exam);

                        pitchYawRoll.Y = (float)Math.Atan2(2f * exam.X * exam.W + 2f * exam.Y * exam.Z, 1 - 2f * (sqz + sqw));     
                        pitchYawRoll.X = (float)Math.Asin(2f * (exam.X * exam.Z - exam.W * exam.Y));                             
                        pitchYawRoll.Z = (float)Math.Atan2(2f * exam.X * exam.Y + 2f * exam.Z * exam.W, 1 - 2f * (sqy + sqz));

                        Console.WriteLine("Euler: {0}", pitchYawRoll);

                        transformationBox.Text = String.Format(
                        "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15}",
                        endData.M11, endData.M12, endData.M13, endData.M14,
                        endData.M21, endData.M22, endData.M23, endData.M24,
                        endData.M31, endData.M32, endData.M33, endData.M34,
                        endData.M41, endData.M42, endData.M43, endData.M44);

                    }
                    catch {}
                }
            }));
        }

        private float FloatPointing(String num)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(Convert.ToInt32(num)), 0);
        }

        void Sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            counter++;

            if (counter % 6 != 0)
            {
                return;
            }

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    //Using standard SDK
                    this.colorPixelData = new byte[colorFrame.PixelDataLength];

                    colorFrame.CopyPixelDataTo(this.colorPixelData);

                    this.outputImage = new WriteableBitmap(
                    colorFrame.Width,
                    colorFrame.Height,
                    96,  // DpiX
                    96,  // DpiY
                    PixelFormats.Bgr32,
                    null);

                    this.outputImage.WritePixels(
                    new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                    this.colorPixelData,
                    colorFrame.Width * 4,
                    0);

                    this.imgStream.Source = this.outputImage;
                }
            }
        }

        void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            if (counter % 6 != 0)
            {
                return;
            }

            WriteableBitmap outputBitmap;
            byte[] depthFrame32;
            DepthImagePixel[] depthPixelData;

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null && sensorKinect != null)
                {
                    //Using standard SDK
                    depthPixelData = new DepthImagePixel[sensorKinect.DepthStream.FramePixelDataLength];
                    depthFrame.CopyDepthImagePixelDataTo(depthPixelData);

                    depthFrame32 = new byte[depthFrame.Width * depthFrame.Height * 4]; //To form an RGB image
                    byte[] convertedDepthBits = ConvertDepthFrame(depthPixelData, depthFrame32);

                    outputBitmap = new WriteableBitmap
                    (
                        depthFrame.Width,
                        depthFrame.Height,
                        96,  //DpiX 
                        96,  // DpiY 
                        PixelFormats.Bgr32,
                        null
                    );

                    outputBitmap.WritePixels(
                    new Int32Rect(0, 0, depthFrame.Width, depthFrame.Height),
                    convertedDepthBits,
                    depthFrame.Width * 4,
                    0);

                    this.depthStream.Source = outputBitmap;
                }
            }
        }

        private byte[] ConvertDepthFrame(DepthImagePixel[] depthFrame, byte[] depthFrame32)
        {
            binaryDepth = new byte[640 * 480 * 2];
            int countBinaryDepth = 0;

            for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < depthFrame32.Length; i16++, i32 += 4)
            {
                short realDepth;
                if (depthFrame[i16].IsKnownDepth)
                {
                    realDepth = depthFrame[i16].Depth;
                }
                else
                {
                    realDepth = 0x7FFF;
                }



                short intensity = (short)realDepth;

                depthFrame32[i32 + RedIndex] = (byte)(intensity >> 3);
                depthFrame32[i32 + GreenIndex] = depthFrame32[i32 + RedIndex];
                depthFrame32[i32 + BlueIndex] = depthFrame32[i32 + RedIndex];

                binaryDepth[countBinaryDepth++] = (byte)(intensity & 255);
                binaryDepth[countBinaryDepth++] = (byte)(intensity >> 8);
            }

            byte[] newBinaryDepth = new byte[countBinaryDepth];
            Array.Copy(binaryDepth, newBinaryDepth, countBinaryDepth);
            binaryDepth = newBinaryDepth;

            return depthFrame32;
        }

        void SaveBinaryDepth(string name)
        {
            using (FileStream outFile = File.Create(name))
            {
                using (GZipStream compress = new GZipStream(outFile, CompressionMode.Compress))
                {
                    compress.Write(binaryDepth, 0, 640 * 480 * 2);
                }
            }
        }

        private void BrowseFolder()
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Choose output directory";
                dlg.SelectedPath = savePath;
                dlg.ShowNewFolderButton = true;
                DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    savePath = dlg.SelectedPath;
                    capturePath.Text = savePath;
                }
            }
        }

        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            now = DateTime.Now;

            Directory.CreateDirectory(savePath);

            BitmapSource colorStream = (BitmapSource)imgStream.Source;
            colorStream.Save(Path.Combine(savePath, now.ToString("ddMMyyyy-HHmmss") + "-color.jpg"), ImageFormat.Jpeg); //ImageStream

            SaveBinaryDepth(Path.Combine(savePath, now.ToString(@"ddMMyyyy-HHmmss") + "-depth.gz")); //DepthStream

            using (StreamWriter writeTransInfo = new StreamWriter(Path.Combine(savePath, now.ToString("ddMMyyyy-HHmmss") + "-trans.txt"), true))
            {
                try
                {
                    writeTransInfo.Write(transformationBox.Text);
                }
                catch
                {
                }
            }
        }

        private void HideImg_Checked(object sender, RoutedEventArgs e)
        {
            imgStream.Visibility = Visibility.Hidden;
        }

        private void HideDep_Checked(object sender, RoutedEventArgs e)
        {
            depthStream.Visibility = Visibility.Hidden;
        }

        private void ButtonConfigureClick(object sender, RoutedEventArgs e)
        {
            BrowseFolder();
        }

        private void comportBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            comportName = comportBox.Items[comportBox.SelectedIndex].ToString();
            if (comport != null)
            {
                comport.Close();
                comport.PortName = comportName;
                try
                {
                    comport.Open();
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("This comport does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
