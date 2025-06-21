using System.Collections.Generic;
using DataAccess.DTO;
using DataAccess.Models;
using Services;

namespace QRTrackerApp.Handlers
{
    public class SessionManager
    {
        public int CurrentSessionID { get; private set; }
        public string CurrentProductCode { get; private set; }
        public int ExpectedTrayCount { get; private set; }
        public List<QRInfo> TrayQRCodes { get; } = new();
        public QRInfo BoxQR { get; private set; }

        private readonly ProductServices productServices;
        private readonly WorkSessionServices workSessionService;
        private readonly TrayScanServices trayScanService;
        private readonly GeneratedTrayServices generatedTrayService;

        public SessionManager(
            ProductServices productServices,
            WorkSessionServices workSessionService,
            TrayScanServices trayScanService,
            GeneratedTrayServices generatedTrayService)
        {
            this.productServices = productServices;
            this.workSessionService = workSessionService;
            this.trayScanService = trayScanService;
            this.generatedTrayService = generatedTrayService;
        }

        //hàm RestoreUnfinishedSession trả về kiểu tuple
        public (bool hasUnfinished, string errorKey) RestoreUnfinishedSession()
        {
            var unfinishedSession = workSessionService.GetUnfinishedSession();

            //nếu có session chưa hoàn tất
            if (unfinishedSession != null)
            {
                //lấy ra sessionId của session chưa hoàn thành đó
                CurrentSessionID = unfinishedSession.SessionId;
                //lấy ra mã sản phẩm của session chưa hoàn thành
                CurrentProductCode = productServices.GetProductCodeById(unfinishedSession.ProductId);
                //lấy ra số lượng khay dự kiến của session chưa hoàn thành
                ExpectedTrayCount = unfinishedSession.ExpectedTrayCount ?? 0;

                //lấy ra những khay đã quét trong session chưa hoàn thành
                var scannedTrays = trayScanService.GetTrayScansBySessionId(CurrentSessionID);
                //clear lại danh sách TrayQRCodes để không bị lẫn lộn
                TrayQRCodes.Clear();

                //add lại các tray đã đưuọc scan vào danh sách TrayQRCodes
                foreach (var tray in scannedTrays)
                {
                    var trayInfo = ScannerHandleServices.ExtractQRTrayInfo(tray.TrayQrcode);
                    TrayQRCodes.Add(trayInfo);
                }
                return (true, unfinishedSession.ErrorKey);
            }
            return (false, null);
        }

        //hàm reset session
        public void ResetSession()
        {
            TrayQRCodes.Clear();
            BoxQR = null;
            CurrentProductCode = null;
            ExpectedTrayCount = 0;
            CurrentSessionID = 0;
        }

        public void SetBoxQR(QRInfo boxInfo)
        {
            BoxQR = boxInfo;
        }

        public void StartNewSession(string productCode, int expectedTrayCount)
        {
            CurrentProductCode = productCode;
            ExpectedTrayCount = expectedTrayCount;
            CurrentSessionID = workSessionService.CreateNewSession(productCode, expectedTrayCount);
        }
    }
}
