using DataAccess.DTO;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class HistoryScanService
    {
        HistoryScanRepo historyScanRepo;

        public HistoryScanService()
        {
            historyScanRepo = new HistoryScanRepo();
        }
        public List<HistoryScanDTO> GetHistoryScan(
            string? keyword = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            TimeSpan? fromTime = null,
            TimeSpan? toTime = null,
            int page = 1,
            int pageSize = 10)
        {
            return historyScanRepo.GetHistoryScan(keyword, fromDate, toDate, fromTime, toTime, page, pageSize);
        }
    }

}