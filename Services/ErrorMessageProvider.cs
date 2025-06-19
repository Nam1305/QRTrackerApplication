using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Services
{
    public class ErrorMessageProvider
    {

        private static Dictionary<string, string> _errorMessages;

        static ErrorMessageProvider()
        {
            LoadMessages();
        }
        private static void LoadMessages()
        {
            try
            {
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error-messages.json");
                string json = File.ReadAllText(jsonPath);
                _errorMessages = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            catch (Exception ex)
            {
                _errorMessages = new Dictionary<string, string>
            {
                { "UNKNOWN_ERROR", "❗Lỗi không xác định khi tải thông báo lỗi." }
            };
            }
        }

        public static string GetMessage(string key)
        {
            if (_errorMessages.ContainsKey(key))
                return _errorMessages[key];

            return $"❗Thông báo lỗi không xác định cho mã: {key}";
        }
    }
}