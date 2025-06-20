using System;
using System.Linq;
using DataAccess.DTO;
using DataAccess.Models;
using Services;

namespace QRTrackerApp.Handlers
{
    public class QRScanHandler
    {
        private readonly SessionManager sessionManager;
        private readonly ProductServices productServices;
        private readonly WorkSessionServices workSessionService;
        private readonly GeneratedTrayServices generatedTrayService;
        private readonly TrayScanServices trayScanService;
        private readonly ErrorLogServices errorLogService;

        public event Action<string> Log;
        public event Action<string> ShowAlert;
        public event Action<int> UpdateTraySlot;
        public event Action<int> RenderTraySlots;
        public event Action<string> UpdateStatus;
        public event Action ResetSession;
        public event Action<string> WriteToCOM4;

        public QRScanHandler(
            SessionManager sessionManager,
            ProductServices productServices,
            WorkSessionServices workSessionService,
            GeneratedTrayServices generatedTrayService,
            TrayScanServices trayScanService,
            ErrorLogServices errorLogService)
        {
            this.sessionManager = sessionManager;
            this.productServices = productServices;
            this.workSessionService = workSessionService;
            this.generatedTrayService = generatedTrayService;
            this.trayScanService = trayScanService;
            this.errorLogService = errorLogService;
        }

        public void HandleTrayScan(string raw)
        {
            var trayInfo = ScannerHandleServices.ExtractQRTrayInfo(raw);

            if (!productServices.IsProductCodeExist(trayInfo.ProductCode))
            {
                Log?.Invoke("? Mã QR không hợp lệ, không tồn tại trong MASTER");
                ShowAlert?.Invoke("E001");
                errorLogService.LogError("E001", ErrorMessageProvider.GetMessage("E001"), sessionManager.CurrentSessionID > 0 ? sessionManager.CurrentSessionID : null);
                workSessionService.UpdateStatusWithError(sessionManager.CurrentSessionID, "E001");
                return;
            }

            if (sessionManager.CurrentProductCode == null)
            {
                sessionManager.StartNewSession(trayInfo.ProductCode, int.TryParse(trayInfo.TrayPerBox, out int count) ? count : 0);
                RenderTraySlots?.Invoke(sessionManager.ExpectedTrayCount);
                Log?.Invoke("Đã tạo Session mới có ID = " + sessionManager.CurrentSessionID);
            }

            if (trayInfo.ProductCode != sessionManager.CurrentProductCode)
            {
                string msg = ErrorMessageProvider.GetMessage("E002");
                UpdateStatus?.Invoke(msg);
                ShowAlert?.Invoke("E002");
                errorLogService.LogError("E002", msg, sessionManager.CurrentSessionID);
                workSessionService.UpdateStatusWithError(sessionManager.CurrentSessionID, "E002");
                return;
            }

            if (sessionManager.TrayQRCodes.Any(x => x.KanbanSequence == trayInfo.KanbanSequence))
            {
                string msg = ErrorMessageProvider.GetMessage("E003");
                Log?.Invoke($"? Mã Kanban {trayInfo.KanbanSequence} đã được quét trong phiên này!");
                ShowAlert?.Invoke("E003");
                errorLogService.LogError("E003", msg, sessionManager.CurrentSessionID);
                workSessionService.UpdateStatusWithError(sessionManager.CurrentSessionID, "E003");
                return;
            }

            if (sessionManager.TrayQRCodes.Count >= sessionManager.ExpectedTrayCount)
            {
                UpdateStatus?.Invoke("Đã quét đủ khay, hãy quét hộp.");
                return;
            }

            sessionManager.TrayQRCodes.Add(trayInfo);
            UpdateTraySlot?.Invoke(sessionManager.TrayQRCodes.Count - 1);

            int? generatedTrayID = generatedTrayService.GetGeneratedTrayIDByQRCodeContent(raw);
            if (generatedTrayID == null)
            {
                string msg = ErrorMessageProvider.GetMessage("E004");
                Log?.Invoke("Không tìm thấy mã QR trong bảng GeneratedTray!");
                ShowAlert?.Invoke("E004");
                errorLogService.LogError("E004", msg, sessionManager.CurrentSessionID);
                workSessionService.UpdateStatusWithError(sessionManager.CurrentSessionID, "E004");
                return;
            }

            trayScanService.SaveTrayScan(new TrayScan
            {
                TrayQrcode = raw,
                ScanDate = DateOnly.FromDateTime(DateTime.Now),
                ScanTime = DateTime.Now.TimeOfDay,
                GeneratedTrayId = generatedTrayID.Value,
                SessionId = sessionManager.CurrentSessionID
            });

            Log?.Invoke("Đã lưu QR khay vào database.");
            UpdateStatus?.Invoke(sessionManager.TrayQRCodes.Count == sessionManager.ExpectedTrayCount
                ? "Đã đủ khay, hãy quét hộp."
                : $"Đã quét {sessionManager.TrayQRCodes.Count}/{sessionManager.ExpectedTrayCount} khay");
        }

        public void HandleBoxScan(string raw)
        {
            if (sessionManager.TrayQRCodes.Count == 0)
            {
                UpdateStatus?.Invoke("Chưa quét khay nào. Hãy quét khay trước khi quét hộp!");
                Log?.Invoke("Quét hộp có session khay nào!");
                ShowAlert?.Invoke("E005");
                errorLogService.LogError("E005", ErrorMessageProvider.GetMessage("E005"), null);
                workSessionService.UpdateStatusWithError(null, "E005");
                return;
            }

            if (sessionManager.TrayQRCodes.Count < sessionManager.ExpectedTrayCount)
            {
                UpdateStatus?.Invoke("CHƯA ĐỦ KHAY-HÃY QUÉT ĐỦ KHAY TRƯỚC KHI QUÉT HỘP!");
                ShowAlert?.Invoke("E006");
                errorLogService.LogError("E006", ErrorMessageProvider.GetMessage("E006"), sessionManager.CurrentSessionID);
                workSessionService.UpdateStatusWithError(sessionManager.CurrentSessionID, "E006");
                return;
            }

            var boxInfo = ScannerHandleServices.ExtractQRBoxInfo(raw);

            if (!productServices.IsProductCodeExist(boxInfo.ProductCode))
            {
                Log?.Invoke("Mã QR không hợp lệ");
                ShowAlert?.Invoke("E007");
                errorLogService.LogError("E007", ErrorMessageProvider.GetMessage("E007"), sessionManager.CurrentSessionID);
                workSessionService.UpdateStatusWithError(sessionManager.CurrentSessionID, "E007");
                return;
            }

            if (boxInfo.ProductCode != sessionManager.CurrentProductCode)
            {
                UpdateStatus?.Invoke("Mã hộp không khớp với mã session khay!");
                ShowAlert?.Invoke("E009");
                errorLogService.LogError("E009", "Mã hộp không khớp với mã session khay!", sessionManager.CurrentSessionID);
                workSessionService.UpdateStatusWithError(sessionManager.CurrentSessionID, "E009");
                return;
            }

            if (workSessionService.IsBoxScanned(boxInfo.BoxSequence))
            {
                UpdateStatus?.Invoke("? Hộp đã được quét trong phiên trước đó!");
                Log?.Invoke($"Hộp {boxInfo.BoxSequence} đã quét trước đó");
                ShowAlert?.Invoke("E008");
                errorLogService.LogError("E008", ErrorMessageProvider.GetMessage("E008"), sessionManager.CurrentSessionID);
                workSessionService.UpdateStatusWithError(sessionManager.CurrentSessionID, "E008");
                return;
            }

            sessionManager.SetBoxQR(boxInfo);
            UpdateStatus?.Invoke("Đã quét hộp và gửi tới QRClient.");
            WriteToCOM4?.Invoke(raw);
            Log?.Invoke("Đã xử lý QR HỘP: " + raw);

            workSessionService.UpdateSessionInfo(sessionManager.CurrentSessionID, new WorkSession
            {
                BoxSequence = boxInfo.BoxSequence,
                ScanDate = DateOnly.FromDateTime(DateTime.Now),
                ScanTime = DateTime.Now.TimeOfDay,
                Status = 1,
                ErrorKey = null
            });

            ResetSession?.Invoke();
        }
    }
}
