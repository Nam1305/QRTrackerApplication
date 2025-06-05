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
        public CustomAlert(string message)
        {
            InitializeComponent();
            lblMessage.Text = message;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Hàm static tiện lợi để gọi giống MessageBox
        public static void Show(string message)
        {
            var alert = new CustomAlert(message);
            alert.ShowDialog();
        }
    }
}
