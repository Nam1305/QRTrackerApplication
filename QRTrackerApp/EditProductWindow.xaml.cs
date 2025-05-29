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
    /// Interaction logic for EditProductWindow.xaml
    /// </summary>
    public partial class EditProductWindow : Window
    {
        private Product productToEdit;
        ProductServices productServices;
        public EditProductWindow()
        {
            InitializeComponent();
        }

        public void SetProductToEdit(Product product)
        {
            productToEdit = product;

            // Đổ dữ liệu lên giao diện
            txtProductCode.Text = product.ProductCode;
            txtQuantityPerTray.Text = product.QuantityPerTray.ToString();
            txtTrayPerBox.Text = product.TrayPerBox.ToString();
            txtQuantityPerBox.Text = product.QuantityPerBox.ToString();
        }

        private void OnTrayOrBoxChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(txtQuantityPerTray.Text, out int qtyPerTray) &&
                int.TryParse(txtTrayPerBox.Text, out int trayPerBox))
            {
                int qtyPerBox = qtyPerTray * trayPerBox;
                txtQuantityPerBox.Text = qtyPerBox.ToString();
            }
            else
            {
                txtQuantityPerBox.Text = "—";
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (productToEdit == null)
            {
                MessageBox.Show("Không tìm thấy sản phẩm để cập nhật!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(txtQuantityPerTray.Text, out int qtyPerTray) ||
        !int.TryParse(txtTrayPerBox.Text, out int trayPerBox))
            {
                MessageBox.Show("Vui lòng nhập đúng định dạng số cho số lượng!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int qtyPerBox = qtyPerTray * trayPerBox;

            // Cập nhật dữ liệu vào productToEdit
            productToEdit.QuantityPerTray = qtyPerTray;
            productToEdit.TrayPerBox = trayPerBox;
            productToEdit.QuantityPerBox = qtyPerBox;

            productServices = new ProductServices();
            bool updated = productServices.UpdateProduct(productToEdit);
            if (updated) 
            {
                MessageBox.Show("✅ Cập nhật sản phẩm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true; // Để caller biết là đã cập nhật xong
                this.Close();
            }
            else
            {
                MessageBox.Show("❌ Cập nhật không thành công. Vui lòng thử lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
