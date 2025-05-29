using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataAccess.Models;
using Services;
namespace QRTrackerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
           
        }

        private void btn_ListProduct_Click(object sender, RoutedEventArgs e)
        {
            Window member = new ListProduct();
            member.WindowState = WindowState.Maximized;
            member.Show();
        }

        private void btn_ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            Window member = new ViewHistory();
            member.WindowState = WindowState.Maximized;
            member.Show();
        }

    }
}