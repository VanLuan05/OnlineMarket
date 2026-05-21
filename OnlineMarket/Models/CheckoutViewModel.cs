using OnlineMarket;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CheckoutViewModel
{
    // Thông tin giỏ hàng
    public List<GioHang> CartItems { get; set; }
    public decimal TongTien { get; set; }
    public decimal PhiVanChuyen { get; set; }
    public decimal VAT { get; set; }
    public decimal TongThanhToan { get; set; }
    public List<DiaChiNguoiDung> DiaChiList { get; set; }

    // Thông tin khuyến mãi
    public List<KHUYENMAI> AvailableDiscounts { get; set; } // THÊM DÒNG NÀY
    public List<KHUYENMAI> AvailableVouchers { get; set; }
    public string DiscountCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public string DiscountMessage { get; set; }

    // Thông tin giao hàng
    [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
    public string HoTenNhanHang { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    public string SoDienThoai { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
    public string DiaChi { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn tỉnh/thành")]
    public string TinhThanh { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn quận/huyện")]
    public string QuanHuyen { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn phường/xã")]
    public string PhuongXa { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn hình thức thanh toán")]
    public string HinhThucThanhToan { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn phương thức giao hàng")]
    public string PhuongThucGiaoHang { get; set; }

    public string GhiChu { get; set; }
}

// Model cho hiển thị voucher
public class VoucherDisplayModel
{
    public int MaKm { get; set; }
    public string TenKm { get; set; }
    public string MoTa { get; set; }
    public int? PhanTramGiam { get; set; }
    public decimal? GiaTriGiam { get; set; }
    public string Loai { get; set; }
    public decimal? DieuKienApDung { get; set; }
    public string DisplayText { get; set; }
}