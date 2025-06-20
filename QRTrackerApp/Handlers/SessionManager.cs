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

        public (bool hasUnfinished, string errorKey) RestoreUnfinishedSession()
        {
            var unfinishedSession = workSessionService.GetUnfinishedSession();
            if (unfinishedSession != null)
            {
                CurrentSessionID = unfinishedSession.SessionId;
                CurrentProductCode = productServices.GetProductCodeById(unfinishedSession.ProductId);
                ExpectedTrayCount = unfinishedSession.ExpectedTrayCount ?? 0;

                var scannedTrays = trayScanService.GetTrayScansBySessionId(CurrentSessionID);
                TrayQRCodes.Clear();
                foreach (var tray in scannedTrays)
                {
                    var trayInfo = ScannerHandleServices.ExtractQRTrayInfo(tray.TrayQrcode);
                    TrayQRCodes.Add(trayInfo);
                }
                return (true, unfinishedSession.ErrorKey);
            }
            return (false, null);
        }

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
