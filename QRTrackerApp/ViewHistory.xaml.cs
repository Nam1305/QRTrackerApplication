using DataAccess.DTO;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QRTrackerApp
{
    public partial class ViewHistory : Window
    {
        HistoryScanService historyScanService;
        private int currentPage = 1;
        private int itemsPerPage = 10;
        private int totalPages = 1;
        private List<HistoryScanDTO> allData;
        private string ckeyword = "";

        public ViewHistory()
        {
            InitializeComponent();
            historyScanService = new HistoryScanService();
            LoadAllData();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadAllData();
        }

        public void LoadAllData()
        {
            ckeyword = txtSearch.Text.Trim();

            DateOnly? fromDate = null;
            DateOnly? toDate = null;

            if (dpFromDate.SelectedDate.HasValue)
                fromDate = DateOnly.FromDateTime(dpFromDate.SelectedDate.Value);

            if (dpToDate.SelectedDate.HasValue)
                toDate = DateOnly.FromDateTime(dpToDate.SelectedDate.Value);

            // Kiểm tra ngày
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                CustomAlert.Show("❗ Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
                return;
            }

            TimeSpan? fromTime = null;
            TimeSpan? toTime = null;

            if (!string.IsNullOrWhiteSpace(txtFromTime.Text))
            {
                if (!TimeSpan.TryParse(txtFromTime.Text, out TimeSpan fromParsed))
                {
                    CustomAlert.Show("❗ Định dạng giờ bắt đầu không hợp lệ. Vui lòng nhập đúng định dạng Giờ:Phút:Giây.");
                    return;
                }
                else if (fromParsed.TotalHours < 0 || fromParsed.TotalHours >= 24)
                {
                    CustomAlert.Show("❗ Giờ bắt đầu không hợp lệ. Giờ phải nằm trong khoảng từ 00:00:00 đến 23:59:59.");
                    return;
                }
                fromTime = fromParsed;
            }

            if (!string.IsNullOrWhiteSpace(txtToTime.Text))
            {
                if (!TimeSpan.TryParse(txtToTime.Text, out TimeSpan toParsed))
                {
                    CustomAlert.Show("❗ Định dạng giờ kết thúc không hợp lệ. Vui lòng nhập đúng định dạng Giờ:Phút:Giây.");
                    return;
                }
                else if (toParsed.TotalHours < 0 || toParsed.TotalHours >= 24)
                {
                    CustomAlert.Show("❗ Giờ kết thúc không hợp lệ. Giờ phải nằm trong khoảng từ 00:00:00 đến 23:59:59.");
                    return;
                }
                toTime = toParsed;
            }

            allData = historyScanService.GetHistoryScan(
                keyword: ckeyword,
                fromDate: fromDate,
                toDate: toDate,
                fromTime: fromTime,
                toTime: toTime,
                page: 1,
                pageSize: 10000);

            totalPages = (int)Math.Ceiling((double)allData.Count / itemsPerPage);
            if (totalPages == 0) totalPages = 1;

            currentPage = 1;
            LoadPage(currentPage);
        }

        private void LoadPage(int pageNumber)
        {
            var pagedData = allData
                .Skip((pageNumber - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            dgHistoryScan.ItemsSource = pagedData;
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

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadAllData();
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            dpFromDate.SelectedDate = null;
            dpToDate.SelectedDate = null;
            txtFromTime.Text = "";
            txtToTime.Text = "";
            LoadAllData();
        }
    }
}
