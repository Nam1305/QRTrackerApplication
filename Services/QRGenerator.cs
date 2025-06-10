using QRCoder;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Services
{
    public class QRGenerator
    {

        public bool GenerateQRCode(string productCode, string quantityPerTray, string trayPerBox, string quantityPerBox, string kanbanNumber)
        {
            try
            {
                if (!int.TryParse(kanbanNumber, out int totalKanban) || totalKanban <= 0)
                {
                    Console.WriteLine("Kanban Number phải là số nguyên dương.");
                    return false;
                }

                PdfDocument document = new PdfDocument();
                PdfPage page = document.AddPage();
                page.Width = XUnit.FromMillimeter(210);
                page.Height = XUnit.FromMillimeter(297);
                XGraphics gfx = XGraphics.FromPdfPage(page);

                double marginLeft = 30;
                double marginTop = 30;
                double qrSize = 120;
                double padding = 20;
                double pageWidth = page.Width;
                double pageHeight = page.Height;
                double spacePerQR = qrSize + padding + 20;
                int qrPerRow = 3;
                double columnWidth = (pageWidth - 2 * marginLeft) / qrPerRow;

                int col = 0;
                double currentX = marginLeft;
                double currentY = marginTop;

                for (int i = 1; i <= totalKanban; i++)
                {
                    if (currentY + qrSize + 30 > pageHeight)
                    {
                        page = document.AddPage();
                        page.Width = XUnit.FromMillimeter(210);
                        page.Height = XUnit.FromMillimeter(297);
                        gfx = XGraphics.FromPdfPage(page);
                        currentX = marginLeft;
                        currentY = marginTop;
                        col = 0;
                    }

                    string sequence = i.ToString("D3");
                    string content = $"TRAY-{productCode} {quantityPerTray} {trayPerBox} {quantityPerBox} {sequence}";

                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                    BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
                    byte[] qrImageBytes = qrCode.GetGraphic(10);

                    using (MemoryStream ms = new MemoryStream(qrImageBytes))
                    using (Bitmap bmp = new Bitmap(ms))
                    {
                        XImage xImg;
                        using (MemoryStream imageStream = new MemoryStream())
                        {
                            bmp.Save(imageStream, ImageFormat.Png);
                            imageStream.Position = 0;
                            xImg = XImage.FromStream(() => new MemoryStream(imageStream.ToArray()));
                        }

                        gfx.DrawImage(xImg, currentX, currentY, qrSize, qrSize);
                        gfx.DrawString(content, new XFont("Arial", 10), XBrushes.Black,
                            new XRect(currentX, currentY + qrSize + 5, qrSize, 20),
                            XStringFormats.Center);
                    }

                    col++;
                    if (col >= qrPerRow)
                    {
                        col = 0;
                        currentX = marginLeft;
                        currentY += spacePerQR;
                    }
                    else
                    {
                        currentX += columnWidth;
                    }
                }

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string outputPath = Path.Combine(desktopPath, $"Kanban_QR_{DateTime.Now:yyyyMMdd_HHmmss}_{productCode}.pdf");
                document.Save(outputPath);

                Console.WriteLine($"✅ QR Code PDF đã được lưu tại: {outputPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi sinh QRCode PDF: {ex.Message}");
                return false;
            }
        }


    }
}
