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
            // Phục hồi session nếu có
            RestoreUnfinishedSession();
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
                    ShowAlert("E011");
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
                ShowAlert("E012");
            }
        }

        //hàm phục hồi session chưa hoàn tất
        private void RestoreUnfinishedSession()
        {
            var unfinishedSession = workSessionService.GetUnfinishedSession();
            if (unfinishedSession != null)
            {
                currentSessionID = unfinishedSession.SessionId;
                currentProductCode = productServices.GetProductCodeById(unfinishedSession.ProductId);
                expectedTrayCount = unfinishedSession.ExpectedTrayCount ?? 0;

                // Vẽ toàn bộ slot trắng
                RenderTraySlots(expectedTrayCount);

                // Load các tray đã quét
                var scannedTrays = trayScanService.GetTrayScansBySessionId(currentSessionID);

                int index = 0;
                foreach (var tray in scannedTrays)
                {
                    try
                    {
                        var trayInfo = ScannerHandleServices.ExtractQRTrayInfo(tray.TrayQrcode);
                        trayQRCodes.Add(trayInfo);
                        UpdateTraySlot(index);
                        index++;
                    }
                    catch (Exception ex)
                    {
                        Log($"⚠️ Lỗi khi phân tích QR khay: {ex.Message}");
                    }
                }

                Log($"⚠️ Phục hồi session chưa hoàn tất - ID = {currentSessionID}, đã quét {trayQRCodes.Count}/{expectedTrayCount} khay");

                txtStatus.Text = trayQRCodes.Count == expectedTrayCount
                    ? "✅ Đã đủ khay, hãy quét hộp."
                    : $"✔️ Đã quét {trayQRCodes.Count}/{expectedTrayCount} khay";
            }
            else
            {
                Log("✅ Không có session chưa hoàn tất.");
            }
        }

        // hàm nhận dữ liệu từ COM5 và xử lý dữ liệu
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

        // HÀM xử lý quét khay
        private void HandleTrayScan(string raw)
        {
            //trích xuất dữ liệu từ QR code đọc được
            var trayInfo = ScannerHandleServices.ExtractQRTrayInfo(raw);

            // E001: MÃ QR KHÔNG HỢP LỆ! KHÔNG TỒN TẠI TRONG MASTER
            if (!productServices.IsProductCodeExist(trayInfo.ProductCode))
            {
                Log($"Mã QR không hợp lệ, không tồn tại trong MASTER");
                ShowAlert("E001");
                return;
            }

            if (currentProductCode == null)
            {
                currentProductCode = trayInfo.ProductCode;
                expectedTrayCount = int.TryParse(trayInfo.TrayPerBox, out int count) ? count : 0;
                RenderTraySlots(expectedTrayCount);

                // Tạo session mới trong database
                currentSessionID = workSessionService.CreateNewSession(currentProductCode, expectedTrayCount);

                Log("→ Đã tạo session mới ID = " + currentSessionID);
            }
            // E002: MÃ KHAY KHÔNG KHỚP VỚI MÃ CỦA PHIÊN HIỆN TẠI
            if (trayInfo.ProductCode != currentProductCode)
            {
                txtStatus.Text = ErrorMessageProvider.GetMessage("E002");
                ShowAlert("E002");
                return;
            }

            // E003: MÃ KANBAN ĐÃ TỒN TẠI TRONG PHIÊN
            if (trayQRCodes.Any(x => x.KanbanSequence == trayInfo.KanbanSequence))
            {
                Log($"❌ Mã Kanban {trayInfo.KanbanSequence} đã được quét trong phiên này!");
                ShowAlert("E003");
                return;
            }

            // Kiểm tra xem đã đủ số lượng khay cần quét chưa
            if (trayQRCodes.Count >= expectedTrayCount)
            {
                txtStatus.Text = "✅ Đã đủ khay, hãy quét hộp.";
                return;
            }

            // Thêm thông tin khay vào danh sách
            trayQRCodes.Add(trayInfo);

            // Cập nhật giao diện hiển thị khay
            UpdateTraySlot(trayQRCodes.Count - 1);

            // ✅ lấy ra GeneratedTrayID dựa vào content QR Code đọc được từ scanner
            int? generatedTrayID = generatedTrayService.GetGeneratedTrayIDByQRCodeContent(raw);

            // E004: KHÔNG TÌM THẤY MÃ TRONG CSDL
            if (generatedTrayID == null)
            {
                Log("❌ Không tìm thấy mã QR trong bảng GeneratedTray!");
                ShowAlert("E004");
                return;
            }
            //
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

            //Toán tử ba ngôi
            txtStatus.Text = trayQRCodes.Count == expectedTrayCount ? "✅ Đã đủ khay, hãy quét hộp." : $"✔️ Đã quét {trayQRCodes.Count}/{expectedTrayCount} khay";
        }

        // Hàm xử lý quét hộp
        private void HandleBoxScan(string raw)
        {
            // ✅ Kiểm tra nếu chưa bắt đầu session (chưa quét khay đầu tiên)
            if (trayQRCodes.Count == 0)
            {
                txtStatus.Text = "❌ Chưa quét khay nào. Hãy quét khay trước khi quét hộp!";
                Log("⚠️ Quét hộp nhưng chưa có session khay nào!");
                ShowAlert("E005"); // CHƯA QUÉT KHAY! Hãy quét khay trước khi quét hộp.
                return;
            }
            //check xem đã quẹt đủ khay chưa
            if (trayQRCodes.Count < expectedTrayCount)
            {
                txtStatus.Text = "❌ Chưa đủ khay, hãy quét đủ trước khi quét hộp!";
                ShowAlert("E006"); // Chưa đủ khay, hãy quét đủ trước khi quét hộp!
                return;
            }

            //lưu thông tin của box
            var boxInfo = ScannerHandleServices.ExtractQRBoxInfo(raw);

            // Kiểm tra xem mã QR có hợp lệ không
            if (!productServices.IsProductCodeExist(boxInfo.ProductCode))
            {
                Log($"Mã QR không hợp lệ");
                ShowAlert("E007"); // MÃ QR KHÔNG HỢP LỆ - KHÔNG NẰM TRONG MASTER!
                return;
            }
            // Kiểm tra xem mã hộp có khớp với mã khay hay không
            if (boxInfo.ProductCode != currentProductCode)
            {
                txtStatus.Text = "❌ Mã hộp không khớp với mã session khay!";
                ShowAlert("E009");
                return;
            }

            //kiểm tra xem hộp đang quét đã từng được quét chưa
            if (workSessionService.IsBoxScanned(boxInfo.BoxSequence))
            {
                txtStatus.Text = "❌ Hộp đã được quét trong phiên trước đó!";
                Log($"⚠️ Hộp {boxInfo.BoxSequence} đã được quét trước đó.");
                ShowAlert("E008"); // Hộp đã được quét trong phiên trước đó!
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
                ScanTime = DateTime.Now.TimeOfDay,
                IsCompleted = "done"
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

        // Hàm cập nhật màu sắc và thông tin cho từng slot khay
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

        // Hàm reset session, xóa hết dữ liệu đã quét
        private void ResetSession()
        {
            trayQRCodes.Clear();
            boxQR = null;
            currentProductCode = null;
            expectedTrayCount = 0;
            currentSessionID = 0;
            RenderTraySlots(0);
        }

        // Hàm ghi log vào TextBox
        private void Log(string message)
        {
            txtLog.Text += $"{DateTime.Now:HH:mm:ss} - {message}\n";
            txtLog.ScrollToEnd();
        }

        // Hàm xử lý sự kiện khi nhấn nút "Quay lại"
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem session đang hoạt động
            if (currentProductCode != null || trayQRCodes.Count > 0)
            {
                ShowAlert("E010");
                return;
            }

            // Nếu không có session, quay lại MainWindow
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void ShowAlert(string errorKey)
        {
            string message = ErrorMessageProvider.GetMessage(errorKey);
            CustomAlert alert = new CustomAlert(message);
            //alert.Owner = this;
            alert.ShowDialog();
        }
    }
}

//
