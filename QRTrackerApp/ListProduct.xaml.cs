using DataAccess.Models;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QRTrackerApp
{
    /// <summary>
    /// Interaction logic for ListProduct.xaml
    /// </summary>
    public partial class ListProduct : Window
    {
        ProductServices productServices;
        private List<Product> AllProducts = new List<Product>();
        private int currentPage = 1;
        private int itemsPerPage = 10;
        private int totalPages = 1;
        private string currentKeyword = "";

        public ListProduct()
        {
            InitializeComponent();
            productServices = new ProductServices();
            LoadPage(currentPage);
        }

        private void dgListProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgListProduct.SelectedItem is Product product)
            {
                txtProductCode.Text = product.ProductCode;
                txtQuantityPerTray.Text = product.QuantityPerTray.ToString();
                txtQuantityPerBox.Text = product.QuantityPerBox.ToString();
                txtTrayPerBox.Text = product.TrayPerBox.ToString();
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentKeyword = txtSearch.Text.Trim();
            currentPage = 1;
            LoadPage(currentPage);
        }

        private void LoadPage(int page)
        {
            var pagedProducts = productServices.LoadPagedProducts(page, itemsPerPage, currentKeyword);
            int totalItems = productServices.GetTotalProductCount(currentKeyword);
            totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            this.dgListProduct.ItemsSource = pagedProducts;
            txtPageInfo.Text = $"Trang {currentPage} / {totalPages}";
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadPage(currentPage);
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadPage(currentPage);
            }
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 1;
            currentKeyword = string.Empty;
            txtSearch.Text = string.Empty;
            LoadPage(currentPage);
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Owner = this;
            loginWindow.ShowDialog();

            if (loginWindow.IsAuthenticated)
            {
                AddNewProductWindow addProductWindow = new AddNewProductWindow();
                addProductWindow.ShowDialog();
            }
        }

        private void btnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dgListProduct.SelectedItem == null)
            {
                ShowAlert("⚠ Vui lòng chọn một sản phẩm trước khi chỉnh sửa!");
                return;
            }
                //ShowAlert("❌ Lỗi khi lấy thông tin sản phẩm!");
                //return;

            var selectedProduct = dgListProduct.SelectedItem as Product;
            if (selectedProduct == null)
            {
            }

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Owner = this;
            loginWindow.ShowDialog();

            if (loginWindow.IsAuthenticated)
            {
                EditProductWindow editProductWindow = new EditProductWindow();
                editProductWindow.SetProductToEdit(selectedProduct);
                editProductWindow.Owner = this;
                editProductWindow.ShowDialog();
            }
        }

        private void ShowAlert(string message)
        {
            CustomAlert alert = new CustomAlert(message);
            alert.Owner = this;
            alert.ShowDialog();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
