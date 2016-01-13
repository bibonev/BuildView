using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;
using Parago.Windows;
using System.Windows.Input;
using System.Windows.Forms;

namespace BuildViewEditor
{
    public partial class MainWindow : Window
    {
        BuildViewEditorControl.BuildViewEditorControl buildViewControl;
        string importPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create), "BuildViewScan");
        string exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.Create), "BuildViewExport");

        public MainWindow()
        {
            InitializeComponent();

            buildViewControl = new BuildViewEditorControl.BuildViewEditorControl(xnaControl.Handle);
        }

        private bool BrowseFolder()
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Choose import directory";
                dlg.SelectedPath = importPath;
                dlg.ShowNewFolderButton = true;
                DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    importPath = dlg.SelectedPath;
                    return true;
                }
                return false;
            }
        }

        private void fileImport_Click(object sender, RoutedEventArgs e)
        {
            if (BrowseFolder())
            {
                ProgressDialogResult result = ProgressDialog.Execute(this, "Importing data", (bw, we) =>
                {
                    buildViewControl.model.ImportCloud(importPath);
                    buildViewControl.Buffers();

                }, ProgressDialogSettings.WithSubLabelAndCancel);
                
                if (result.OperationFailed)
                {
                    System.Windows.MessageBox.Show("Processing failed.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    System.Windows.MessageBox.Show("Processing successfully executed.", "Successfull", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void fileExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void fileSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "NSideView Files|*.nsv";
            dialog.FilterIndex = 2;
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.FileStream fs = null;
                if (dialog.FileName != null)
                {
                    try
                    {
                        using (fs = System.IO.File.Create(dialog.FileName))
                        {
                            using (GZipStream gz = new GZipStream(fs, CompressionMode.Compress))
                            {
                                buildViewControl.model.Save(gz);
                            }
                        }

                        System.Windows.MessageBox.Show("Your file has been created successfully.", "Successfull!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception)
                    {
                        System.Windows.MessageBox.Show("Your file has not been created.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void fileOpen_Click(object sender, RoutedEventArgs e)
        {
            #region Code for opening
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "NSideView Files|*.nsv";
            openFile.Title = @"Select a '.nsv' file";

            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    using (FileStream inFile = File.Open(openFile.FileName, FileMode.Open))
                    {
                        using (GZipStream gz = new GZipStream(inFile, CompressionMode.Decompress))
                        {
                            buildViewControl.model.Open(gz);
                            buildViewControl.Buffers();
                        }
                    }

                    System.Windows.MessageBox.Show("Your file has been created successfully.", "Successfull!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("Your file has not been created.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            #endregion
        }

        private void fileExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Collada Files|*.dae";
            dialog.FilterIndex = 2;
            dialog.InitialDirectory = exportPath;
            dialog.RestoreDirectory = true;

            Directory.CreateDirectory(exportPath);

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                exportPath = Path.GetDirectoryName(dialog.FileName);

                System.IO.FileStream fs = null;
                if (dialog.FileName != null)
                {
                    try
                    {
                        buildViewControl.model.Export(dialog.FileName);
                        System.Windows.MessageBox.Show("Your file has been created successfully.", "Successfull!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception)
                    {
                        System.Windows.MessageBox.Show("Your file has not been created.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void meshStour_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            string PCLName = Path.Combine(Path.GetTempPath(), "PCL.pcd");
            string PCLFilteredName = Path.Combine(Path.GetTempPath(), "PCL_filtered.pcd");

            start.WorkingDirectory = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath); 
            start.Arguments = "StatisticalOutlierRemoval";
            start.Arguments += " \"" + PCLName + "\"";
            start.Arguments += " \"" + PCLFilteredName + "\"";
            start.FileName = "BuildViewCloudFilter.exe";
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            

            ProgressDialogResult result = ProgressDialog.Execute(this, "Filtering data", (bw, we) =>
            {
                buildViewControl.model.ExportPcd(PCLName);

                using (Process proc = Process.Start(start))
                {
                    while (!proc.WaitForExit(1000))
                    {
                        if (ProgressDialog.CheckForPendingCancellation(bw, we))
                        {
                            proc.Kill();
                            return;
                        }
                    }
                    
                }

                buildViewControl.model.ImportPcd(PCLFilteredName);
                buildViewControl.Buffers();

            }, ProgressDialogSettings.WithSubLabelAndCancel);

            if (result.OperationFailed)
            {
                System.Windows.MessageBox.Show("Processing failed.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (result.Cancelled)
            {
                File.Delete(PCLName);
                System.Windows.MessageBox.Show("Processing canceled successfully.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                File.Delete(PCLName);
                File.Delete(PCLFilteredName);
                System.Windows.MessageBox.Show("Processing successfully executed.", "Successfull", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void meshSmooth_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            string PCLName = Path.Combine(Path.GetTempPath(), "PCL_smooth.pcd");
            string PCLFilteredName = Path.Combine(Path.GetTempPath(), "PCL_filtered_smooth.pcd");

            start.WorkingDirectory = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            start.Arguments = "Smoothing";
            start.Arguments += " \"" + PCLName + "\"";
            start.Arguments += " \"" + PCLFilteredName + "\"";
            start.FileName = "BuildViewCloudFliter.exe";
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;


            ProgressDialogResult result = ProgressDialog.Execute(this, "Filtering data", (bw, we) =>
            {
                buildViewControl.model.ExportPcd(PCLName);

                using (Process proc = Process.Start(start))
                {
                    while (!proc.WaitForExit(1000))
                    {
                        if (ProgressDialog.CheckForPendingCancellation(bw, we))
                        {
                            proc.Kill();
                            return;
                        }
                    }

                }

                buildViewControl.model.ImportPcd(PCLFilteredName);
                buildViewControl.Buffers();

            }, ProgressDialogSettings.WithSubLabelAndCancel);

            if (result.OperationFailed)
            {
                System.Windows.MessageBox.Show("Processing failed.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (result.Cancelled)
            {
                File.Delete(PCLName);
                System.Windows.MessageBox.Show("Processing canceled successfully.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                File.Delete(PCLName);
                File.Delete(PCLFilteredName);
                System.Windows.MessageBox.Show("Processing successfully executed.", "Successfull", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void meshTriangulation_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            string PCLName = Path.Combine(Path.GetTempPath(), "PCL.pcd");
            string BinName = Path.Combine(Path.GetTempPath(), "meshIndexes.bin");

            start.WorkingDirectory = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath); 
            start.Arguments = "CreateMesh";
            start.Arguments += " \"" + PCLName + "\"";
            start.Arguments += " \"" + BinName + "\"";
            start.FileName = "BuildViewCloudFilter.exe";
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;

            ProgressDialogResult result = ProgressDialog.Execute(this, "Triangulation data", (bw, we) =>
            {
                buildViewControl.model.ExportPcdRgb(PCLName);

                using (Process proc = Process.Start(start))
                {
                    while (!proc.WaitForExit(1000))
                    {
                        if (ProgressDialog.CheckForPendingCancellation(bw, we))
                        {
                            proc.Kill();
                            return;
                        }
                    }
                }

                buildViewControl.model.ImportMesh(BinName);

            }, ProgressDialogSettings.WithSubLabelAndCancel);

            if (result.OperationFailed)
            {
                System.Windows.MessageBox.Show("Processing failed.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (result.Cancelled)
            {
                File.Delete(PCLName);
                System.Windows.MessageBox.Show("Processing canceled successfully.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                File.Delete(PCLName);
                File.Delete(BinName);
                System.Windows.MessageBox.Show("Processing successfully executed.", "Successfull", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            start.WorkingDirectory = "";
            start.Arguments = "";
            start.FileName = "BuildViewDocumentation.pdf";
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;

            using (Process proc = Process.Start(start)) { }
        }

        private void comboBoxColor_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (comboBoxColor.SelectedIndex)
            {
                case 0:
                    buildViewControl.view.Color = Microsoft.Xna.Framework.Color.White;
                    break;

                case 1: buildViewControl.view.Color = Microsoft.Xna.Framework.Color.Gray;
                    break;

                case 2: buildViewControl.view.Color = Microsoft.Xna.Framework.Color.Yellow;
                    break;

                case 3: buildViewControl.view.Color = Microsoft.Xna.Framework.Color.Red;
                    break;

                case 4: buildViewControl.view.Color = Microsoft.Xna.Framework.Color.Blue;
                    break;

                default: buildViewControl.view.Color = Microsoft.Xna.Framework.Color.Black;
                    break;
            }    
        }

        private void comboBoxItemBlack_Selected(object sender, RoutedEventArgs e)
        {
            buildViewControl.view.Color = Microsoft.Xna.Framework.Color.Black;
        }
    }

    static class CustomCommands
    {
        public static RoutedCommand DoStuff = new RoutedCommand();
    }
}
