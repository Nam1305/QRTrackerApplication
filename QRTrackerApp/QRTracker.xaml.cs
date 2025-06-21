using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DataAccess.DTO;
using DataAccess.Models;
using Services;
using QRTrackerApp.Handlers;

namespace QRTrackerApp
{
    public partial class QRTracker : Window
    {
        // Handler classes
        private SerialPortManager serialPortManager;
        private SessionManager sessionManager;
        private QRScanHandler qrScanHandler;
        private UILogger uiLogger;
        private AlertService alertService;

        public QRTracker()
        {
            InitializeComponent();

            // Khởi tạo các service
            var productServices = new ProductServices();
            var workSessionService = new WorkSessionServices();
            var generatedTrayService = new GeneratedTrayServices();
            var trayScanService = new TrayScanServices();
            var errorLogService = new ErrorLogServices();

            // Khởi tạo các handler
            sessionManager = new SessionManager(productServices, workSessionService, trayScanService, generatedTrayService);
            uiLogger = new UILogger(txtLog, txtStatus);
            alertService = new AlertService();

            qrScanHandler = new QRScanHandler(
                sessionManager,
                productServices,
                workSessionService,
                generatedTrayService,
                trayScanService,
                errorLogService
            );

            // Wiring event handlers
            qrScanHandler.Log += uiLogger.Log;
            qrScanHandler.ShowAlert += ShowAlert;
            qrScanHandler.UpdateTraySlot += UpdateTraySlot;
            qrScanHandler.RenderTraySlots += RenderTraySlots;
            qrScanHandler.UpdateStatus += uiLogger.UpdateStatus;
            qrScanHandler.ResetSession += ResetSession;
            qrScanHandler.WriteToCOM4 += WriteToCOM4;

            // Serial port manager
            serialPortManager = new SerialPortManager();
            serialPortManager.DataReceived += OnSerialDataReceived;

            // Phục hồi session nếu có
            RestoreUnfinishedSession();

            // Khởi tạo cổng COM
            if (!serialPortManager.InitializePorts())
            {
                uiLogger.Log("❌ Cổng COM5 không tồn tại. Vui lòng kiểm tra VSPE");
                ShowAlert("E011");
            }
            else
            {
                uiLogger.Log("✅ Đã mở cổng COM5 để nhận dữ liệu QR.");
            }
        }

        // Phục hồi session chưa hoàn tất
        private void RestoreUnfinishedSession()
        {
            var (hasUnfinished, errorKey) = sessionManager.RestoreUnfinishedSession();
            if (hasUnfinished)
            {
                //nếu có session chưa hoàn tất, render các slot khay và cập nhật UI
                RenderTraySlots(sessionManager.ExpectedTrayCount);//01
                for (int i = 0; i < sessionManager.TrayQRCodes.Count; i++)
                    UpdateTraySlot(i);//cập nhật UI//02
                //cập nhật log
                uiLogger.Log($"⚠️ Phục hồi session chưa hoàn tất - ID = {sessionManager.CurrentSessionID}, đã quét {sessionManager.TrayQRCodes.Count}/{sessionManager.ExpectedTrayCount} khay");

                //cập nhật status
                txtStatus.Text = sessionManager.TrayQRCodes.Count == sessionManager.ExpectedTrayCount
                    ? "✅ Đã đủ khay, hãy quét hộp."
                    : $"✔️ Đã quét {sessionManager.TrayQRCodes.Count}/{sessionManager.ExpectedTrayCount} khay";
                //check errorKey để hiển thị thông báo lỗi nếu có
                if (!string.IsNullOrEmpty(errorKey))
                {
                    ShowAlert(errorKey);
                    uiLogger.Log($"⚠️ Cảnh báo lỗi từ session: {errorKey} - {ErrorMessageProvider.GetMessage(errorKey)}");
                    txtStatus.Text = ErrorMessageProvider.GetMessage(errorKey);
                }
            }
            else
            {
                uiLogger.Log("✅ Không có session chưa hoàn tất.");
            }
        }

        // Nhận dữ liệu từ COM5
        private void OnSerialDataReceived(string raw)
        {
            Dispatcher.Invoke(() =>
            {
                uiLogger.Log($"Đã nhận từ COM5: {raw}");
                var service = new ScannerHandleServices();
                if (service.IsTray(raw))
                {
                    qrScanHandler.HandleTrayScan(raw);
                }
                else if (service.IsBox(raw))
                {
                    qrScanHandler.HandleBoxScan(raw);
                }
            });
        }

        // Render các slot khay lên UI
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

        // Cập nhật thông tin slot khay
        private void UpdateTraySlot(int index)
        {
            if (index >= 0 && index < panelTrays.Children.Count)
            {
                var border = (Border)panelTrays.Children[index];
                border.Background = Brushes.LightGreen;

                var trayInfo = sessionManager.TrayQRCodes[index];

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

                    var rowBorder = new Border
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Child = rowGrid,
                        Margin = new Thickness(0, 0, 0, 2)
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

        // Reset session và UI
        private void ResetSession()
        {
            sessionManager.ResetSession();
            RenderTraySlots(0);
        }

        // Gửi dữ liệu tới COM4
        private void WriteToCOM4(string data)
        {
            serialPortManager.WriteToCOM4(data);
        }

        // Hiển thị alert
        private void ShowAlert(string errorKey)
        {
            alertService.ShowAlert(errorKey);
        }

        // Xử lý nút "Quay lại"
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (sessionManager.CurrentProductCode != null || sessionManager.TrayQRCodes.Count > 0)
            {
                ShowAlert("E010");
                return;
            }
            serialPortManager.ClosePorts();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
