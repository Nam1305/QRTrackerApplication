using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class TrayScanRepo
    {
        private readonly NewDbContext context;

        public TrayScanRepo()
        {
            context = new NewDbContext();
        }

        public bool SaveTrayScan(TrayScan trayScan)
        {
            try
            {
                context.TrayScans.Add(trayScan);
                return context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
