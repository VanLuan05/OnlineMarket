using OnlineMarket;
using OnlineMarket.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

public class OrderController : Controller
{
    private QL_OnlineMarketEntities db = new QL_OnlineMarketEntities();

    // GET: /Order/History - Lịch sử đơn hàng
    public ActionResult History()
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var donHangs = db.DonHangs
            .Where(d => d.MAND == user.MAND)
            .OrderByDescending(d => d.NGAYDAT)
            .ToList();

        return View(donHangs);
    }

    // GET: /Order/Details/5 - Chi tiết đơn hàng
    public ActionResult Details(int id)
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

        return View(donHang);
    }

    // GET: /Order/Tracking/5 - Theo dõi đơn hàng
    public ActionResult Tracking(int id)
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

        // Tạo dữ liệu timeline
        var timeline = TaoTimelineDonHang(donHang);
        ViewBag.Timeline = timeline;

        return View(donHang);
    }

    // POST: /Order/Cancel/5 - Hủy đơn hàng
    [HttpPost]
    public ActionResult Cancel(int id)
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

        // Chỉ cho phép hủy đơn hàng ở trạng thái "Chờ xác nhận"
        if (donHang.TRANG_THAI != "Chờ xác nhận")
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

    // ========== PHƯƠNG THỨC HỖ TRỢ ==========

    private NguoiDung GetCurrentUser()
    {
        if (Session["UserID"] != null)
        {
            // Sử dụng Convert.ToInt32 để chuyển đổi an toàn từ String sang Int
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

        if (User.Identity.IsAuthenticated)
        {
            var userName = User.Identity.Name;
            return db.NguoiDungs.FirstOrDefault(u => u.TENDANGNHAP == userName);
        }

        return null;
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            db.Dispose();
        }
        base.Dispose(disposing);
    }
}

// Model cho Timeline
