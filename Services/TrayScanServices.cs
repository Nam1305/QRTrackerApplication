using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;
namespace Services
{
    public class TrayScanServices
    {
        private readonly TrayScanRepo trayScanRepo;
        private readonly WorkSessionRepo workSessionRepo;   
        public TrayScanServices()
        {
            trayScanRepo = new TrayScanRepo();
        }
        public bool SaveTrayScan(TrayScan trayScan)
        {
            if (trayScan == null)
                throw new ArgumentNullException(nameof(trayScan), "Tray scan cannot be null");
            return trayScanRepo.SaveTrayScan(trayScan);
        }
        

    }
}
