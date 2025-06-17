using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class WorkSessionRepo
    {
        private readonly NewDbContext context;
        private readonly ProductRepo productRepo;

        public WorkSessionRepo()
        {
            context = new NewDbContext();
            productRepo = new ProductRepo();
        }

        public int CreateSession(string productCode)
        {
            Product? product = productRepo.GetProductByCode(productCode);
            if (product == null)
            {
                throw new ArgumentException("Product not found");
            }
            WorkSession newSession = new WorkSession
            {
                ProductId = product.ProductId
            };

            context.WorkSessions.Add(newSession);
            context.SaveChanges();
            return newSession.SessionId;
        }

        public bool UpdateSessionInfo(int sessionId, WorkSession updated)
        {
            WorkSession? existingSession = context.WorkSessions.FirstOrDefault(s => s.SessionId == sessionId);
            if (existingSession == null)
            {
                return false; // Session not found
            }
            existingSession.ScanTime = updated.ScanTime;
            existingSession.ScanDate = updated.ScanDate;
            existingSession.BoxSequence = updated.BoxSequence;
            context.SaveChanges();
            return true;
        }

        public bool IsBoxScanned(string boxSequence)
        {
            return context.WorkSessions.Any(s => s.BoxSequence == boxSequence);
        }


    }
}
