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
        private readonly QrtrackerDbv2Context context;

        public TrayScanRepo()
        {
            context = new QrtrackerDbv2Context();
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

        public List<TrayScan> GetTrayScansBySessionId(int sessionId)
        {
            try
            {
                return context.TrayScans
                    .Where(ts => ts.SessionId == sessionId)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<TrayScan>();
            }
        }
    }
}
