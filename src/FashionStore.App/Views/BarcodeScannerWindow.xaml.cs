using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using ZXing.Windows.Compatibility;

namespace FashionStore.App.Views
{
    public partial class BarcodeScannerWindow : Window
    {
        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _videoSource;
        private readonly BarcodeReader _barcodeReader;
        
        public string ScannedBarcode { get; private set; } = string.Empty;

        public BarcodeScannerWindow()
        {
            InitializeComponent();
            
            _barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.EAN_13,
                        BarcodeFormat.EAN_8,
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.QR_CODE,
                        BarcodeFormat.CODE_39
                    }
                }
            };

            Loaded += BarcodeScannerWindow_Loaded;
            Closing += BarcodeScannerWindow_Closing;
        }

        private void BarcodeScannerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Start Animation
            var sb = (Storyboard)FindResource("ScannerAnimation");
            sb.Begin();

            // Find cameras
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (_videoDevices.Count == 0)
            {
                MessageBox.Show("Không tìm thấy webcam nào trên máy tính.");
                Close();
                return;
            }

            foreach (FilterInfo device in _videoDevices)
            {
                CameraComboBox.Items.Add(new CameraInfo { Name = device.Name, MonikerString = device.MonikerString });
            }

            CameraComboBox.SelectedIndex = 0;
        }

        private void CameraComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            StopCamera();

            if (CameraComboBox.SelectedItem is CameraInfo info)
            {
                _videoSource = new VideoCaptureDevice(info.MonikerString);
                _videoSource.NewFrame += VideoSource_NewFrame;
                _videoSource.Start();
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    // Convert to BitmapSource for UI
                    Dispatcher.Invoke(() =>
                    {
                        CameraImage.Source = ToBitmapSource(bitmap);
                    });

                    // Try scanning
                    var result = _barcodeReader.Decode(bitmap);
                    if (result != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ScannedBarcode = result.Text;
                            StopCamera();
                            DialogResult = true;
                            Close();
                        });
                    }
                }
            }
            catch { /* Ignore frame errors */ }
        }

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= VideoSource_NewFrame;
                _videoSource = null;
            }
        }

        private void BarcodeScannerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopCamera();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private BitmapSource ToBitmapSource(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
    }

    public class CameraInfo
    {
        public string Name { get; set; } = string.Empty;
        public string MonikerString { get; set; } = string.Empty;
    }
}
