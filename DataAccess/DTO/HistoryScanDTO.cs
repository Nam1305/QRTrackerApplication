using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTO
{
    public class HistoryScanDTO
    {
        public int RowNumber { get; set; } // Số thứ tự tự tăng
        public string ProductCode { get; set; }
        public int QuantityPerTray { get; set; }
        public int TrayPerBox { get; set; }
        public string BoxSequence { get; set; }
        public string TrayKanban { get; set; }
        public TimeSpan? ScanTime { get; set; }
        public DateOnly? ScanDate { get; set; }
    }
}
