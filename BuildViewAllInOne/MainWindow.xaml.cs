using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace BuildViewAllInOne
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void radQuadrantNWGauge1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            start.WorkingDirectory = "";
            start.Arguments = "";
            start.FileName = "BuildView3DScan.exe";
            start.WindowStyle = ProcessWindowStyle.Normal;
            start.CreateNoWindow = true;

            using (Process proc = Process.Start(start)){}
        }

        private void radRadialGauge1_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            start.WorkingDirectory = "";
            start.Arguments = "";
            start.FileName = "BuildViewEditor.exe";
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;

            using (Process proc = Process.Start(start)){}
        }

        private void radQuadrantNEGauge1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            start.WorkingDirectory = "";
            start.Arguments = "";
            start.FileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create), "BuildViewExport");
            start.WindowStyle = ProcessWindowStyle.Maximized;
            start.CreateNoWindow = false;

            using (Process proc = Process.Start(start)){}
        }
    }
}
