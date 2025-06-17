using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;

namespace Services
{
    public class GeneratedTrayServices
    {
        GeneratedTrayRepo generatedTrayRepo;
        public GeneratedTrayServices()
        {
            generatedTrayRepo = new GeneratedTrayRepo();
        }
        public bool AddGeneratedTray(List<GeneratedTray> tray)
        {
            if (tray == null || tray.Count == 0)
            {
                throw new ArgumentException("Tray list cannot be null or empty", nameof(tray));
            }
            return generatedTrayRepo.AddGeneratedTray(tray);
        }

        public int? GetGeneratedTrayIDByQRCodeContent(string qrContent)
        {
            if (string.IsNullOrWhiteSpace(qrContent))
            {
                throw new ArgumentException("QR code content cannot be null or empty", nameof(qrContent));
            }
            return generatedTrayRepo.GetGeneratedTrayIDByQRCodeContent(qrContent.Trim());
        }
    }
}
