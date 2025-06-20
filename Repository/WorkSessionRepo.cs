using DataAccess.Models;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class WorkSessionRepo
    {
        private readonly QrtrackerDbv2Context context;
        private readonly ProductRepo productRepo;

        public WorkSessionRepo()
        {
            context = new QrtrackerDbv2Context();
            productRepo = new ProductRepo();
        }

        public int CreateSession(string productCode, int expectedTrayCount)
        {
            Product? product = productRepo.GetProductByCode(productCode);
            if (product == null)
            {
                throw new ArgumentException("Product not found");
            }
            WorkSession newSession = new WorkSession
            {
                ProductId = product.ProductId,
                ExpectedTrayCount = expectedTrayCount,
                Status = 2, // processing
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
            existingSession.Status = updated.Status;
            existingSession.ErrorKey = updated.ErrorKey;
            context.WorkSessions.Update(existingSession);
            context.SaveChanges();
            return true;
        }

        public bool IsBoxScanned(string boxSequence)
        {
            return context.WorkSessions.Any(s => s.BoxSequence == boxSequence);
        }

        public WorkSession? GetUnfinishedSession()
        {
            // Status: 2 = processing, 4 = error
            return context.WorkSessions
                .Where(s => s.Status == 2 || s.Status == 4)
                .OrderByDescending(s => s.SessionId)
                .FirstOrDefault();
        }

        public void UpdateStatusWithError(int? sessionId, string errorKey)
        {
            if (sessionId == null) return;

            var session = context.WorkSessions.FirstOrDefault(s => s.SessionId == sessionId);
            if (session != null)
            {
                session.Status = 4; // lỗi
                session.ErrorKey = errorKey;
                context.SaveChanges();
            }
        }


    }
}

//