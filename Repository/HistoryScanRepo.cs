using DataAccess.DTO;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class HistoryScanRepo
    {
        private readonly NewDbContext context;

        public HistoryScanRepo()
        {
            context = new NewDbContext();
        }

        public List<HistoryScanDTO> GetHistoryScan(
            //đối số
            string? keyword = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            TimeSpan? fromTime = null,
            TimeSpan? toTime = null,
            int page = 1,
            int pageSize = 10)
        {
            //join các bảng
            var query = context.TrayScans
                .Include(ts => ts.Session)
                .ThenInclude(ws => ws.Product)
                .AsQueryable();

            // Filter keyword 
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(ts =>
                    ts.Session.Product.ProductCode.Contains(keyword) ||
                    ts.Session.BoxSequence.Contains(keyword) ||
                    ts.TrayQrcode.Contains(keyword));
            }

            // Filter ngày
            if (fromDate.HasValue)
                query = query.Where(ts => ts.ScanDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(ts => ts.ScanDate <= toDate.Value);

            // Lấy dữ liệu ra bộ nhớ trước (bỏ filter time, vì không dịch được)
            var list = query.ToList();

            // Filter giờ (TimeSpan) trên client
            if (fromTime.HasValue)
                list = list.Where(ts => ts.ScanTime >= fromTime.Value).ToList();
            if (toTime.HasValue)
                list = list.Where(ts => ts.ScanTime <= toTime.Value).ToList();

            // Sắp xếp mới nhất trước: theo ngày rồi theo giờ
            list = list.OrderByDescending(ts => ts.ScanDate)
                   .ThenByDescending(ts => ts.ScanTime)
                   .ToList();

            // Phân trang
            var pagedList = list.Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            // Chuyển sang DTO
            return pagedList.Select((ts, index) => new HistoryScanDTO

            {
                RowNumber = (page - 1) * pageSize + index + 1,
                ProductCode = ts.Session.Product.ProductCode,
                QuantityPerTray = ts.Session.Product.QuantityPerTray,
                TrayPerBox = ts.Session.Product.TrayPerBox,
                BoxSequence = ts.Session.BoxSequence,
                TrayKanban = ts.TrayQrcode.Length >= 3 ? ts.TrayQrcode.Substring(ts.TrayQrcode.Length - 3) : ts.TrayQrcode,
                ScanTime = ts.ScanTime,
                ScanDate = ts.ScanDate  
            }).ToList();

        }

    }

}
