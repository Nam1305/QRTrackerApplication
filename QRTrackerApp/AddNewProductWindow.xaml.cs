using DataAccess.Models;
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
    /// <summary>
    /// Interaction logic for AddNewProductWindow.xaml
    /// </summary>
    public partial class AddNewProductWindow : Window
    {
        ProductServices productServices;
        public AddNewProductWindow()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string productCode = txtProductCode.Text.Trim();
                string quantityPerTrayStr = txtQuantityPerTray.Text.Trim();
                string trayPerBoxStr = txtTrayPerBox.Text.Trim();
                int quantityPerTray;
                int trayPerBox;

                if (string.IsNullOrEmpty(productCode) || string.IsNullOrEmpty(quantityPerTrayStr) || string.IsNullOrEmpty(trayPerBoxStr))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(quantityPerTrayStr, out quantityPerTray) || !int.TryParse(trayPerBoxStr, out trayPerBox))
                {
                    MessageBox.Show("Số lượng phải là số nguyên hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (quantityPerTray <= 0 || trayPerBox <= 0)
                {
                    MessageBox.Show("Giá trị phải lớn hơn 0!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int quantityPerBox = quantityPerTray * trayPerBox;
                productServices = new ProductServices();
                if (productServices.IsProductCodeExist(productCode))
                {
                    MessageBox.Show("Mã sản phẩm đã tồn tại");
                    return;
                }
                Product newProduct = new Product()
                {
                    ProductCode = productCode,
                    QuantityPerTray = quantityPerTray,
                    TrayPerBox = trayPerBox,
                    QuantityPerBox = quantityPerBox
                };
                
                bool success = productServices.AddNewProduct(newProduct); 

                if (success)
                    MessageBox.Show("Thêm sản phẩm thành công!");
                else
                    MessageBox.Show("Thêm sản phẩm thất bại!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
