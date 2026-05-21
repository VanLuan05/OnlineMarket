using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineMarket.Models
{
    // ViewModel cho tạo khuyến mãi
    public class KhuyenMaiCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã khuyến mãi")]
        [Display(Name = "Mã khuyến mãi")]
        public string TENKM { get; set; }

        [Display(Name = "Mô tả")]
        public string MOTA { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại khuyến mãi")]
        [Display(Name = "Loại khuyến mãi")]
        public string LOAI { get; set; } // Percentage, Fixed, Freeship

        [Display(Name = "Phần trăm giảm (%)")]
        [Range(0, 100, ErrorMessage = "Phần trăm giảm phải từ 0 đến 100")]
        public int? PHANTRAMGIAM { get; set; }

        [Display(Name = "Giá trị giảm (VNĐ)")]
        [Range(0, 1000000000, ErrorMessage = "Giá trị giảm không hợp lệ")]
        public decimal? GIATRIGIAM { get; set; }

        [Display(Name = "Giá trị tối đa (VNĐ)")]
        [Range(0, 1000000000, ErrorMessage = "Giá trị tối đa không hợp lệ")]
        public decimal? GIATRITOIDA { get; set; }

        [Display(Name = "Điều kiện áp dụng (VNĐ)")]
        [Range(0, 1000000000, ErrorMessage = "Điều kiện áp dụng không hợp lệ")]
        public decimal? DIEUKIENAPDUNG { get; set; }

        [Display(Name = "Số lần sử dụng tối đa")]
        [Range(1, 100000, ErrorMessage = "Số lần sử dụng phải lớn hơn 0")]
        public int? SOLANSUDUNG { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime? NGAYBATDAU { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime? NGAYKETTHUC { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        [Display(Name = "Trạng thái")]
        public string TRANGTHAI { get; set; }
    }

    // ViewModel cho chỉnh sửa khuyến mãi
    public class KhuyenMaiEditViewModel
    {
        public int MAKM { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã khuyến mãi")]
        [Display(Name = "Mã khuyến mãi")]
        public string TENKM { get; set; }

        [Display(Name = "Mô tả")]
        public string MOTA { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại khuyến mãi")]
        [Display(Name = "Loại khuyến mãi")]
        public string LOAI { get; set; }

        [Display(Name = "Phần trăm giảm (%)")]
        [Range(0, 100, ErrorMessage = "Phần trăm giảm phải từ 0 đến 100")]
        public int? PHANTRAMGIAM { get; set; }

        [Display(Name = "Giá trị giảm (VNĐ)")]
        [Range(0, 1000000000, ErrorMessage = "Giá trị giảm không hợp lệ")]
        public decimal? GIATRIGIAM { get; set; }

        [Display(Name = "Giá trị tối đa (VNĐ)")]
        [Range(0, 1000000000, ErrorMessage = "Giá trị tối đa không hợp lệ")]
        public decimal? GIATRITOIDA { get; set; }

        [Display(Name = "Điều kiện áp dụng (VNĐ)")]
        [Range(0, 1000000000, ErrorMessage = "Điều kiện áp dụng không hợp lệ")]
        public decimal? DIEUKIENAPDUNG { get; set; }

        [Display(Name = "Số lần sử dụng tối đa")]
        [Range(1, 100000, ErrorMessage = "Số lần sử dụng phải lớn hơn 0")]
        public int? SOLANSUDUNG { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime? NGAYBATDAU { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime? NGAYKETTHUC { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        [Display(Name = "Trạng thái")]
        public string TRANGTHAI { get; set; }
    }

    // ViewModel cho thống kê
    public class KhuyenMaiStatisticsViewModel
    {
        public KHUYENMAI KhuyenMai { get; set; }
        public int TotalUsage { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<MonthlyUsage> UsageByMonth { get; set; }
        public List<DonHang> RecentOrders { get; set; }
    }

    public class MonthlyUsage
    {
        public string Month { get; set; }
        public int UsageCount { get; set; }
    }

    // ViewModel cho import nhiều khuyến mãi
    public class ImportKhuyenMaiViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập danh sách mã")]
        [Display(Name = "Danh sách mã (mỗi mã một dòng)")]
        public string MaList { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại khuyến mãi")]
        [Display(Name = "Loại khuyến mãi")]
        public string LOAI { get; set; }

        [Display(Name = "Phần trăm giảm (%)")]
        [Range(0, 100, ErrorMessage = "Phần trăm giảm phải từ 0 đến 100")]
        public int? PHANTRAMGIAM { get; set; }

        [Display(Name = "Giá trị giảm (VNĐ)")]
        [Range(0, 1000000000, ErrorMessage = "Giá trị giảm không hợp lệ")]
        public decimal? GIATRIGIAM { get; set; }

        [Display(Name = "Giá trị tối đa (VNĐ)")]
        [Range(0, 1000000000, ErrorMessage = "Giá trị tối đa không hợp lệ")]
        public decimal? GIATRITOIDA { get; set; }

        [Display(Name = "Điều kiện áp dụng (VNĐ)")]
        [Range(0, 1000000000, ErrorMessage = "Điều kiện áp dụng không hợp lệ")]
        public decimal? DIEUKIENAPDUNG { get; set; }

        [Display(Name = "Số lần sử dụng tối đa")]
        [Range(1, 100000, ErrorMessage = "Số lần sử dụng phải lớn hơn 0")]
        public int? SOLANSUDUNG { get; set; }
    }
}