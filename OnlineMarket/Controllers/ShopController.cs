using OnlineMarket;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineMarket.Controllers; // THÊM NÀY
public class ShopController : Controller
{
    private QL_OnlineMarketEntities db = new QL_OnlineMarketEntities();

    // GET: /Shop/
    public ActionResult Index(string search, string sortOrder, int? categoryId)
    {
        // Khởi tạo ViewBag với giá trị mặc định
        ViewBag.Search = search ?? "";
        ViewBag.CategoryId = categoryId;
        ViewBag.PriceSort = sortOrder == "price_asc" ? "price_desc" : "price_asc";
        ViewBag.NameSort = sortOrder == "name_asc" ? "name_desc" : "name_asc";
        ViewBag.PopularSort = sortOrder == "popular" ? "popular_desc" : "popular";

        // Lấy tất cả sản phẩm còn hàng
        var products = db.SanPhams.Where(sp => sp.TRANGTHAI == "Còn hàng");

        // Lọc theo danh mục
        if (categoryId.HasValue && categoryId > 0)
        {
            products = products.Where(p => p.MADM == categoryId);
        }

        // Tìm kiếm
        if (!string.IsNullOrEmpty(search))
        {
            products = products.Where(p => p.TENSP.Contains(search) || p.MOTA.Contains(search));
        }

        // Sắp xếp
        switch (sortOrder)
        {
            case "price_asc":
                products = products.OrderBy(p => p.GIA);
                break;
            case "price_desc":
                products = products.OrderByDescending(p => p.GIA);
                break;
            case "name_asc":
                products = products.OrderBy(p => p.TENSP);
                break;
            case "name_desc":
                products = products.OrderByDescending(p => p.TENSP);
                break;
            case "popular":
                products = products.OrderBy(p => p.LUOTMUA);
                break;
            case "popular_desc":
                products = products.OrderByDescending(p => p.LUOTMUA);
                break;
            default:
                products = products.OrderBy(p => p.NGAYTHEM);
                break;
        }

        // Lấy danh mục cho filter - đảm bảo không null
        var categories = db.DanhMucs.ToList();
        ViewBag.Categories = categories ?? new List<DanhMuc>();

        return View(products.ToList());
    }

    // GET: /Shop/Details/1
    // GET: /Shop/Details/1
    // GET: /Shop/Details/1
    public ActionResult Details(int id)
    {
        var product = db.SanPhams
            .Include(p => p.KHUYENMAI) // Quan trọng: Include khuyến mãi
            .Include(p => p.DanhGias)  // Include đánh giá
            .FirstOrDefault(p => p.MASP == id);

        if (product == null || product.TRANGTHAI != "Còn hàng")
        {
            return HttpNotFound();
        }

        // Lấy đánh giá từ database
        var danhGia = db.DanhGias
            .Where(dg => dg.MASP == id)
            .Include(dg => dg.NguoiDung)
            .OrderByDescending(dg => dg.NGAYDANHGIA)
            .ToList();

        // Tính toán thống kê đánh giá
        var tongDanhGia = danhGia.Count;
        var trungBinhSao = danhGia.Any() ? danhGia.Average(dg => dg.SOSAO ?? 0) : 0;
        var phanPhoiSao = new int[5];

        for (int i = 0; i < 5; i++)
        {
            phanPhoiSao[i] = danhGia.Count(dg => dg.SOSAO == i + 1);
        }

        // Lấy sản phẩm liên quan
        var relatedProducts = db.SanPhams
            .Where(p => p.MADM == product.MADM && p.MASP != id && p.TRANGTHAI == "Còn hàng")
            .OrderByDescending(p => p.LUOTMUA)
            .Take(4)
            .ToList();

        // Truyền dữ liệu riêng lẻ qua ViewBag
        ViewBag.DanhGia = danhGia;
        ViewBag.TongDanhGia = tongDanhGia;
        ViewBag.TrungBinhSao = trungBinhSao;
        ViewBag.PhanPhoiSao = phanPhoiSao;
        ViewBag.RelatedProducts = relatedProducts;

        return View(product);
    }


    // ============Dánh giá sản phẩm============
    // Kiểm tra user đã mua sản phẩm chưa
    private bool DaMuaSanPham(int userId, int productId)
    {
        return db.ChiTietDonHangs
            .Any(ct => ct.DonHang.MAND == userId && ct.MASP == productId && ct.DonHang.TRANG_THAI == "Hoàn tất");
    }

    // GET: Hiển thị form viết đánh giá
    public ActionResult CreateReview(int productId)
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Kiểm tra user đã mua sản phẩm này chưa
        if (!DaMuaSanPham(user.MAND, productId))
        {
            TempData["ErrorMessage"] = "Bạn cần mua sản phẩm này trước khi đánh giá!";
            return RedirectToAction("Details", "Shop", new { id = productId });
        }

        // Kiểm tra đã đánh giá chưa
        var existingReview = db.DanhGias
            .FirstOrDefault(dg => dg.MAND == user.MAND && dg.MASP == productId);

        if (existingReview != null)
        {
            TempData["ErrorMessage"] = "Bạn đã đánh giá sản phẩm này rồi!";
            return RedirectToAction("Details", "Shop", new { id = productId });
        }

        var product = db.SanPhams.Find(productId);
        if (product == null)
        {
            return HttpNotFound();
        }

        ViewBag.Product = product;
        return View();
    }

    // POST: Xử lý đánh giá
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult CreateReview(int productId, int rating, string comment, HttpPostedFileBase image)
    {
        var user = GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            // Kiểm tra lại
            if (!DaMuaSanPham(user.MAND, productId))
            {
                return Json(new { success = false, message = "Bạn cần mua sản phẩm này trước khi đánh giá!" });
            }

            var existingReview = db.DanhGias
                .FirstOrDefault(dg => dg.MAND == user.MAND && dg.MASP == productId);

            if (existingReview != null)
            {
                return Json(new { success = false, message = "Bạn đã đánh giá sản phẩm này rồi!" });
            }

            // Xử lý upload ảnh
            string imagePath = null;
            if (image != null && image.ContentLength > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = "Chỉ chấp nhận file ảnh JPG, PNG, GIF!" });
                }

                if (image.ContentLength > 5 * 1024 * 1024) // 5MB
                {
                    return Json(new { success = false, message = "Kích thước file tối đa là 5MB!" });
                }

                // Lưu file
                var fileName = $"review_{user.MAND}_{productId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var path = Path.Combine(Server.MapPath("~/Content/img/reviews/"), fileName);
                image.SaveAs(path);
                imagePath = fileName;
            }

            // Tạo đánh giá mới
            var review = new DanhGia
            {
                MASP = productId,
                MAND = user.MAND,
                SOSAO = rating,
                NOIDUNG = comment,
                HINHANH = imagePath,
                NGAYDANHGIA = DateTime.Now
            };

            db.DanhGias.Add(review);
            db.SaveChanges();

            // Cập nhật tổng quan đánh giá cho sản phẩm
            CapNhatTongQuanDanhGia(productId);

            return Json(new
            {
                success = true,
                message = "Cảm ơn bạn đã đánh giá sản phẩm!",
                reviewId = review.MADG
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
        }
    }

    // Cập nhật tổng quan đánh giá
    private void CapNhatTongQuanDanhGia(int productId)
    {
        var reviews = db.DanhGias.Where(dg => dg.MASP == productId).ToList();

        // Logic cập nhật tổng quan đánh giá (nếu cần)
        // Có thể lưu vào bảng cache hoặc tính toán real-time
    }

    // Lấy user hiện tại
    private NguoiDung GetCurrentUser()
    {
        var username = User.Identity.Name;
        return db.NguoiDungs.FirstOrDefault(u => u.TENDANGNHAP == username);
    }
    // THÊM CÁC METHOD PUBLIC ĐỂ GỌI TỪ VIEW
    public NguoiDung GetCurrentUserPublic()
    {
        var username = User.Identity.Name;
        return db.NguoiDungs.FirstOrDefault(u => u.TENDANGNHAP == username);
    }

    public bool DaMuaSanPhamPublic(int userId, int productId)
    {
        return db.ChiTietDonHangs
            .Any(ct => ct.DonHang.MAND == userId &&
                       ct.MASP == productId &&
                       ct.DonHang.TRANG_THAI == "Hoàn tất");
    }
}