using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTO
{
    public enum QRType
    {
        Tray,
        Box
    }

    public class QRInfo
    {
        public QRType Type { get; set; }

        //thông tin chung của QRBox và QRTray
        //ProductCode
        public string? ProductCode { get; set; }

        //thông tin riêng cho QRTRAY--------------------------
        
        public string? QuantityPerTray { get; set; }//QuantityPerTray
        
        public string? TrayPerBox { get; set; }//TrayPerBox
        
        public string? QuantityPerBox { get; set; }//QuantityPerBox
        
        public string? KanbanSequence { get; set; }//KanbanSeq

        //---------------------------------------------------
        //thông tin riêng cho QRBOX
        
        public string? ProcessCode { get; set; }//ProcessCode
        
        public string? BoxSequence { get; set; }//BoxSeq


        // === Chuỗi gốc để debug ===
        public string Raw { get; set; } = string.Empty;


    }
}
