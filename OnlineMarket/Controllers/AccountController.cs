using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
// Đảm bảo các using này đúng với tên Project của bạn
using OnlineMarket;
using OnlineMarket.Models;

namespace OnlineMarket.Controllers
{
    public class AccountController : Controller
    {
        // SỬA TÊN NÀY NẾU TÊN ENTITIES TRONG PROJECT CỦA BẠN KHÁC
        private QL_OnlineMarketEntities db = new QL_OnlineMarketEntities();

        // =========================================================================
        // PHẦN 1: XÁC THỰC (LOGIN / REGISTER / LOGOUT)
        // =========================================================================

        // GET: Đăng ký
        public ActionResult Register()
        {
            return View();
        }

        // POST: Đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(NguoiDung nguoiDung)
        {
            if (ModelState.IsValid)
            {
                if (db.NguoiDungs.Any(u => u.EMAIL == nguoiDung.EMAIL))
                {
                    ModelState.AddModelError("EMAIL", "Email đã được sử dụng");
                    return View(nguoiDung);
                }

                if (db.NguoiDungs.Any(u => u.TENDANGNHAP == nguoiDung.TENDANGNHAP))
                {
                    ModelState.AddModelError("TENDANGNHAP", "Tên đăng nhập đã được sử dụng");
                    return View(nguoiDung);
                }

                // Mã hóa mật khẩu và set vai trò mặc định
                nguoiDung.MATKHAU = SimpleHash(nguoiDung.MATKHAU);
                if (string.IsNullOrEmpty(nguoiDung.VAITRO))
                {
                    nguoiDung.VAITRO = "Khách hàng";
                }

                db.NguoiDungs.Add(nguoiDung);
                db.SaveChanges();

                // Tự động đăng nhập sau khi đăng ký
                SetUserSession(nguoiDung);
                return RedirectToAction("Index", "Home");
            }
            return View(nguoiDung);
        }

        public ActionResult Login()
        {
            return View();
        }

        // GET: Đăng nhập
        [HttpPost]
        public JsonResult Login(string tendangnhap, string matkhau)
        {
            // 1. Validate
            if (string.IsNullOrEmpty(tendangnhap) || string.IsNullOrEmpty(matkhau))
            {
                return Json(new
                {
                    success = false,
                    message = "Thiếu thông tin"
                });
            }

            // 2. Check DB
            var user = db.NguoiDungs.FirstOrDefault(u => u.TENDANGNHAP == tendangnhap);

            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Sai tài khoản hoặc mật khẩu"
                });
            }

            string hashPass = SimpleHash(matkhau);

            if (user.MATKHAU != hashPass && user.MATKHAU != matkhau)
            {
                return Json(new
                {
                    success = false,
                    message = "Sai tài khoản hoặc mật khẩu"
                });
            }

            // 3. Thành công
            return Json(new
            {
                success = true,
                message = "Đăng nhập thành công",
                role = user.VAITRO,
                token = Guid.NewGuid().ToString()
            });
        }



        // GET: Đăng nhập cho admin
        public ActionResult LoginAM()
        {
            if (CheckAdminLogin()) return RedirectToAction("Dashboard");
            if (Session["UserID"] != null) return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: Đăng nhập (Xử lý cả Admin Test và User DB)
        // POST: /Account/Login
        // POST: Đăng nhập Admin (AJAX)
        [HttpPost]
        public JsonResult LoginAM(string username, string password, bool rememberMe = false)
        {
            try
            {
                // 1. Kiểm tra input
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin!" });
                }

                // 2. Kiểm tra Backdoor Admin
                if (username == "admin" && password == "admin123")
                {
                    SetSession("0", "Super Admin", "admin@test.com", "Admin");
                    return Json(new
                    {
                        success = true,
                        message = "Đăng nhập Admin thành công!",
                        token = Guid.NewGuid().ToString()
                    });
                }

                // 3. Kiểm tra User trong Database có vai trò Admin
                var user = db.NguoiDungs.FirstOrDefault(u => u.TENDANGNHAP == username);
                if (user != null)
                {
                    string hashPass = SimpleHash(password);
                    if (user.MATKHAU == hashPass || user.MATKHAU == password)
                    {
                        // Chỉ cho phép Admin đăng nhập
                        if (user.VAITRO == "Admin" || user.VAITRO == "Quản trị viên")
                        {
                            SetSession(user.MAND.ToString(), user.HOTEN, user.EMAIL, user.VAITRO);
                            return Json(new
                            {
                                success = true,
                                message = "Đăng nhập Admin thành công!",
                            });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Bạn không có quyền truy cập trang Admin!"});
                        }
                    }
                }

                // 4. Thất bại
                return Json(new { success = false, message = "Sai tên đăng nhập hoặc mật khẩu!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        // Hàm phụ set session cho gọn code
        private void SetSession(string id, string name, string email, string role)
        {
            Session["UserID"] = id;
            Session["UserName"] = name;
            Session["UserEmail"] = email;
            Session["UserRole"] = role;

            // Nếu là Admin thì bật cờ AdminLoggedIn
            if (role == "Admin" || role == "Quản trị viên")
            {
                Session["AdminLoggedIn"] = true;
            }
        }

        // Đăng xuất
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();

            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "")
            {
                Expires = DateTime.Now.AddYears(-1)
            };
            Response.Cookies.Add(authCookie);

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, message = "Đã đăng xuất!" });
            }
            return RedirectToAction("Index", "Home");
        }

        // =========================================================================
        // PHẦN 2: CÁC HÀM HỖ TRỢ (HELPER)
        // =========================================================================

        private string SimpleHash(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private void SetUserSession(NguoiDung user)
        {
            Session["UserID"] = user.MAND;
            Session["UserName"] = user.HOTEN;
            Session["UserEmail"] = user.EMAIL;
            Session["UserRole"] = user.VAITRO;

            if (user.VAITRO == "Admin" || user.VAITRO == "Quản trị viên")
            {
                Session["AdminLoggedIn"] = true;
                Session["AdminUsername"] = user.TENDANGNHAP;
            }

            CreateAuthCookie(user.TENDANGNHAP, user.VAITRO);
        }

        private void CreateAuthCookie(string username, string role)
        {
            var authTicket = new FormsAuthenticationTicket(
                1, username, DateTime.Now, DateTime.Now.AddMinutes(2880), false, role ?? ""
            );
            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            Response.Cookies.Add(authCookie);
        }

        private bool CheckAdminLogin()
        {
            if (Session["AdminLoggedIn"] != null && (bool)Session["AdminLoggedIn"]) return true;
            if (Session["UserRole"] != null &&
               (Session["UserRole"].ToString() == "Admin" || Session["UserRole"].ToString() == "Quản trị viên"))
                return true;

            return false;
        }

        // =========================================================================
        // PHẦN 3: ADMIN DASHBOARD & MANAGEMENT
        // =========================================================================

        public ActionResult Dashboard()
        {
            if (!CheckAdminLogin()) return RedirectToAction("Login");

            var model = new AdminDashboardViewModel
            {
                TotalProducts = db.SanPhams.Count(),
                TotalOrders = db.DonHangs.Count(),
                TotalUsers = db.NguoiDungs.Count(),
                TotalRevenue = db.DonHangs.Where(d => d.TRANG_THAI == "Hoàn thành").Sum(d => d.TongThanhToan) ?? 0,
                NewOrders = db.DonHangs.Where(d => d.TRANG_THAI == "Chờ xác nhận").OrderByDescending(d => d.NGAYDAT).Take(5).ToList(),
                LowStockProducts = db.SanPhams.Where(p => p.SOLUONGTON < 10 && p.SOLUONGTON > 0).Take(5).ToList(),
                MonthlyRevenue = GetMonthlyRevenue(),
                TopSellingProducts = GetTopSellingProducts()
            };

            return View(model);
        }

        // --- QUẢN LÝ SẢN PHẨM ---
        public ActionResult Products(string search = "", int categoryId = 0)
        {
            if (!CheckAdminLogin()) return RedirectToAction("Login");

            var products = db.SanPhams.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.TENSP.Contains(search) || p.MOTA.Contains(search));
            if (categoryId > 0)
                products = products.Where(p => p.MADM == categoryId);

            var model = products.Include(p => p.DanhMuc).OrderByDescending(p => p.MASP).ToList();
            ViewBag.Categories = db.DanhMucs.ToList();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            return View(model);
        }

        public ActionResult ProductCreate()
        {
            if (!CheckAdminLogin()) return RedirectToAction("Login");
            ViewBag.Categories = db.DanhMucs.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductCreate(SanPham product, HttpPostedFileBase imageFile)
        {
            if (!CheckAdminLogin()) return RedirectToAction("Login");

            try
            {
                if (ModelState.IsValid)
                {
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(imageFile.FileName);
                        string path = Path.Combine(Server.MapPath("~/Content/img/Products"), fileName);
                        // Tạo thư mục nếu chưa có
                        if (!Directory.Exists(Server.MapPath("~/Content/img/Products")))
                        {
                            Directory.CreateDirectory(Server.MapPath("~/Content/img/Products"));
                        }
                        imageFile.SaveAs(path);
                        product.URL_ANH = "/Content/img/Products/" + fileName;
                    }
                    product.NGAYTHEM = DateTime.Now;
                    product.TRANGTHAI = "Còn hàng";
                    db.SanPhams.Add(product);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    return RedirectToAction("Products");
                }
            }
            catch (Exception ex) { ModelState.AddModelError("", "Lỗi: " + ex.Message); }
            ViewBag.Categories = db.DanhMucs.ToList();
            return View(product);
        }

        public ActionResult ProductEdit(int id)
        {
            if (!CheckAdminLogin()) return RedirectToAction("Login");
            var product = db.SanPhams.Find(id);
            if (product == null) return HttpNotFound();
            ViewBag.Categories = db.DanhMucs.ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductEdit(SanPham product, HttpPostedFileBase imageFile)
        {
            if (!CheckAdminLogin()) return RedirectToAction("Login");
            try
            {
                if (ModelState.IsValid)
                {
                    var existing = db.SanPhams.Find(product.MASP);
                    if (existing == null) return HttpNotFound();

                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(imageFile.FileName);
                        string path = Path.Combine(Server.MapPath("~/Content/img/Products"), fileName);
                        imageFile.SaveAs(path);
                        existing.URL_ANH = "/Content/img/Products/" + fileName;
                    }
                    existing.TENSP = product.TENSP;
                    existing.MOTA = product.MOTA;
                    existing.GIA = product.GIA;
                    existing.SOLUONGTON = product.SOLUONGTON;
                    existing.MADM = product.MADM;
                    existing.TRANGTHAI = product.SOLUONGTON > 0 ? "Còn hàng" : "Hết hàng";
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật thành công!";
                    return RedirectToAction("Products");
                }
            }
            catch (Exception ex) { ModelState.AddModelError("", "Lỗi: " + ex.Message); }
            ViewBag.Categories = db.DanhMucs.ToList();
            return View(product);
        }

        [HttpPost]
        public ActionResult ProductDelete(int id)
        {
            if (!CheckAdminLogin()) return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            try
            {
                var product = db.SanPhams.Find(id);
                if (product == null) return Json(new { success = false, message = "Không tồn tại!" });
                db.SanPhams.Remove(product);
                db.SaveChanges();
                return Json(new { success = true, message = "Xóa thành công!" });
            }
            catch (Exception ex) { return Json(new { success = false, message = "Lỗi: " + ex.Message }); }
        }

        // --- QUẢN LÝ ĐƠN HÀNG ---
        public ActionResult Orders(string status = "", string search = "")
        {
            if (!CheckAdminLogin()) return RedirectToAction("Login");
            var orders = db.DonHangs.AsQueryable();
            if (!string.IsNullOrEmpty(status)) orders = orders.Where(o => o.TRANG_THAI == status);
            if (!string.IsNullOrEmpty(search))
                orders = orders.Where(o => o.HoTenNhanHang.Contains(search) || o.MADH.ToString().Contains(search));

            ViewBag.StatusList = GetOrderStatusList();
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;
            return View(orders.Include(o => o.NguoiDung).OrderByDescending(o => o.NGAYDAT).ToList());
        }

        public ActionResult OrderDetails(int id)
        {
            if (!CheckAdminLogin()) return RedirectToAction("Login");
            var order = db.DonHangs.Include(o => o.NguoiDung).Include(o => o.ChiTietDonHangs.Select(ct => ct.SanPham))
                          .FirstOrDefault(o => o.MADH == id);
            if (order == null) return HttpNotFound();
            ViewBag.Timeline = CreateOrderTimeline(order);
            return View(order);
        }

        [HttpPost]
        public ActionResult UpdateOrderStatus(int orderId, string status, string note = "")
        {
            if (!CheckAdminLogin()) return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            try
            {
                var order = db.DonHangs.Find(orderId);
                if (order == null) return Json(new { success = false, message = "Không tồn tại!" });

                order.TRANG_THAI = status;
                order.TrangThaiChiTiet = GetOrderStatusDetail(status);
                if (!string.IsNullOrEmpty(note)) order.GHI_CHU += $"\n[{DateTime.Now:dd/MM/yyyy HH:mm}] {note}";

                if (status == "Đã hủy")
                {
                    foreach (var detail in db.ChiTietDonHangs.Where(ct => ct.MADH == orderId).Include(ct => ct.SanPham))
                    {
                        if (detail.SanPham != null)
                        {
                            detail.SanPham.SOLUONGTON += detail.SOLUONG;
                            if (detail.SanPham.SOLUONGTON > 0) detail.SanPham.TRANGTHAI = "Còn hàng";
                        }
                    }
                }
                db.SaveChanges();
                return Json(new { success = true, message = "Cập nhật thành công!", status = status, statusDetail = order.TrangThaiChiTiet });
            }
            catch (Exception ex) { return Json(new { success = false, message = "Lỗi: " + ex.Message }); }
        }

        public ActionResult PrintInvoice(int id)
        {
            if (!CheckAdminLogin()) return RedirectToAction("Login");
            var order = db.DonHangs.Include(o => o.ChiTietDonHangs.Select(ct => ct.SanPham)).FirstOrDefault(o => o.MADH == id);
            return order == null ? (ActionResult)HttpNotFound() : View(order);
        }

        // --- QUẢN LÝ DANH MỤC ---
        public ActionResult Categories(string search = "")
        {
            if (!CheckAdminLogin()) return RedirectToAction("Login");
            var categories = db.DanhMucs.AsQueryable();
            if (!string.IsNullOrEmpty(search)) categories = categories.Where(c => c.TENDM.Contains(search));

            var model = categories.ToList().Select(c => new CategoryViewModel
            {
                MADM = c.MADM,
                TENDM = c.TENDM,
                ProductCount = db.SanPhams.Count(p => p.MADM == c.MADM)
            }).ToList();
            return View(model);
        }

        public ActionResult CategoryCreate()
        {
            if (!CheckAdminLogin()) return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CategoryCreate(string TENDM)
        {
            if (!CheckAdminLogin()) return RedirectToAction("Login");
            try
            {
                if (string.IsNullOrWhiteSpace(TENDM)) ModelState.AddModelError("TENDM", "Không được để trống");
                else if (db.DanhMucs.Any(c => c.TENDM.ToLower() == TENDM.Trim().ToLower())) ModelState.AddModelError("TENDM", "Đã tồn tại");
                else
                {
                    db.DanhMucs.Add(new DanhMuc { TENDM = TENDM.Trim() });
                    db.SaveChanges();
                    return RedirectToAction("Categories");
                }
            }
            catch (Exception ex) { ModelState.AddModelError("", ex.Message); }
            return View();
        }

        // --- QUẢN LÝ KHÁCH HÀNG ---
        public ActionResult Customers(string search = "", string role = "", string sortBy = "newest")
        {
            if (!CheckAdminLogin()) return RedirectToAction("Login");

            var query = db.NguoiDungs.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.HOTEN.Contains(search) || c.EMAIL.Contains(search) || c.SODIENTHOAI.Contains(search));
            if (!string.IsNullOrEmpty(role)) query = query.Where(c => c.VAITRO == role);

            switch (sortBy)
            {
                case "name_asc": query = query.OrderBy(c => c.HOTEN); break;
                case "name_desc": query = query.OrderByDescending(c => c.HOTEN); break;
                default: query = query.OrderByDescending(c => c.MAND); break;
            }

            var model = query.ToList().Select(c => new CustomerViewModel
            {
                MAND = c.MAND,
                HOTEN = c.HOTEN,
                EMAIL = c.EMAIL,
                SODIENTHOAI = c.SODIENTHOAI,
                VAITRO = c.VAITRO ?? "Customer",
                TotalOrders = c.DonHangs.Count,
                TotalSpent = c.DonHangs.Where(d => d.TRANG_THAI == "Hoàn thành").Sum(d => d.TongThanhToan) ?? 0
            }).ToList();

            ViewBag.Search = search;
            ViewBag.Role = role;
            return View(model);
        }

        public ActionResult CustomerDetails(int id)
        {
            if (!CheckAdminLogin()) return RedirectToAction("Login");
            var customer = db.NguoiDungs.Include(c => c.DonHangs).FirstOrDefault(c => c.MAND == id);
            if (customer == null) return HttpNotFound();

            var model = new CustomerDetailViewModel
            {
                Customer = customer,
                OrderStats = new CustomerOrderStats
                {
                    TotalOrders = customer.DonHangs.Count,
                    TotalSpent = customer.DonHangs.Where(d => d.TRANG_THAI == "Hoàn thành").Sum(d => d.TongThanhToan) ?? 0
                },
                RecentOrders = customer.DonHangs.OrderByDescending(d => d.NGAYDAT).Take(10).ToList()
            };
            return View(model);
        }

        // --- API & UTILITIES ---
        [HttpGet]
        public ActionResult GetCustomerStats()
        {
            if (!CheckAdminLogin()) return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            var stats = new { TotalCustomers = db.NguoiDungs.Count(), AdminCount = db.NguoiDungs.Count(c => c.VAITRO == "Admin") };
            return Json(new { success = true, data = stats }, JsonRequestBehavior.AllowGet);
        }

        // --- SUPPORT METHODS ---
        private List<MonthlyRevenue> GetMonthlyRevenue()
        {
            var revenue = new List<MonthlyRevenue>();
            var now = DateTime.Now;
            for (int i = 5; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
                var total = db.DonHangs.Where(d => d.NGAYDAT.Value.Month == date.Month && d.NGAYDAT.Value.Year == date.Year && d.TRANG_THAI == "Hoàn thành").Sum(d => d.TongThanhToan) ?? 0;
                revenue.Add(new MonthlyRevenue { Month = date.ToString("MM/yyyy"), Revenue = total });
            }
            return revenue;
        }

        private List<TopSellingProduct> GetTopSellingProducts()
        {
            return db.ChiTietDonHangs.GroupBy(ct => new { ct.MASP, ct.SanPham.TENSP })
                .Select(g => new TopSellingProduct { ProductId = g.Key.MASP, ProductName = g.Key.TENSP, QuantitySold = g.Sum(ct => ct.SOLUONG), Revenue = g.Sum(ct => ct.THANHTIEN * ct.SOLUONG) })
                .OrderByDescending(p => p.QuantitySold).Take(5).ToList();
        }

        private string GetOrderStatusDetail(string status)
        {
            switch (status)
            {
                case "Chờ xác nhận": return "Đang xử lý";
                case "Đã xác nhận": return "Đang đóng gói";
                case "Đang giao": return "Đang vận chuyển";
                case "Hoàn thành": return "Giao hàng thành công";
                default: return status;
            }
        }

        private List<SelectListItem> GetOrderStatusList()
        {
            return new List<string> { "", "Chờ xác nhận", "Đã xác nhận", "Đang giao", "Hoàn thành", "Đã hủy" }
                   .Select(s => new SelectListItem { Value = s, Text = s == "" ? "Tất cả" : s }).ToList();
        }

        private List<OrderTimelineItem> CreateOrderTimeline(DonHang order)
        {
            var list = new List<OrderTimelineItem>();
            var date = order.NGAYDAT ?? DateTime.Now;
            list.Add(new OrderTimelineItem { Title = "Đặt hàng", Date = date, IsCompleted = true, Icon = "📝" });
            list.Add(new OrderTimelineItem { Title = "Xác nhận", Date = date.AddHours(1), IsCompleted = order.TRANG_THAI != "Chờ xác nhận", Icon = "✅" });
            return list;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}

// =========================================================================
// PHẦN 4: VIEW MODELS 
// QUAN TRỌNG: CHỈ BỎ COMMENT NẾU BẠN CHƯA CÓ CÁC CLASS NÀY TRONG MODELS
// NẾU BỎ COMMENT MÀ BỊ LỖI "Type Already Defined", HÃY COMMENT LẠI
// =========================================================================


namespace OnlineMarket.Models 
{
    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<DonHang> NewOrders { get; set; }
        public List<SanPham> LowStockProducts { get; set; }
        public List<MonthlyRevenue> MonthlyRevenue { get; set; }
        public List<TopSellingProduct> TopSellingProducts { get; set; }
    }

    public class MonthlyRevenue { public string Month { get; set; } public decimal Revenue { get; set; } }
    public class TopSellingProduct { public int ProductId { get; set; } public string ProductName { get; set; } public int? QuantitySold { get; set; } public decimal? Revenue { get; set; } }
    public class OrderTimelineItem { public string Title { get; set; } public string Description { get; set; } public DateTime? Date { get; set; } public bool IsCompleted { get; set; } public bool IsCurrent { get; set; } public string Icon { get; set; } }
    public class CategoryViewModel { public int MADM { get; set; } public string TENDM { get; set; } public int ProductCount { get; set; } }
    public class CustomerViewModel { public int MAND { get; set; } public string TENDANGNHAP { get; set; } public string HOTEN { get; set; } public string EMAIL { get; set; } public string SODIENTHOAI { get; set; } public string DIACHI { get; set; } public string VAITRO { get; set; } public int TotalOrders { get; set; } public decimal TotalSpent { get; set; } public DateTime? LastOrderDate { get; set; } }
    public class CustomerDetailViewModel { public NguoiDung Customer { get; set; } public CustomerOrderStats OrderStats { get; set; } public List<DonHang> RecentOrders { get; set; } public List<DiaChiNguoiDung> ShippingAddresses { get; set; } public List<DanhGia> Reviews { get; set; } }
    public class CustomerOrderStats { public int TotalOrders { get; set; } public int CompletedOrders { get; set; } public int PendingOrders { get; set; } public int CancelledOrders { get; set; } public decimal TotalSpent { get; set; } public decimal AverageOrderValue { get; set; } }
    public class CustomerAddressViewModel { public bool IsPrimary { get; set; } public string HOTEN { get; set; } public string SODIENTHOAI { get; set; } public string DIACHI { get; set; } public string PHUONGXA { get; set; } public string QUANHUYEN { get; set; } public string TINHTHANH { get; set; } public string GIOITINH { get; set; } public string GHICHU { get; set; } public bool IsFromOrder { get; set; } public int? OrderId { get; set; } public DateTime? LastUsed { get; set; } }
}
