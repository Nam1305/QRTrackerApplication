using DataAccess.Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ErrorLogServices
    {
        private readonly ErrorLogRepo errorLogRepo;

        public ErrorLogServices()
        {
            errorLogRepo = new ErrorLogRepo();
        }

        public bool SaveErrorLog(ErrorLog error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error), "Error log cannot be null");
            }
            return errorLogRepo.SaveErrorLog(error);
        }

        public void LogError(string errorKey, string message, int? sessionId = null)
        {
            var log = new ErrorLog
            {
                ErrorKey = errorKey,
                ErrorMessage = message,
                ErrorDate = DateTime.Now.Date,
                ErrorTime = DateTime.Now.TimeOfDay,
                SessionId = sessionId,
                IsResolved = 0
            };

            errorLogRepo.SaveErrorLog(log);
        }
    }
}
