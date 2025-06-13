using System;
using Services;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            QRGenerator qrGenerator = new QRGenerator();
            string productCode = "VN197400-53804T";
            string quantityPerTray = "30";
            string trayPerBox = "4";
            string quantityPerBox = "120";
            string kanbanNumber = "12";
            qrGenerator.GenerateQRCode(productCode, quantityPerTray, trayPerBox, quantityPerBox, kanbanNumber);
            Console.WriteLine("QR Code generated successfully.");
        }
    }
}