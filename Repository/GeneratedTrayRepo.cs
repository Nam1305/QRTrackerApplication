using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class GeneratedTrayRepo
    {
        private readonly NewDbContext context;
        public GeneratedTrayRepo()
        {
            context = new NewDbContext();
        }

        public bool AddGeneratedTray(List<GeneratedTray> trays)
        {
            try
            {
                context.GeneratedTrays.AddRange(trays);
                return context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error adding generated trays: {ex.Message}");
                return false;
            }
        }

        //hàm lấy GeneratedTrayId từ mã QR

        public int? GetGeneratedTrayIDByQRCodeContent(string qrContent)
        {
            var tray = context.GeneratedTrays.FirstOrDefault(t => t.QrcodeContent == qrContent);
            return tray?.GeneratedTrayId;
        }

    }
}
