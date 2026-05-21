using OnlineMarket;
using OnlineMarket.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace OnlineMarket.Controllers
{
    public class AdminKhuyenMaiController : Controller
    {
        private QL_OnlineMarketEntities db = new QL_OnlineMarketEntities();

        // Kiểm tra đăng nhập Admin
        private bool CheckAdminLogin()
        {
            return Session["AdminLoggedIn"] != null && (bool)Session["AdminLoggedIn"];
        }

        // ========== DANH SÁCH KHUYẾN MÃI ==========

        // GET: /AdminKhuyenMai/
        public ActionResult Index(string status = "", string search = "")
        {
            if (!CheckAdminLogin())
            {
                return RedirectToAction("Login", "Account");
            }

            var promotions = db.KHUYENMAIs.AsQueryable();

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                promotions = promotions.Where(k => k.TRANGTHAI == status);
            }

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                promotions = promotions.Where(k =>
                    k.TENKM.Contains(search) ||
                    k.MOTA.Contains(search) ||
                    k.MAKM.ToString().Contains(search));
            }

            var model = promotions
                .OrderByDescending(k => k.NGAYBATDAU)
                .ToList();

            ViewBag.StatusList = GetStatusList();
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;

            return View(model);
        }

        // ========== TẠO KHUYẾN MÃI MỚI ==========

        // GET: /AdminKhuyenMai/Create
        public ActionResult Create()
        {
            if (!CheckAdminLogin())
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new KhuyenMaiCreateViewModel
            {
                NGAYBATDAU = DateTime.Now,
                NGAYKETTHUC = DateTime.Now.AddDays(30),
                TRANGTHAI = "Active",
                LOAI = "Percentage"
            };

            return View(model);
        }

        // POST: /AdminKhuyenMai/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KhuyenMaiCreateViewModel model)
        {
            if (!CheckAdminLogin())
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Kiểm tra mã khuyến mãi đã tồn tại chưa
                    var existingCode = db.KHUYENMAIs
                        .Any(k => k.TENKM == model.TENKM);

                    if (existingCode)
                    {
                        ModelState.AddModelError("TENKM", "Mã khuyến mãi đã tồn tại!");
                        return View(model);
                    }

                    // Kiểm tra ngày tháng
                    if (model.NGAYKETTHUC <= model.NGAYBATDAU)
                    {
                        ModelState.AddModelError("NGAYKETTHUC", "Ngày kết thúc phải sau ngày bắt đầu!");
                        return View(model);
                    }

                    // Tạo khuyến mãi mới
                    var khuyenMai = new KHUYENMAI
                    {
                        TENKM = model.TENKM,
                        MOTA = model.MOTA,
                        LOAI = model.LOAI,
                        PHANTRAMGIAM = model.LOAI == "Percentage" ? model.PHANTRAMGIAM : null,
                        GIATRIGIAM = model.LOAI == "Fixed" ? model.GIATRIGIAM : null,
                        GIATRITOIDA = model.GIATRITOIDA,
                        DIEUKIENAPDUNG = model.DIEUKIENAPDUNG,
                        SOLANSUDUNG = model.SOLANSUDUNG,
                        NGAYBATDAU = model.NGAYBATDAU,
                        NGAYKETTHUC = model.NGAYKETTHUC,
                        TRANGTHAI = model.TRANGTHAI,
                        SOLANDASUDUNG = 0
                    };

                    db.KHUYENMAIs.Add(khuyenMai);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Tạo khuyến mãi thành công!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
            }

            return View(model);
        }

        // ========== CHỈNH SỬA KHUYẾN MÃI ==========

        // GET: /AdminKhuyenMai/Edit/5
        public ActionResult Edit(int id)
        {
            if (!CheckAdminLogin())
            {
                return RedirectToAction("Login", "Account");
            }

            var khuyenMai = db.KHUYENMAIs.Find(id);
            if (khuyenMai == null)
            {
                return HttpNotFound();
            }

            var model = new KhuyenMaiEditViewModel
            {
                MAKM = khuyenMai.MAKM,
                TENKM = khuyenMai.TENKM,
                MOTA = khuyenMai.MOTA,
                LOAI = khuyenMai.LOAI ?? "Percentage", // Mặc định nếu null
                PHANTRAMGIAM = khuyenMai.PHANTRAMGIAM,
                GIATRIGIAM = khuyenMai.GIATRIGIAM,
                GIATRITOIDA = khuyenMai.GIATRITOIDA,
                DIEUKIENAPDUNG = khuyenMai.DIEUKIENAPDUNG,
                SOLANSUDUNG = khuyenMai.SOLANSUDUNG,
                NGAYBATDAU = khuyenMai.NGAYBATDAU,
                NGAYKETTHUC = khuyenMai.NGAYKETTHUC,
                TRANGTHAI = khuyenMai.TRANGTHAI ?? "Active" // Mặc định nếu null
            };

            return View(model);
        }

        // POST: /AdminKhuyenMai/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(KhuyenMaiEditViewModel model)
        {
            if (!CheckAdminLogin())
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var khuyenMai = db.KHUYENMAIs.Find(model.MAKM);
                    if (khuyenMai == null)
                    {
                        return HttpNotFound();
                    }

                    // Kiểm tra mã khuyến mãi đã tồn tại (trừ chính nó)
                    var existingCode = db.KHUYENMAIs
                        .Any(k => k.TENKM == model.TENKM && k.MAKM != model.MAKM);

                    if (existingCode)
                    {
                        ModelState.AddModelError("TENKM", "Mã khuyến mãi đã tồn tại!");
                        return View(model);
                    }

                    // Kiểm tra ngày tháng
                    if (model.NGAYKETTHUC <= model.NGAYBATDAU)
                    {
                        ModelState.AddModelError("NGAYKETTHUC", "Ngày kết thúc phải sau ngày bắt đầu!");
                        return View(model);
                    }

                    // Cập nhật thông tin
                    khuyenMai.TENKM = model.TENKM;
                    khuyenMai.MOTA = model.MOTA;
                    khuyenMai.LOAI = model.LOAI;
                    khuyenMai.PHANTRAMGIAM = model.LOAI == "Percentage" ? model.PHANTRAMGIAM : null;
                    khuyenMai.GIATRIGIAM = model.LOAI == "Fixed" ? model.GIATRIGIAM : null;
                    khuyenMai.GIATRITOIDA = model.GIATRITOIDA;
                    khuyenMai.DIEUKIENAPDUNG = model.DIEUKIENAPDUNG;
                    khuyenMai.SOLANSUDUNG = model.SOLANSUDUNG;
                    khuyenMai.NGAYBATDAU = model.NGAYBATDAU;
                    khuyenMai.NGAYKETTHUC = model.NGAYKETTHUC;
                    khuyenMai.TRANGTHAI = model.TRANGTHAI;

                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Cập nhật khuyến mãi thành công!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
            }

            return View(model);
        }

        // ========== XÓA KHUYẾN MÃI ==========

        // POST: /AdminKhuyenMai/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!CheckAdminLogin())
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            try
            {
                var khuyenMai = db.KHUYENMAIs.Find(id);
                if (khuyenMai == null)
                {
                    return Json(new { success = false, message = "Khuyến mãi không tồn tại!" });
                }

                // Kiểm tra xem khuyến mãi đã được sử dụng chưa
                var hasUsage = db.DonHangs.Any(d => d.MaKM == id);
                if (hasUsage)
                {
                    // Chỉ vô hiệu hóa thay vì xóa
                    khuyenMai.TRANGTHAI = "Inactive";
                    db.SaveChanges();
                    return Json(new { success = true, message = "Đã vô hiệu hóa khuyến mãi!" });
                }
                else
                {
                    // Xóa hoàn toàn nếu chưa được sử dụng
                    db.KHUYENMAIs.Remove(khuyenMai);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Xóa khuyến mãi thành công!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ========== TẠO MÃ TỰ ĐỘNG ==========

        // POST: /AdminKhuyenMai/GenerateCode
        [HttpPost]
        public ActionResult GenerateCode(string prefix = "VOUCHER")
        {
            if (!CheckAdminLogin())
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            try
            {
                var random = new Random();
                string code;
                bool exists;

                // Tạo mã không trùng
                do
                {
                    code = prefix + random.Next(1000, 9999).ToString();
                    exists = db.KHUYENMAIs.Any(k => k.TENKM == code);
                } while (exists);

                return Json(new { success = true, code = code });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ========== THỐNG KÊ SỬ DỤNG ==========

        // GET: /AdminKhuyenMai/Statistics/5
        public ActionResult Statistics(int id)
        {
            if (!CheckAdminLogin())
            {
                return RedirectToAction("Login", "Account");
            }

            var khuyenMai = db.KHUYENMAIs.Find(id);
            if (khuyenMai == null)
            {
                return HttpNotFound();
            }

            var model = new KhuyenMaiStatisticsViewModel
            {
                KhuyenMai = khuyenMai,
                TotalUsage = db.DonHangs.Count(d => d.MaKM == id),
                TotalRevenue = db.DonHangs
                    .Where(d => d.MaKM == id && d.TRANG_THAI == "Hoàn thành")
                    .Sum(d => d.TongThanhToan) ?? 0,
                UsageByMonth = GetUsageByMonth(id),
                RecentOrders = db.DonHangs
                    .Where(d => d.MaKM == id)
                    .Include(d => d.NguoiDung)
                    .OrderByDescending(d => d.NGAYDAT)
                    .Take(10)
                    .ToList()
            };

            return View(model);
        }

        // ========== IMPORT NHIỀU MÃ ==========

        // GET: /AdminKhuyenMai/Import
        public ActionResult Import()
        {
            if (!CheckAdminLogin())
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ImportKhuyenMaiViewModel
            {
                LOAI = "Percentage",
                SOLANSUDUNG = 1
            };

            return View(model);
        }

        // POST: /AdminKhuyenMai/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(ImportKhuyenMaiViewModel model)
        {
            if (!CheckAdminLogin())
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var maList = model.MaList.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    var createdCount = 0;
                    var errorMessages = new List<string>();

                    foreach (var ma in maList)
                    {
                        var maTrimmed = ma.Trim();
                        if (string.IsNullOrEmpty(maTrimmed))
                            continue;

                        // Kiểm tra mã đã tồn tại chưa
                        if (db.KHUYENMAIs.Any(k => k.TENKM == maTrimmed))
                        {
                            errorMessages.Add($"Mã '{maTrimmed}' đã tồn tại");
                            continue;
                        }

                        // Tạo khuyến mãi
                        var khuyenMai = new KHUYENMAI
                        {
                            TENKM = maTrimmed,
                            MOTA = "Mã khuyến mãi tự động",
                            LOAI = model.LOAI,
                            PHANTRAMGIAM = model.LOAI == "Percentage" ? model.PHANTRAMGIAM : null,
                            GIATRIGIAM = model.LOAI == "Fixed" ? model.GIATRIGIAM : null,
                            GIATRITOIDA = model.GIATRITOIDA,
                            DIEUKIENAPDUNG = model.DIEUKIENAPDUNG,
                            SOLANSUDUNG = model.SOLANSUDUNG,
                            NGAYBATDAU = DateTime.Now,
                            NGAYKETTHUC = DateTime.Now.AddDays(30),
                            TRANGTHAI = "Active",
                            SOLANDASUDUNG = 0
                        };

                        db.KHUYENMAIs.Add(khuyenMai);
                        createdCount++;
                    }

                    db.SaveChanges();

                    var message = $"Đã tạo thành công {createdCount} mã khuyến mãi.";
                    if (errorMessages.Any())
                    {
                        message += $"\nLỗi ({errorMessages.Count}): " + string.Join(", ", errorMessages.Take(5));
                        if (errorMessages.Count > 5)
                        {
                            message += $"... và {errorMessages.Count - 5} lỗi khác";
                        }
                    }

                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
            }

            return View(model);
        }

        // ========== PHƯƠNG THỨC HỖ TRỢ ==========

        private List<SelectListItem> GetStatusList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Tất cả" },
                new SelectListItem { Value = "Active", Text = "Active" },
                new SelectListItem { Value = "Inactive", Text = "Inactive" },
                new SelectListItem { Value = "Expired", Text = "Hết hạn" }
            };
        }

        private List<MonthlyUsage> GetUsageByMonth(int maKm)
        {
            var usage = new List<MonthlyUsage>();
            var now = DateTime.Now;

            for (int i = 5; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
                var monthName = date.ToString("MM/yyyy");

                var count = db.DonHangs
                    .Count(d => d.MaKM == maKm &&
                               d.NGAYDAT.HasValue &&
                               d.NGAYDAT.Value.Month == date.Month &&
                               d.NGAYDAT.Value.Year == date.Year);

                usage.Add(new MonthlyUsage
                {
                    Month = monthName,
                    UsageCount = count
                });
            }

            return usage;
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
}