using DataAccess.Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class WorkSessionServices
    {
        private readonly WorkSessionRepo workSessionRepo;
        public WorkSessionServices()
        {
            workSessionRepo = new WorkSessionRepo();
        }
        public int CreateNewSession(string productCode)
        {
            if (string.IsNullOrWhiteSpace(productCode))
            {
                throw new ArgumentException("Product code cannot be null or empty.");
            }
            return workSessionRepo.CreateSession(productCode.Trim());
        }

        public bool UpdateSessionInfo(int sessionId, WorkSession updatedSession)
        {
            if (updatedSession == null)
            {
                throw new ArgumentNullException(nameof(updatedSession), "Updated session cannot be null.");
            }
            return workSessionRepo.UpdateSessionInfo(sessionId, updatedSession);
        }

        public bool IsBoxScanned(string boxSequence)
        {
            if (string.IsNullOrWhiteSpace(boxSequence))
            {
                throw new ArgumentException("Box sequence cannot be null or empty.");
            }
            return workSessionRepo.IsBoxScanned(boxSequence.Trim());
        }

    }
}
