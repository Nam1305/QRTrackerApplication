using System;
using System.Windows.Controls;

namespace QRTrackerApp.Handlers
{
    public class UILogger
    {
        private readonly TextBox logBox;
        private readonly TextBlock statusBlock;

        public UILogger(TextBox logBox, TextBlock statusBlock)
        {
            this.logBox = logBox;
            this.statusBlock = statusBlock;
        }

        public void Log(string message)
        {
            logBox.Text += $"{DateTime.Now:HH:mm:ss} - {message}\n";
            logBox.ScrollToEnd();
        }

        public void UpdateStatus(string status)
        {
            statusBlock.Text = status;
        }
    }
}
