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
        //private SerialPort COM3;
        private SerialPort COM4;
        private SerialPort COM5;    
        private List<QRInfo> trayQRCodes = new();
        private QRInfo? boxQR = null;
        private string? currentProductCode = null;
        private int expectedTrayCount = 0;


        public QRTracker()
        {
            InitializeComponent();
            InitPorts();
        }

        // Hàm khởi tạo cổng COM5
        private void InitPorts()
        {
            try
            {
                // Kiểm tra cổng COM5 có sẵn không
                if (!SerialPort.GetPortNames().Contains("COM5", StringComparer.OrdinalIgnoreCase))
                {
                    Log("❌ Cổng COM5 không tồn tại. Vui lòng kiểm tra VSPE");
                    MessageBox.Show("Cổng COM5 không tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Mở cổng COM5 để nhận dữ liệu
                COM5 = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);
                COM5.DataReceived += COM5_DataReceived;
                COM5.Open();
                Log("✅ Đã mở cổng COM5 để nhận dữ liệu QR.");
            }
            catch (Exception ex)
            {
                Log($"❌ Lỗi khởi tạo cổng COM5: {ex.Message}");
                MessageBox.Show($"Lỗi khởi tạo cổng COM5: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //hàm nhận dữ liệu từ COM3 và xử lý dữ liệu
        private void COM5_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var service = new ScannerHandleServices();

            //lắng nghe dữ liệu từ máy Scanner
            string raw = COM5.ReadExisting();
            Dispatcher.Invoke(() =>
            {
                //in ra QR Content đã nhận đưuọc 
                Log($"Đã nhận từ COM5: {raw}");

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

                    //Kiếm tra xem mã hộp có khớp với mã khay hay không
                    if (boxInfo.ProductCode != currentProductCode)
                    {
                        txtStatus.Text = "❌ Mã hộp không khớp với mã session khay!";
                        return;
                    }

                    boxQR = boxInfo;
                    txtStatus.Text = "📦 Đã quét hộp và gửi tới QRClient.";
                    Log("→ Đã xử lý QR hộp: " + raw);
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
                    Margin = new Thickness(10),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.White,
                    MinWidth = 260,
                    Height = 250
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

                // Tạo stack chứa từng dòng có border riêng
                StackPanel infoPanel = new StackPanel
                {
                    Margin = new Thickness(5),
                    Orientation = Orientation.Vertical
                };

                void AddRow(string label, string value)
                {
                    Grid rowGrid = new Grid();

                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    var labelText = new TextBlock
                    {
                        Text = label,
                        FontWeight = FontWeights.Bold,
                        FontSize = 16,
                        Margin = new Thickness(5),
                        VerticalAlignment = VerticalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };

                    var valueText = new TextBlock
                    {
                        Text = value,
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(5),
                        VerticalAlignment = VerticalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };

                    Grid.SetColumn(labelText, 0);
                    Grid.SetColumn(valueText, 1);

                    rowGrid.Children.Add(labelText);
                    rowGrid.Children.Add(valueText);

                    // Bọc cả rowGrid trong một border để tạo khung
                    var rowBorder = new Border
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Child = rowGrid,
                        Margin = new Thickness(0, 0, 0, 2) // khoảng cách giữa các dòng
                    };

                    infoPanel.Children.Add(rowBorder);
                }

                AddRow("Product:", trayInfo.ProductCode);
                AddRow("Qty/Tray:", trayInfo.QuantityPerTray);
                AddRow("Tray/Box:", trayInfo.TrayPerBox);
                AddRow("Qty/Box:", trayInfo.QuantityPerBox);
                AddRow("Kanban:", trayInfo.KanbanSequence);

                border.Child = infoPanel;
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



