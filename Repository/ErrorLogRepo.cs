using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Repository
{
    public class ErrorLogRepo
    {
        private readonly QrtrackerDbv2Context context;
        public ErrorLogRepo()
        {
            context = new QrtrackerDbv2Context();
        }

        public bool SaveErrorLog(ErrorLog error)
        {
            try
            {
                if (error == null)
                {
                    throw new ArgumentNullException(nameof(error), "Error log cannot be null");
                }
                // Thiết lập thời gian lỗi nếu chưa có
                if (error.ErrorDate == null)
                    error.ErrorDate = DateTime.Now.Date;
                if (error.ErrorTime == null)
                    error.ErrorTime = DateTime.Now.TimeOfDay;
                // Thêm lỗi vào cơ sở dữ liệu
                context.ErrorLogs.Add(error);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Error saving error log: {ex.Message}");
                return false;
            }
        }
    }
}
