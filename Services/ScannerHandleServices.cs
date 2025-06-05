using DataAccess.DTO;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services
{
    public class ScannerHandleServices
    {
        public bool IsTray(string qrCode)
        {

            if (qrCode.StartsWith("TRAY-"))
            {
                return true;
            }
            return false;
        }

        public bool IsBox(string qrCode)
        {

            if (qrCode.StartsWith("DISC"))
            {
                return true;
            }
            return false;
        }

        public static QRInfo ExtractQRBoxInfo(string qrCode)
        {
            var info = new QRInfo
            {
                Type = QRType.Box,
                Raw = qrCode
            };

            try
            {
                var pnIndex = qrCode.IndexOf("VN");
                if (pnIndex >= 0)
                {
                    info.ProductCode = qrCode.Substring(pnIndex, 15).Trim(); // VD: "VN012010-70201N"
                }

                string regex = @"\b\d{7}\b";
                var matchSequence = Regex.Match(qrCode, regex);
                if (matchSequence.Success)
                {
                    info.BoxSequence = matchSequence.Value;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return info;
        }

        public static QRInfo ExtractQRTrayInfo(string qrCode)
        {
            var info = new QRInfo
            {
                Type = QRType.Tray,
                Raw = qrCode
            };
            var parts = qrCode.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            try
            {
                if (parts.Length >= 5)
                {
                    info.ProductCode = parts[0].Replace("TRAY-", "").Trim();
                    info.QuantityPerTray = parts[1];
                    info.TrayPerBox = parts[2];
                    info.QuantityPerBox = parts[3];
                    info.KanbanSequence = parts[4];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return info;
        }


    }
}
