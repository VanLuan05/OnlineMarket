using OnlineMarket;
using OnlineMarket.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
public class CartController : Controller
{
    private QL_OnlineMarketEntities db = new QL_OnlineMarketEntities();

    // GET: /Cart/
    public ActionResult Index()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            ViewBag.Error = "Vui lòng đăng nhập để xem giỏ hàng";
            return View(new List<GioHang>());
        }

        var cartItems = db.GioHangs
            .Where(gh => gh.MAND == user.MAND)
            .Include(gh => gh.SanPham)
            .ToList();

        // Tính tổng tiền
        decimal tongTien = cartItems.Sum(item => (item.SanPham.GIA * item.SOLUONG) ?? 0);
        decimal phiVanChuyen = tongTien >= 500000 || tongTien == 0 ? 0 : 20000;
        decimal vat = tongTien * 0.08m;
        decimal tongThanhToan = tongTien + phiVanChuyen + vat;

        ViewBag.TongTien = tongTien;
        ViewBag.PhiVanChuyen = phiVanChuyen;
        ViewBag.VAT = vat;
        ViewBag.TongThanhToan = tongThanhToan;

        return View(cartItems);
    }

    // POST: /Cart/AddToCart
    [HttpPost]
    public ActionResult AddToCart(int productId, int quantity = 1)
    {
        try
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            var product = db.SanPhams.Find(productId);
            if (product == null || product.TRANGTHAI != "Còn hàng")
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại hoặc hết hàng!" });
            }

            // Kiểm tra số lượng tồn
            if (quantity > product.SOLUONGTON)
            {
                return Json(new { success = false, message = "Số lượng vượt quá tồn kho!" });
            }

            // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
            var existingCartItem = db.GioHangs
                .FirstOrDefault(gh => gh.MAND == user.MAND && gh.MASP == productId);

            if (existingCartItem != null)
            {
                // Cập nhật số lượng
                var newQuantity = existingCartItem.SOLUONG + quantity;

                if (newQuantity > product.SOLUONGTON)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Chỉ có thể thêm tối đa {product.SOLUONGTON - existingCartItem.SOLUONG} sản phẩm!"
                    });
                }

                existingCartItem.SOLUONG = newQuantity;
            }
            else
            {
                // Thêm mới vào giỏ hàng
                var cartItem = new GioHang
                {
                    MAND = user.MAND,
                    MASP = productId,
                    SOLUONG = quantity,
                    NGAYTHEM = DateTime.Now
                };
                db.GioHangs.Add(cartItem);
            }

            db.SaveChanges();

            // Đếm tổng số sản phẩm trong giỏ
            var cartCount = db.GioHangs
                .Where(gh => gh.MAND == user.MAND)
                .Sum(gh => gh.SOLUONG) ?? 0;

            return Json(new
            {
                success = true,
                message = "Đã thêm vào giỏ hàng!",
                cartCount = cartCount
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
        }
    }

    // POST: /Cart/UpdateQuantity
    [HttpPost]
    public ActionResult UpdateQuantity(int cartId, int quantity)
    {
        try
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            var cartItem = db.GioHangs
                .Include(gh => gh.SanPham)
                .FirstOrDefault(gh => gh.MAGH == cartId && gh.MAND == user.MAND);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng!" });
            }

            // Kiểm tra số lượng hợp lệ
            if (quantity < 1)
            {
                return Json(new { success = false, message = "Số lượng phải lớn hơn 0!" });
            }

            // Kiểm tra số lượng tồn kho
            if (quantity > cartItem.SanPham.SOLUONGTON)
            {
                return Json(new
                {
                    success = false,
                    message = $"Chỉ còn {cartItem.SanPham.SOLUONGTON} sản phẩm trong kho!"
                });
            }

            cartItem.SOLUONG = quantity;
            db.SaveChanges();

            // Tính toán lại và trả về kết quả đầy đủ
            var summary = CalculateCartSummary(user.MAND);
            return Json(new
            {
                success = true,
                message = "Đã cập nhật số lượng!",
                cartCount = summary.cartCount,
                tongTien = summary.tongTien,
                phiVanChuyen = summary.phiVanChuyen,
                vat = summary.vat,
                tongThanhToan = summary.tongThanhToan
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
        }
    }
    // POST: /Cart/RemoveFromCart
    [HttpPost]
    public ActionResult RemoveFromCart(int cartId)
    {
        try
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            var cartItem = db.GioHangs
                .FirstOrDefault(gh => gh.MAGH == cartId && gh.MAND == user.MAND);

            if (cartItem != null)
            {
                db.GioHangs.Remove(cartItem);
                db.SaveChanges();
            }

            // Tính toán lại và trả về kết quả đầy đủ
            var summary = CalculateCartSummary(user.MAND);
            return Json(new
            {
                success = true,
                message = "Đã xóa sản phẩm khỏi giỏ hàng!",
                cartCount = summary.cartCount,
                tongTien = summary.tongTien,
                phiVanChuyen = summary.phiVanChuyen,
                vat = summary.vat,
                tongThanhToan = summary.tongThanhToan
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
        }
    }

    // POST: /Cart/ClearCart
    [HttpPost]
    public ActionResult ClearCart()
    {
        try
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            var cartItems = db.GioHangs
                .Where(gh => gh.MAND == user.MAND)
                .ToList();

            foreach (var item in cartItems)
            {
                db.GioHangs.Remove(item);
            }

            db.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Đã xóa toàn bộ giỏ hàng!",
                cartCount = 0,
                tongTien = 0,
                phiVanChuyen = 0,
                vat = 0,
                tongThanhToan = 0
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
        }
    }

    // ========== CÁC PHƯƠNG THỨC HỖ TRỢ ==========

    private NguoiDung GetCurrentUser()
    {
        // Ưu tiên lấy từ Session trước
        if (Session["UserID"] != null)
        {// Sử dụng Convert.ToInt32 để chuyển đổi an toàn từ String sang Int
            int userId = Convert.ToInt32(Session["UserID"]);

            // Thêm kiểm tra null để tránh lỗi nếu admin ảo (ID=0) không có trong database
            var user = db.NguoiDungs.Find(userId);
            if (user == null)
            {
                // Xử lý trường hợp Admin test (ID=0) không tồn tại trong DB
                return null;
            }
            return user;
        }

        // Nếu không có Session, lấy từ Authentication
        if (User.Identity.IsAuthenticated)
        {
            var userName = User.Identity.Name;
            return db.NguoiDungs.FirstOrDefault(u => u.TENDANGNHAP == userName);
        }

        return null;
    }

    private dynamic CalculateCartSummary(int userId)
    {
        var cartItems = db.GioHangs
            .Where(gh => gh.MAND == userId)
            .Include(gh => gh.SanPham)
            .ToList();

        decimal tongTien = cartItems.Sum(item => (item.SanPham.GIA * item.SOLUONG) ?? 0);
        decimal phiVanChuyen = tongTien >= 500000 || tongTien == 0 ? 0 : 20000;
        decimal vat = tongTien * 0.08m;
        decimal tongThanhToan = tongTien + phiVanChuyen + vat;
        var cartCount = cartItems.Sum(gh => gh.SOLUONG) ?? 0;

        return new
        {
            cartCount = cartCount,
            tongTien = tongTien,
            phiVanChuyen = phiVanChuyen,
            vat = vat,
            tongThanhToan = tongThanhToan
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            db.Dispose();
        }
        base.Dispose(disposing);
    }

    // ========== TÍNH NĂNG ĐẶT HÀNG ==========

    // GET: /Cart/Checkout
    // ========== PHẦN KHUYẾN MÃI - VOUCHER ==========

    // GET: /Cart/Checkout
    public ActionResult Checkout(string discountCode = "")
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cartItems = db.GioHangs
            .Where(gh => gh.MAND == user.MAND)
            .Include(gh => gh.SanPham)
            .ToList();

        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống";
            return RedirectToAction("Index");
        }

        // Tính tổng tiền
        decimal tongTien = cartItems.Sum(item => (item.SanPham.GIA * item.SOLUONG) ?? 0);
        decimal phiVanChuyen = tongTien >= 500000 || tongTien == 0 ? 0 : 20000;

        // Kiểm tra và áp dụng khuyến mãi nếu có
        decimal giamGia = 0;
        string discountMessage = "";

        if (!string.IsNullOrEmpty(discountCode))
        {
            var discountResult = ApplyDiscount(discountCode, user.MAND, tongTien);
            giamGia = discountResult.Item1; // Chỉ có Item1 và Item2
            discountMessage = discountResult.Item2; // Item2 là message

            // Kiểm tra thông báo lỗi
            if (!string.IsNullOrEmpty(discountMessage) &&
                (discountMessage.Contains("không tồn tại") ||
                 discountMessage.Contains("hết hạn") ||
                 discountMessage.Contains("không khả dụng") ||
                 discountMessage.Contains("tối thiểu")))
            {
                TempData["DiscountError"] = discountMessage;
                discountCode = ""; // Reset mã nếu không hợp lệ
            }
            else if (!string.IsNullOrEmpty(discountMessage))
            {
                TempData["DiscountSuccess"] = discountMessage;
                TempData["DiscountCode"] = discountCode;
            }
        }

        decimal vat = (tongTien - giamGia) * 0.08m;
        decimal tongThanhToan = tongTien + phiVanChuyen + vat - giamGia;

        // Lấy danh sách địa chỉ của user
        var diaChiList = db.DiaChiNguoiDungs.Where(d => d.NguoiDungId == user.MAND).ToList();

        // Lấy danh sách khuyến mãi khả dụng
        var availableDiscounts = GetAvailableDiscounts(tongTien);

        var model = new CheckoutViewModel
        {
            CartItems = cartItems,
            TongTien = tongTien,
            PhiVanChuyen = phiVanChuyen,
            VAT = vat,
            TongThanhToan = tongThanhToan,
            DiaChiList = diaChiList,
            AvailableDiscounts = availableDiscounts,
            DiscountCode = discountCode,
            DiscountAmount = giamGia,
            DiscountMessage = discountMessage,
            HoTenNhanHang = user.HOTEN,
            SoDienThoai = user.SODIENTHOAI,
            DiaChi = user.DIACHI
        };

        return View(model);
    }

    // POST: /Cart/ApplyDiscount - Áp dụng mã giảm giá
    [HttpPost]
    public ActionResult ApplyDiscount(string discountCode)
    {
        try
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            // Lấy giỏ hàng hiện tại
            var cartItems = db.GioHangs
                .Where(gh => gh.MAND == user.MAND)
                .Include(gh => gh.SanPham)
                .ToList();

            if (!cartItems.Any())
            {
                return Json(new { success = false, message = "Giỏ hàng của bạn đang trống!" });
            }

            decimal tongTien = cartItems.Sum(item => (item.SanPham.GIA * item.SOLUONG) ?? 0);

            // Kiểm tra và áp dụng khuyến mãi
            var discountResult = ApplyDiscount(discountCode, user.MAND, tongTien);
            var giamGia = discountResult.Item1; // Số tiền giảm
            var message = discountResult.Item2; // Thông báo

            // Kiểm tra nếu có lỗi
            if (message.Contains("không tồn tại") || message.Contains("hết hạn") ||
                message.Contains("không khả dụng") || message.Contains("tối thiểu"))
            {
                return Json(new { success = false, message = message });
            }

            // Tính toán lại tổng tiền
            decimal phiVanChuyen = tongTien >= 500000 || tongTien == 0 ? 0 : 20000;

            // Kiểm tra nếu mã là FREESHIP thì miễn phí vận chuyển
            if (discountCode.ToUpper() == "FREESHIP")
            {
                phiVanChuyen = 0;
            }

            decimal vat = (tongTien - giamGia) * 0.08m;
            decimal tongThanhToan = tongTien + phiVanChuyen + vat - giamGia;

            return Json(new
            {
                success = true,
                message = message,
                discountCode = discountCode,
                discountAmount = giamGia,
                phiVanChuyen = phiVanChuyen,
                vat = vat,
                tongThanhToan = tongThanhToan,
                // Không có discountType và discountPercentage vì method không trả về
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
        }
    }

    // POST: /Cart/RemoveDiscount - Xóa mã giảm giá
    [HttpPost]
    public ActionResult RemoveDiscount()
    {
        try
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            // Lấy giỏ hàng hiện tại
            var cartItems = db.GioHangs
                .Where(gh => gh.MAND == user.MAND)
                .Include(gh => gh.SanPham)
                .ToList();

            decimal tongTien = cartItems.Sum(item => (item.SanPham.GIA * item.SOLUONG) ?? 0);
            decimal phiVanChuyen = tongTien >= 500000 || tongTien == 0 ? 0 : 20000;
            decimal vat = tongTien * 0.08m;
            decimal tongThanhToan = tongTien + phiVanChuyen + vat;

            return Json(new
            {
                success = true,
                message = "Đã xóa mã giảm giá!",
                discountCode = "",
                discountAmount = 0,
                phiVanChuyen = phiVanChuyen,
                vat = vat,
                tongThanhToan = tongThanhToan
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
        }
    }

    // Phương thức hỗ trợ: Kiểm tra và áp dụng khuyến mãi
    private Tuple<decimal, string> ApplyDiscount(string discountCode, int userId, decimal totalAmount)
    {
        if (string.IsNullOrEmpty(discountCode))
        {
            return Tuple.Create(0m, "");
        }

        // 1. Kiểm tra trong database trước
        var khuyenMai = db.KHUYENMAIs
            .FirstOrDefault(km => km.TENKM.ToUpper() == discountCode.ToUpper() ||
                                 km.MAKM.ToString() == discountCode);

        if (khuyenMai != null)
        {
            // Kiểm tra thời hạn
            var now = DateTime.Now;
            if (khuyenMai.NGAYBATDAU.HasValue && khuyenMai.NGAYBATDAU > now)
            {
                return Tuple.Create(0m, "Khuyến mãi chưa bắt đầu!");
            }

            if (khuyenMai.NGAYKETTHUC.HasValue && khuyenMai.NGAYKETTHUC < now)
            {
                return Tuple.Create(0m, "Khuyến mãi đã hết hạn!");
            }

            // Kiểm tra trạng thái
            if (khuyenMai.TRANGTHAI != "Active")
            {
                return Tuple.Create(0m, "Khuyến mãi không khả dụng!");
            }

            // Tính toán giảm giá
            decimal giamGia = 0;
            string message = "";

            if (khuyenMai.PHANTRAMGIAM.HasValue)
            {
                // Giảm giá theo phần trăm
                giamGia = totalAmount * (khuyenMai.PHANTRAMGIAM.Value / 100m);

                // Giới hạn tối đa nếu cần
                if (giamGia > totalAmount * 0.5m) // Giới hạn tối đa 50% đơn hàng
                {
                    giamGia = totalAmount * 0.5m;
                }

                message = $"Áp dụng thành công mã giảm giá {khuyenMai.PHANTRAMGIAM}%!";
            }
            else
            {
                message = "Áp dụng thành công mã khuyến mãi!";
            }

            return Tuple.Create(giamGia, message);
        }

        // 2. Nếu không có trong database, kiểm tra mã mặc định
        switch (discountCode.ToUpper())
        {
            case "SALE10":
                if (totalAmount >= 100000)
                {
                    decimal giamGia = totalAmount * 0.1m;
                    return Tuple.Create(giamGia, "Áp dụng thành công mã giảm giá 10%!");
                }
                return Tuple.Create(0m, "Đơn hàng tối thiểu 100,000đ để áp dụng mã SALE10!");

            case "SALE20":
                if (totalAmount >= 200000)
                {
                    decimal giamGia = totalAmount * 0.2m;
                    return Tuple.Create(giamGia, "Áp dụng thành công mã giảm giá 20%!");
                }
                return Tuple.Create(0m, "Đơn hàng tối thiểu 200,000đ để áp dụng mã SALE20!");

            case "FREESHIP":
                return Tuple.Create(0m, "Áp dụng thành công mã freeship!");

            case "GIAM50K":
                if (totalAmount >= 300000)
                {
                    return Tuple.Create(50000m, "Áp dụng thành công mã giảm giá 50,000đ!");
                }
                return Tuple.Create(0m, "Đơn hàng tối thiểu 300,000đ để áp dụng mã GIAM50K!");

            default:
                return Tuple.Create(0m, "Mã khuyến mãi không tồn tại!");
        }
    }

    // Phương thức lấy danh sách khuyến mãi khả dụng (đơn giản)
    private List<KHUYENMAI> GetAvailableDiscounts(decimal totalAmount)
    {
        var now = DateTime.Now;

        return db.KHUYENMAIs
            .Where(km => km.TRANGTHAI == "Active" &&
                        (!km.NGAYBATDAU.HasValue || km.NGAYBATDAU <= now) &&
                        (!km.NGAYKETTHUC.HasValue || km.NGAYKETTHUC >= now))
            .OrderByDescending(km => km.PHANTRAMGIAM)
            .Take(10) // Chỉ lấy 10 mã
            .ToList();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult PlaceOrder(CheckoutViewModel model)
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập!" }, JsonRequestBehavior.AllowGet);
        }

        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Vui lòng kiểm tra lại thông tin!" }, JsonRequestBehavior.AllowGet);
        }

        try
        {
            // 1. Kiểm tra lại giỏ hàng
            var cartItems = db.GioHangs
                .Where(gh => gh.MAND == user.MAND)
                .Include(gh => gh.SanPham)
                .ToList();

            if (!cartItems.Any())
            {
                return Json(new { success = false, message = "Giỏ hàng của bạn đang trống!" }, JsonRequestBehavior.AllowGet);
            }

            // 2. Kiểm tra số lượng tồn kho
            foreach (var item in cartItems)
            {
                if (item.SOLUONG > item.SanPham.SOLUONGTON)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Sản phẩm {item.SanPham.TENSP} chỉ còn {item.SanPham.SOLUONGTON} sản phẩm trong kho!"
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            // 3. Tính toán tổng tiền và áp dụng khuyến mãi
            decimal tongTien = cartItems.Sum(item => (item.SanPham.GIA * item.SOLUONG) ?? 0);
            decimal phiGiaoHang = TinhPhiGiaoHang(model.PhuongThucGiaoHang);

            // Áp dụng khuyến mãi nếu có
            decimal giamGia = 0;

            if (!string.IsNullOrEmpty(model.DiscountCode))
            {
                var discountResult = ApplyDiscount(model.DiscountCode, user.MAND, tongTien);
                giamGia = discountResult.Item1;

                // Nếu là freeship, miễn phí vận chuyển
                if (model.DiscountCode.ToUpper() == "FREESHIP")
                {
                    phiGiaoHang = 0;
                    giamGia = 0;
                }
            }

            decimal vat = (tongTien - giamGia) * 0.08m;
            decimal tongThanhToan = tongTien + phiGiaoHang + vat - giamGia;

            // 4. Tạo đơn hàng
            var donHang = new DonHang
            {
                MAND = user.MAND,
                NGAYDAT = DateTime.Now,
                TRANG_THAI = "Chờ xác nhận",
                TrangThaiChiTiet = "Đang xử lý",

                // Thông tin giao hàng
                HoTenNhanHang = model.HoTenNhanHang,
                SDTNGUOINHAN = model.SoDienThoai,
                DIACHIGIAOHANG = model.DiaChi,
                TinhThanh = model.TinhThanh,
                QuanHuyen = model.QuanHuyen,
                PhuongXa = model.PhuongXa,
                HinhThucThanhToan = model.HinhThucThanhToan,
                PhuongThucGiaoHang = model.PhuongThucGiaoHang,
                PhiGiaoHang = phiGiaoHang,
                GHI_CHU = model.GhiChu,
                TONGTIEN = tongTien,
                TongThanhToan = tongThanhToan
            };

            // Thêm thông tin khuyến mãi vào ghi chú
            if (!string.IsNullOrEmpty(model.DiscountCode))
            {
                donHang.GHI_CHU += $"\nĐã áp dụng mã giảm giá: {model.DiscountCode}";
                if (giamGia > 0)
                {
                    donHang.GHI_CHU += $"\nGiảm giá: {giamGia.ToString("N0")}đ";
                }
                if (model.DiscountCode.ToUpper() == "FREESHIP")
                {
                    donHang.GHI_CHU += "\nĐược miễn phí vận chuyển";
                }
            }

            db.DonHangs.Add(donHang);
            db.SaveChanges();

            // 5. Tạo chi tiết đơn hàng
            foreach (var cartItem in cartItems)
            {
                var chiTiet = new ChiTietDonHang
                {
                    MADH = donHang.MADH,
                    MASP = (int)cartItem.MASP,
                    SOLUONG = cartItem.SOLUONG,
                    THANHTIEN = cartItem.SanPham.GIA
                };

                // Cập nhật số lượng tồn kho
                var sanPham = db.SanPhams.Find(cartItem.MASP);
                if (sanPham != null)
                {
                    sanPham.SOLUONGTON -= cartItem.SOLUONG;

                    // Nếu hết hàng, cập nhật trạng thái
                    if (sanPham.SOLUONGTON <= 0)
                    {
                        sanPham.TRANGTHAI = "Hết hàng";
                    }
                }

                db.ChiTietDonHangs.Add(chiTiet);
            }

            // 6. Xóa giỏ hàng
            foreach (var cartItem in cartItems)
            {
                db.GioHangs.Remove(cartItem);
            }

            db.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Đặt hàng thành công!",
                orderId = donHang.MADH
            }, JsonRequestBehavior.AllowGet);
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi khi đặt hàng: " + ex.Message }, JsonRequestBehavior.AllowGet);
        }
    }
    // Thêm vào CartController
    [HttpPost]
    public ActionResult AddComboToCart(int comboId)
    {
        try
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }
            // Tạm thời xử lý đơn giản: thêm sản phẩm đơn lẻ
            var product = db.SanPhams.FirstOrDefault(p => p.MASP == comboId);
            if (product == null || product.TRANGTHAI != "Còn hàng")
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại hoặc hết hàng!" });
            }

            // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
            var existingCartItem = db.GioHangs
                .FirstOrDefault(gh => gh.MAND == user.MAND && gh.MASP == comboId);

            if (existingCartItem != null)
            {
                // Cập nhật số lượng
                existingCartItem.SOLUONG += 1;
            }
            else
            {
                // Thêm mới vào giỏ hàng
                var cartItem = new GioHang
                {
                    MAND = user.MAND,
                    MASP = comboId,
                    SOLUONG = 1,
                    NGAYTHEM = DateTime.Now
                    // BỎ các trường không tồn tại:
                    // IS_COMBO = true,
                    // COMBO_ID = comboId
                };
                db.GioHangs.Add(cartItem);
            }

            db.SaveChanges();

            // Đếm tổng số sản phẩm
            var cartCount = db.GioHangs
                .Where(gh => gh.MAND == user.MAND)
                .Sum(gh => gh.SOLUONG) ?? 0;

            return Json(new
            {
                success = true,
                message = $"Đã thêm sản phẩm vào giỏ hàng!",
                cartCount = cartCount
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
        }
    }


    // POST: /Cart/AddFlashSaleToCart - Sửa để không dùng các trường không tồn tại
    [HttpPost]
    public ActionResult AddFlashSaleToCart(int flashSaleId, int quantity = 1)
    {
        try
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            // Lấy thông tin sản phẩm flash sale
            // Giả sử flashSaleId thực chất là MASP của sản phẩm đang flash sale
            var product = db.SanPhams.Find(flashSaleId);

            if (product == null || product.TRANGTHAI != "Còn hàng")
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại hoặc hết hàng!" });
            }

            // Kiểm tra số lượng tồn
            if (quantity > product.SOLUONGTON)
            {
                return Json(new { success = false, message = "Số lượng vượt quá tồn kho!" });
            }

            // Thêm vào giỏ hàng
            var existingCartItem = db.GioHangs
                .FirstOrDefault(gh => gh.MAND == user.MAND && gh.MASP == flashSaleId);

            if (existingCartItem != null)
            {
                existingCartItem.SOLUONG += quantity;
            }
            else
            {
                var cartItem = new GioHang
                {
                    MAND = user.MAND,
                    MASP = flashSaleId,
                    SOLUONG = quantity,
                    NGAYTHEM = DateTime.Now
                    // BỎ các trường không tồn tại:
                    // IS_FLASH_SALE = true,
                    // FLASH_SALE_ID = flashSaleId,
                    // GIA_FLASH_SALE = flashSale.GIAKHUYENMAI
                };
                db.GioHangs.Add(cartItem);
            }

            db.SaveChanges();

            var cartCount = db.GioHangs
                .Where(gh => gh.MAND == user.MAND)
                .Sum(gh => gh.SOLUONG) ?? 0;

            return Json(new
            {
                success = true,
                message = $"Đã thêm sản phẩm vào giỏ hàng!",
                cartCount = cartCount
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Lỗi: " + ex.Message });
        }
    }

    // GET: /Cart/OrderSuccess
    // GET: /Cart/OrderSuccess - Trang thành công sau đặt hàng
    public ActionResult OrderSuccess(int orderId)
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var donHang = db.DonHangs
            .Include(d => d.ChiTietDonHangs)
            .Include(d => d.ChiTietDonHangs.Select(ct => ct.SanPham))
            .FirstOrDefault(d => d.MADH == orderId && d.MAND == user.MAND);

        if (donHang == null)
        {
            // Nếu không tìm thấy đơn hàng, chuyển hướng về trang chủ với thông báo
            TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
            return RedirectToAction("Index", "Home");
        }

        return View(donHang);
    }
    // AJAX: Tính phí giao hàng
    [HttpPost]
    public ActionResult TinhPhiGiaoHang(string phuongThucGiaoHang, decimal tongTien)
    {
        decimal phiGiaoHang = TinhPhiGiaoHang(phuongThucGiaoHang);
        decimal vat = tongTien * 0.08m;
        decimal tongThanhToan = tongTien + phiGiaoHang + vat;

        return Json(new
        {
            phiGiaoHang = phiGiaoHang,
            vat = vat,
            tongThanhToan = tongThanhToan
        });
    }

    private decimal TinhPhiGiaoHang(string phuongThucGiaoHang)
    {
        if (phuongThucGiaoHang == "Giao hàng nhanh")
        {
            return 30000;
        }
        else // Giao hàng tiêu chuẩn
        {
            return 20000;
        }
    }


    private List<TimelineItem> TaoTimelineDonHang(DonHang donHang)
    {
        var timeline = new List<TimelineItem>();
        var ngayDatHang = donHang.NGAYDAT;

        // Mặc định: Đơn hàng được đặt
        timeline.Add(new TimelineItem
        {
            Title = "Đơn hàng được đặt",
            Description = "Đơn hàng đã được tiếp nhận",
            Date = ngayDatHang,
            IsCompleted = true,
            Icon = "📝"
        });

        // Trạng thái: Đang xử lý
        var ngayXuLy = ngayDatHang?.AddHours(1) ?? ngayDatHang; // Sửa ở đây
        timeline.Add(new TimelineItem
        {
            Title = "Đang xử lý",
            Description = "Đơn hàng đang được xử lý và chuẩn bị",
            Date = ngayXuLy,
            IsCompleted = donHang.TrangThaiChiTiet != "Đang xử lý" &&
                         donHang.TrangThaiChiTiet != "Hủy",
            IsCurrent = donHang.TrangThaiChiTiet == "Đang xử lý",
            Icon = "🔄"
        });

        // Trạng thái: Đang giao
        var ngayGiaoHang = ngayXuLy?.AddDays(1) ?? ngayDatHang; // Sửa ở đây
        timeline.Add(new TimelineItem
        {
            Title = "Đang giao hàng",
            Description = "Đơn hàng đang được vận chuyển",
            Date = ngayGiaoHang,
            IsCompleted = donHang.TrangThaiChiTiet == "Hoàn tất",
            IsCurrent = donHang.TrangThaiChiTiet == "Đang giao",
            Icon = "🚚"
        });

        // Trạng thái: Hoàn tất
        var ngayHoanTat = ngayGiaoHang?.AddDays(1) ?? ngayDatHang; // Sửa ở đây
        timeline.Add(new TimelineItem
        {
            Title = "Giao hàng thành công",
            Description = "Đơn hàng đã được giao thành công",
            Date = donHang.TrangThaiChiTiet == "Hoàn tất" ? ngayHoanTat : null,
            IsCompleted = donHang.TrangThaiChiTiet == "Hoàn tất",
            IsCurrent = false,
            Icon = "✅"
        });

        // Trạng thái: Hủy
        if (donHang.TrangThaiChiTiet == "Hủy")
        {
            timeline.Add(new TimelineItem
            {
                Title = "Đơn hàng đã hủy",
                Description = "Đơn hàng đã được hủy",
                Date = DateTime.Now,
                IsCompleted = true,
                Icon = "❌"
            });
        }

        return timeline;
    }
    // GET: /Cart/OrderHistory - Lịch sử đơn hàng
    public ActionResult OrderHistory()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var donHangs = db.DonHangs
            .Where(d => d.MAND == user.MAND)
            .Include(d => d.ChiTietDonHangs) // THÊM: Include ChiTietDonHangs
            .Include(d => d.ChiTietDonHangs.Select(ct => ct.SanPham)) // THÊM: Include SanPham
            .OrderByDescending(d => d.NGAYDAT)
            .ToList();

        return View(donHangs);
    }

    // GET: /Cart/OrderDetails/5 - Chi tiết đơn hàng
    public ActionResult OrderDetails(int id)
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var donHang = db.DonHangs
            .Include(d => d.ChiTietDonHangs)
            .Include(d => d.ChiTietDonHangs.Select(ct => ct.SanPham))
            .FirstOrDefault(d => d.MADH == id && d.MAND == user.MAND);

        if (donHang == null)
        {
            return HttpNotFound();
        }

        // Tạo timeline
        var timeline = TaoTimelineDonHang(donHang);
        ViewBag.Timeline = timeline;

        return View(donHang);
    }

    // POST: /Cart/CancelOrder - Hủy đơn hàng
    [HttpPost]
    public ActionResult CancelOrder(int id)
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập!" });
        }

        var donHang = db.DonHangs.FirstOrDefault(d => d.MADH == id && d.MAND == user.MAND);
        if (donHang == null)
        {
            return Json(new { success = false, message = "Đơn hàng không tồn tại!" });
        }

        // Chỉ cho phép hủy đơn hàng ở trạng thái "Đang xử lý"
        if (donHang.TrangThaiChiTiet != "Đang xử lý")
        {
            return Json(new
            {
                success = false,
                message = "Không thể hủy đơn hàng. Đơn hàng đã được xử lý!"
            });
        }

        try
        {
            // Cập nhật trạng thái
            donHang.TRANG_THAI = "Đã hủy";
            donHang.TrangThaiChiTiet = "Hủy";

            // Hoàn trả số lượng sản phẩm
            var chiTietDonHangs = db.ChiTietDonHangs.Where(ct => ct.MADH == id).ToList();
            foreach (var chiTiet in chiTietDonHangs)
            {
                var sanPham = db.SanPhams.Find(chiTiet.MASP);
                if (sanPham != null)
                {
                    sanPham.SOLUONGTON += chiTiet.SOLUONG;
                    // Cập nhật lại trạng thái nếu cần
                    if (sanPham.SOLUONGTON > 0 && sanPham.TRANGTHAI == "Hết hàng")
                    {
                        sanPham.TRANGTHAI = "Còn hàng";
                    }
                }
            }

            db.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Đã hủy đơn hàng thành công!"
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = "Lỗi khi hủy đơn hàng: " + ex.Message
            });
        }
    }
    // GET: /Cart/OrderTracking/5 - Theo dõi đơn hàng chi tiết
    public ActionResult OrderTracking(int id)
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var donHang = db.DonHangs
            .Include(d => d.ChiTietDonHangs)
            .Include(d => d.ChiTietDonHangs.Select(ct => ct.SanPham))
            .FirstOrDefault(d => d.MADH == id && d.MAND == user.MAND);

        if (donHang == null)
        {
            return HttpNotFound();
        }

        // Tạo timeline
        var timeline = TaoTimelineDonHang(donHang);
        ViewBag.Timeline = timeline;

        return View(donHang);
    }
    // GET: /Cart/Promotions - Trang khuyến mãi
    public ActionResult Promotions()
    {
        var now = DateTime.Now;

        var promotions = db.KHUYENMAIs
            .Where(km => km.TRANGTHAI == "Active" &&
                        (!km.NGAYBATDAU.HasValue || km.NGAYBATDAU <= now) &&
                        (!km.NGAYKETTHUC.HasValue || km.NGAYKETTHUC >= now))
            .OrderByDescending(km => km.NGAYBATDAU)
            .ToList();

        ViewBag.DefaultPromotions = new List<dynamic>
    {
        new { Code = "SALE10", Title = "Giảm 10%", Description = "Áp dụng cho đơn hàng từ 100,000đ", Expiry = "31/12/2024" },
        new { Code = "SALE20", Title = "Giảm 20%", Description = "Áp dụng cho đơn hàng từ 200,000đ", Expiry = "31/12/2024" },
        new { Code = "FREESHIP", Title = "Miễn phí vận chuyển", Description = "Áp dụng cho tất cả đơn hàng", Expiry = "31/12/2024" },
        new { Code = "GIAM50K", Title = "Giảm 50,000đ", Description = "Áp dụng cho đơn hàng từ 300,000đ", Expiry = "31/12/2024" }
    };

        return View(promotions);
    }
}