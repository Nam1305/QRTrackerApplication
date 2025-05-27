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
        BoxesServices boxesServices;

        public MainWindow()
        {
            InitializeComponent();
            LoadDataGridBoxes();
        }

        public void LoadDataGridBoxes()
        {
            boxesServices = new BoxesServices();    
            var boxes = boxesServices.GetAllBoxes();
            this.dgDataBox.ItemsSource = boxes; 
        }
    }
}