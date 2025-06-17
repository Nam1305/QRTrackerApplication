using System.IO.Packaging;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DataAccess.DTO;
using DataAccess.Models;
using Services;

namespace QRTrackerApp
{
    /// <summary>
    /// Interaction logic for QRTracker.xaml, COM5 đại diện cho QRTrackerApp
    /// </summary>
    public partial class QRTracker : Window
    {
        private SerialPort COM5;
        private SerialPort COM4;
        private List<QRInfo> trayQRCodes = new();
        private QRInfo? boxQR = null;
        private string? currentProductCode = null;
        private string? checkProductCode = null;
        private int expectedTrayCount = 0;
        ProductServices productServices;
        WorkSessionServices workSessionService;
        GeneratedTrayServices generatedTrayService;
        TrayScanServices trayScanService;
        private int currentSessionID = 0;


        public QRTracker()
        {
            InitializeComponent();
            productServices = new ProductServices();
            workSessionService = new WorkSessionServices();
            generatedTrayService = new GeneratedTrayServices();
            trayScanService = new TrayScanServices();
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
                    ShowAlert(" Cổng COM5 không tồn tại. LỖI!");
                    return;
                }
                //khởi tạo COM4:
                COM4 = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
                COM4.Open(); // Mở cổng COM4 để gửi dữ liệu hộp

                // Mở cổng COM5 để nhận dữ liệu
                COM5 = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);
                COM5.DataReceived += COM5_DataReceived;
                COM5.Open();
                Log("✅ Đã mở cổng COM5 để nhận dữ liệu QR.");
            }
            catch (Exception ex)
            {
                Log($"❌ Lỗi khởi tạo cổng COM5: {ex.Message}");
                ShowAlert(" Lỗi khởi tạo cổng COM5");
            }
        }

        //hàm nhận dữ liệu từ COM5 và xử lý dữ liệu
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

                    HandleTrayScan(raw);
                }

                //kiểm tra xem là hộp ko
                else if (service.IsBox(raw))
                {
                    HandleBoxScan(raw);
                }
            });
        }

        //HÀM HandleTrayScan()
        private void HandleTrayScan(string raw)
        {
            //trích xuất dữ liệu từ QR code đọc được
            var trayInfo = ScannerHandleServices.ExtractQRTrayInfo(raw);

            if (!productServices.IsProductCodeExist(trayInfo.ProductCode))
            {
                Log($"Mã QR không hợp lệ, không tồn tại trong MASTER");
                ShowAlert("MÃ QR KHÔNG HỢP LỆ! Không tồn tại trong MASTER");
                return;
            }

            if (currentProductCode == null)
            {
                currentProductCode = trayInfo.ProductCode;
                expectedTrayCount = int.TryParse(trayInfo.TrayPerBox, out int count) ? count : 0;
                RenderTraySlots(expectedTrayCount);

                // Tạo session mới trong database
                currentSessionID = workSessionService.CreateNewSession(currentProductCode);
                Log("→ Đã tạo session mới ID = " + currentSessionID);
            }

            if (trayInfo.ProductCode != currentProductCode)
            {
                txtStatus.Text = "❌ Mã khay không khớp session!";
                ShowAlert($"❌ Mã khay không khớp session!");
                return;
            }

            if (trayQRCodes.Any(x => x.KanbanSequence == trayInfo.KanbanSequence))
            {
                Log($"❌ Mã Kanban {trayInfo.KanbanSequence} đã được quét trong phiên này!");
                ShowAlert($"MÃ KANBAN {trayInfo.KanbanSequence} ĐÃ TỒN TẠI TRONG PHIÊN!");
                return;
            }

            if (trayQRCodes.Count >= expectedTrayCount)
            {
                txtStatus.Text = "✅ Đã đủ khay, hãy quét hộp.";
                return;
            }

            trayQRCodes.Add(trayInfo);
            UpdateTraySlot(trayQRCodes.Count - 1);

            // ✅ Tìm GeneratedTrayID
            int? generatedTrayID = generatedTrayService.GetGeneratedTrayIDByQRCodeContent(raw);
            if (generatedTrayID == null)
            {
                Log("❌ Không tìm thấy mã QR trong bảng GeneratedTray!");
                ShowAlert("KHÔNG TÌM THẤY MÃ TRONG BẢNG GENERATEDTRAY!");
                return;
            }

            // ✅ Lưu TrayScan vào database
            trayScanService.SaveTrayScan(new TrayScan
            {
                TrayQrcode = raw,
                ScanDate = DateOnly.FromDateTime(DateTime.Now),
                ScanTime = DateTime.Now.TimeOfDay,
                GeneratedTrayId = generatedTrayID.Value,
                SessionId = currentSessionID
            });

            Log("→ Đã lưu QR khay vào database.");

            txtStatus.Text = trayQRCodes.Count == expectedTrayCount
                ? "✅ Đã đủ khay, hãy quét hộp."
                : $"✔️ Đã quét {trayQRCodes.Count}/{expectedTrayCount} khay";
        }

        private void HandleBoxScan(string raw)
        {
            // ✅ Kiểm tra nếu chưa bắt đầu session (chưa quét khay đầu tiên)
            if (trayQRCodes.Count == 0)
            {
                txtStatus.Text = "❌ Chưa quét khay nào. Hãy quét khay trước khi quét hộp!";
                Log("⚠️ Quét hộp nhưng chưa có session khay nào!");
                ShowAlert("CHƯA QUÉT KHAY! Hãy quét khay trước khi quét hộp.");
                return;
            }
            //check xem đã quẹt đủ khay chưa
            if (trayQRCodes.Count < expectedTrayCount)
            {
                txtStatus.Text = "❌ Chưa đủ khay, hãy quét đủ trước khi quét hộp!";
                return;
            }

            var boxInfo = ScannerHandleServices.ExtractQRBoxInfo(raw);
            if (!productServices.IsProductCodeExist(boxInfo.ProductCode))
            {
                Log($"Mã QR không hợp lệ");
                ShowAlert("MÃ QR KHÔNG HỢP LỆ!");
                return;
            }
            // Kiểm tra xem mã hộp có khớp với mã khay hay không
            if (boxInfo.ProductCode != currentProductCode)
            {
                txtStatus.Text = "❌ Mã hộp không khớp với mã session khay!";
                return;
            }

            //kiểm tra xem đã quét hộp chưa
            if (workSessionService.IsBoxScanned(boxInfo.BoxSequence))
            {
                txtStatus.Text = "❌ Hộp đã được quét trong phiên trước đó!";
                Log($"⚠️ Hộp {boxInfo.BoxSequence} đã được quét trước đó.");
                ShowAlert($"HỘP {boxInfo.BoxSequence} ĐÃ ĐƯỢC QUÉT TRƯỚC ĐÓ! VUI LÒNG SỬ DỤNG HỘP KHÁC");
                return;
            }

            boxQR = boxInfo;
            txtStatus.Text = "📦 Đã quét hộp và gửi tới QRClient.";

            // Gửi dữ liệu hộp tới COM4
            COM4.Write(raw); 

            Log("→ Đã xử lý QR hộp: " + raw);
            // ✅ Cập nhật thông tin còn thiếu vào WorkSession
            workSessionService.UpdateSessionInfo(currentSessionID, new WorkSession
            {
                BoxSequence = boxInfo.BoxSequence,
                ScanDate = DateOnly.FromDateTime(DateTime.Now),
                ScanTime = DateTime.Now.TimeOfDay
            });

            // Reset session sau khi gửi
            ResetSession();
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
            currentSessionID = 0;
            RenderTraySlots(0);
        }

        private void Log(string message)
        {
            txtLog.Text += $"{DateTime.Now:HH:mm:ss} - {message}\n";
            txtLog.ScrollToEnd();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem session đang hoạt động
            if (currentProductCode != null || trayQRCodes.Count > 0)
            {
                ShowAlert("HOÀN THIỆN SESSION TRƯỚC KHI QUAY VỀ MENU!");
                return;
            }

            // Nếu không có session, quay lại MainWindow
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }


        private void ShowAlert(string message)
        {
            CustomAlert alert = new CustomAlert(message);
            alert.Owner = this;
            alert.ShowDialog();
        }
    }
}


//
