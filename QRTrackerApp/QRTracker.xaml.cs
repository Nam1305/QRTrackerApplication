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

        //Hàm khởi tạo các cổng COM
        private void InitPorts()
        {
            COM3 = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
            COM3.DataReceived += COM3_DataReceived;
            COM3.Open();

            COM4 = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
            COM4.Open();
        }

        //hàm nhận dữ liệu từ COM3 và xử lý dữ liệu
        private void COM3_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var service = new ScannerHandleServices();

            //lắng nghe dữ liệu từ máy Scanner
            string raw = COM3.ReadExisting();
            Dispatcher.Invoke(() =>
            {
                //in ra QR Content đã nhận đưuọc 
                Log($"Đã nhận: {raw}");

                if (service.IsTray(raw))//kiểm tra xem có phải khay không
                {
                    //trích xuất các thành phần của QR CODE
                    var trayInfo = ScannerHandleServices.ExtractQRTrayInfo(raw);

                    if (currentProductCode == null)//xem mã hiện tại đã tồn tại hay chưa
                    {
                        //gán cho mã hiện tại là mã vừa đưuọc quét
                        currentProductCode = trayInfo.ProductCode;

                        //lấy số lượng khay / 1 hộp cần phải quét
                        expectedTrayCount = int.TryParse(trayInfo.TrayPerBox, out int count) ? count : 0;

                        //Render số lượng ô cần thiết
                        RenderTraySlots(expectedTrayCount);

                        Log("→ Tạo session mới cho mã: " + currentProductCode);
                    }

                    //trường hợp quét QR tiếp theo không trùng QR code của khay trước nó
                    if (trayInfo.ProductCode != currentProductCode)
                    {
                        txtStatus.Text = "❌ Mã khay không khớp session!";
                        return;
                    }

                    //tính toán số lượng quẹt
                    if (trayQRCodes.Count >= expectedTrayCount)
                    {
                        txtStatus.Text = "✅ Đã đủ khay, hãy quét hộp.";
                        return;
                    }

                    //Add Tray QR code vào listTrayQRCode
                    trayQRCodes.Add(trayInfo);

                    //cập nhật số lượng cần quét
                    UpdateTraySlot(trayQRCodes.Count - 1);

                    if (trayQRCodes.Count == expectedTrayCount)
                    {
                        //sau này sẽ chuyển thành pop-up
                        txtStatus.Text = "✅ Đã đủ khay, hãy quét hộp.";
                    }
                    else
                    {
                        txtStatus.Text = $"✔️ Đã quét {trayQRCodes.Count}/{expectedTrayCount} khay";
                    }
                }

                //kiểm tra xem là hộp ko
                else if (service.IsBox(raw))
                {
                    //check xem đã quẹt đủ khay chưa
                    if (trayQRCodes.Count < expectedTrayCount)
                    {
                        txtStatus.Text = "❌ Chưa đủ khay, hãy quét đủ trước khi quét hộp!";
                        return;
                    }
                    //trích xuất thông tin hộp
                    var boxInfo = ScannerHandleServices.ExtractQRBoxInfo(raw);

                    //
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

        //Hàm render ra những border lên front end
        private void RenderTraySlots(int count)
        {
            panelTrays.Children.Clear();

            for (int i = 0; i < count; i++)
            {
                Border slot = new Border
                {
                    Margin = new Thickness(5),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.White,
                    MinWidth = 130,
                    MinHeight = 130
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

                var trayInfo = trayQRCodes[index];

                var textBlock = new TextBlock
                {
                    Text = $"Product: {trayInfo.ProductCode}\n" +
                           $"Qty/Tray: {trayInfo.QuantityPerTray}\n" +
                           $"Tray/Box: {trayInfo.TrayPerBox}\n" +
                           $"Qty/Box: {trayInfo.QuantityPerBox}\n" +
                           $"Kanban: {trayInfo.KanbanSequence}",
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 16, // phóng to chữ
                    FontWeight = FontWeights.Bold, // in đậm
                    Margin = new Thickness(5)
                };

                border.Child = textBlock;
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


