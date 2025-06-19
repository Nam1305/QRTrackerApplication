using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QRTrackerApp
{
    public partial class CustomAlert : Window
    {
        private readonly ErrorMessageProvider errorMessageProvider;
        public CustomAlert(string message)
        {
            InitializeComponent();
            errorMessageProvider = new ErrorMessageProvider();
            lblMessage.Text = message;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Owner = this;
            loginWindow.ShowDialog();

            //xác thực đúng mới đóng Cảnh Báo
            if (loginWindow.IsAuthenticated)
            {
                this.Close();
            }
            
        }
        
        // Hàm static tiện lợi để gọi giống MessageBox
        public static void Show(string message)
        {
            var alert = new CustomAlert(message);
            alert.ShowDialog();
        }

        private void ShowAlert(string errorKey, string title = "Thông báo")
        {
            string message = ErrorMessageProvider.GetMessage(errorKey);
            CustomAlert.Show(message);
        }


    }
}
