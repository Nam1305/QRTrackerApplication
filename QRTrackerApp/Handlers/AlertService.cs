using Services;

namespace QRTrackerApp.Handlers
{
    public class AlertService
    {
        public void ShowAlert(string errorKey)
        {
            string message = ErrorMessageProvider.GetMessage(errorKey);
            CustomAlert alert = new CustomAlert(message);
            alert.ShowDialog();
        }
    }
}
