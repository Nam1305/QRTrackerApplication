//using System;
//using System.Collections.Generic;
//using System.IO.Ports;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;
//using System.IO.Ports;
//using DataAccess.DTO;

using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DataAccess.DTO;
using Services;

namespace QRTrackerApp
{
    /// <summary>
    /// Interaction logic for QRTracker.xaml
    /// </summary>
    public partial class QRTracker : Window
    {
        private SerialPort COM3;
        private SerialPort COM4;
        private List<QRInfo> trayQRCodes = new();
        private QRInfo? boxQR = null;
        private string? currentProductCode = null;
        private int expectedTrayCount = 0;


        public QRTracker()
        {
            InitializeComponent();
            InitPorts();
        }

        private void InitPorts()
        {
            COM3 = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
            COM3.DataReceived += COM3_DataReceived;
            COM3.Open();

            COM4 = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
            COM4.Open();
        }

        private void COM3_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var service = new ScannerHandleServices();
            string raw = COM3.ReadExisting();
            Dispatcher.Invoke(() =>
            {
                Log($"Đã nhận: {raw}");
                if (service.IsTray(raw))
                {
                    var trayInfo = ScannerHandleServices.ExtractQRTrayInfo(raw);
                    if (currentProductCode == null)
                    {
                        currentProductCode = trayInfo.ProductCode;
                        expectedTrayCount = int.TryParse(trayInfo.TrayPerBox, out int count) ? count : 0;
                        RenderTraySlots(expectedTrayCount);
                        Log("→ Tạo session mới cho mã: " + currentProductCode);
                    }

                    if (trayInfo.ProductCode != currentProductCode)
                    {
                        txtStatus.Text = "❌ Mã khay không khớp session!";
                        return;
                    }

                    if (trayQRCodes.Count >= expectedTrayCount)
                    {
                        txtStatus.Text = "✅ Đã đủ khay, hãy quét hộp.";
                        return;
                    }

                    trayQRCodes.Add(trayInfo);
                    UpdateTraySlot(trayQRCodes.Count - 1);

                    if (trayQRCodes.Count == expectedTrayCount)
                    {
                        txtStatus.Text = "✅ Đã đủ khay, hãy quét hộp.";
                    }
                    else
                    {
                        txtStatus.Text = $"✔️ Đã quét {trayQRCodes.Count}/{expectedTrayCount} khay";
                    }
                }
                else if (service.IsBox(raw))
                {
                    if (trayQRCodes.Count < expectedTrayCount)
                    {
                        txtStatus.Text = "❌ Chưa đủ khay, hãy quét đủ trước khi quét hộp!";
                        return;
                    }

                    var boxInfo = ScannerHandleServices.ExtractQRBoxInfo(raw);

                    if (boxInfo.ProductCode != currentProductCode)
                    {
                        txtStatus.Text = "❌ Mã hộp không khớp với mã session khay!";
                        return;
                    }

                    boxQR = boxInfo;

                    try
                    {
                        COM4.Write(raw); // Gửi dữ liệu hộp tới QRClient qua COM4
                        txtStatus.Text = "📦 Đã quét hộp và gửi tới QRClient.";
                        Log("→ Đã gửi QR hộp qua COM4: " + raw);
                    }
                    catch (Exception ex)
                    {
                        txtStatus.Text = "❌ Lỗi gửi dữ liệu COM4: " + ex.Message;
                        return;
                    }

                    // Reset session sau khi gửi
                    ResetSession();
                }
            });
        }


        private void RenderTraySlots(int count)
        {
            panelTrays.Children.Clear();
            for (int i = 0; i < count; i++)
            {
                Border slot = new Border
                {
                    Width = 80,
                    Height = 80,
                    Margin = new Thickness(5),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.White
                };
                panelTrays.Children.Add(slot);
            }
        }

        private void UpdateTraySlot(int index)
        {
            if (index >= 0 && index < panelTrays.Children.Count)
            {
                var border = (Border)panelTrays.Children[index];
                border.Background = Brushes.LightGreen;
            }
        }

        private void ResetSession()
        {
            trayQRCodes.Clear();
            boxQR = null;
            currentProductCode = null;
            expectedTrayCount = 0;
            RenderTraySlots(0);
        }

        private void Log(string message)
        {
            txtLog.Text += $"{DateTime.Now:HH:mm:ss} - {message}\n";
            txtLog.ScrollToEnd();
        }
    }
}


//thêm code để sau khi thông báo hãy quét hộp thì người dùng quét hộp dữ liệu sẽ được write vào com4
