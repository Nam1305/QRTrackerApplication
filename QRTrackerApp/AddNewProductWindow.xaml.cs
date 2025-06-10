using DataAccess.Models;
using Services;
using System.Windows;


namespace QRTrackerApp
{
    /// <summary>
    /// Interaction logic for AddNewProductWindow.xaml
    /// </summary>
    public partial class AddNewProductWindow : Window
    {
        ProductServices productServices;
        GeneratedTrayServices generatedTrayServices;
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
                string kanbanNumberStr = txtKanbanNumber.Text.Trim();
                int quantityPerTray;
                int trayPerBox;
                int kanbanNumber;

                //Lỗi: điền thiếu thông tin
                if (string.IsNullOrEmpty(productCode) || string.IsNullOrEmpty(quantityPerTrayStr) || string.IsNullOrEmpty(trayPerBoxStr) || string.IsNullOrEmpty(kanbanNumberStr))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                //Lỗi nhập sai định dạng số
                if (!int.TryParse(quantityPerTrayStr, out quantityPerTray) || !int.TryParse(trayPerBoxStr, out trayPerBox) || !int.TryParse(kanbanNumberStr, out kanbanNumber))
                {
                    MessageBox.Show("Số lượng phải là số nguyên hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                //Lỗi nhập số âm hoặc bằng 0
                if (quantityPerTray <= 0 || trayPerBox <= 0 || kanbanNumber <= 0)
                {
                    MessageBox.Show("Giá trị phải lớn hơn 0!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int quantityPerBox = quantityPerTray * trayPerBox;
                productServices = new ProductServices();

                // Kiểm tra mã sản phẩm đã tồn tại hay chưa
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
                {
                    MessageBox.Show("Thêm sản phẩm thành công!");

                    // Lấy lại sản phẩm vừa thêm từ DB
                    Product? insertedProduct = productServices.GetProductByCode(productCode);
                    if (insertedProduct == null)
                    {
                        MessageBox.Show("Không tìm thấy sản phẩm sau khi thêm!");
                        return;
                    }

                    // Tạo danh sách GeneratedTray
                    List<GeneratedTray> trays = new List<GeneratedTray>();
                    for (int i = 1; i <= kanbanNumber; i++)
                    {
                        string sequence = i.ToString("D3");
                        string qrContent = $"TRAY-{productCode} {quantityPerTray} {trayPerBox} {quantityPerBox} {sequence}";

                        GeneratedTray tray = new GeneratedTray()
                        {
                            QrcodeContent = qrContent,
                            ProductId = insertedProduct.ProductId
                        };
                        trays.Add(tray);
                    }

                    generatedTrayServices = new GeneratedTrayServices();
                    //Sau khi thêm sản phẩm thành công, gọi hàm để Lưu QR content vào bảng GeneratedTray
                    generatedTrayServices.AddGeneratedTray(trays);

                    //Sau khi lưu xong, gọi hàm QRGenerator để tạo QR code
                    // Tạo QR Code PDF
                    QRGenerator qrGen = new QRGenerator();
                    bool qrSuccess = qrGen.GenerateQRCode(productCode, quantityPerTrayStr, trayPerBoxStr, quantityPerBox.ToString(), kanbanNumberStr);

                    if (qrSuccess)
                    {
                        MessageBox.Show("Tạo QR Code thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Tạo QR Code thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }

                else
                {
                    MessageBox.Show("Thêm sản phẩm thất bại!");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}


// hiện tại dưới đây là database của tao, hiện tại tao đang muốn xây dựng tính năng chỉnh sửa kanbansequence cho mỗi mã sản phẩm, ví dụ ban đầu mã A001 có kanbansequnce là 12-thì sẽ có 12 generatedTray tương ứng, sau đó người dùng update xuống thành 8 thì tao muốn generatedTray sẽ cập nhật xuống thành 8 cái được phép sử dụng

//thay vì xóa,ta có thể thêm trường status vào bảng GeneratedTray, nếu thừa thì sẽ set status = disable, nếu thiếu thì sẽ thêm mới vào, và khi in ra thì chỉ in những tray có status = available, và khi quét cũng chỉ chấp nhận những khay có status = available

//kanbansequence đã quét thì có thể chuyển status thành disable được không?

//đã quét không chuyển sang disable, mà chỉ chuyển sang disable khi người dùng chỉnh sửa kanbansequence xuống thấp hơn số lượng hiện tại

