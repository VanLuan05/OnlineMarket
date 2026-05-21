using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineMarket.Models;

namespace OnlineMarket.Controllers
{
    public class BannerManageController : Controller
    {
        // 1. Hiện danh sách Banner
        public ActionResult Index()
        {
            var list = BannerRepository.GetAll().OrderBy(x => x.ThuTu).ToList();
            return View(list);
        }

        // 2. Tạo mới - Giao diện
        public ActionResult Create()
        {
            return View();
        }

        // 2. Tạo mới - Xử lý
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Banner banner, HttpPostedFileBase uploadHinh)
        {
            if (uploadHinh != null && uploadHinh.ContentLength > 0)
            {
                // Lưu ảnh vào thư mục
                string fileName = DateTime.Now.ToString("yyMMddss") + "_" + Path.GetFileName(uploadHinh.FileName);
                string path = Path.Combine(Server.MapPath("~/Content/img/"), fileName);
                uploadHinh.SaveAs(path);
                banner.HinhAnh = fileName;
            }
            else
            {
                banner.HinhAnh = "default.jpg";
            }

            var list = BannerRepository.GetAll();
            banner.Id = list.Any() ? list.Max(x => x.Id) + 1 : 1; // Tự tăng ID
            banner.HienThi = true;

            list.Add(banner);
            BannerRepository.Save(list);

            return RedirectToAction("Index");
        }

        // 3. Sửa - Giao diện
        public ActionResult Edit(int id)
        {
            var banner = BannerRepository.GetAll().FirstOrDefault(x => x.Id == id);
            if (banner == null) return HttpNotFound();
            return View(banner);
        }

        // 3. Sửa - Xử lý
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Banner banner, HttpPostedFileBase uploadHinh)
        {
            var list = BannerRepository.GetAll();
            var item = list.FirstOrDefault(x => x.Id == banner.Id);

            if (item != null)
            {
                item.TenBanner = banner.TenBanner;
                item.MoTa = banner.MoTa;
                item.ThuTu = banner.ThuTu;
                item.HienThi = banner.HienThi;

                // Nếu có up ảnh mới thì thay thế
                if (uploadHinh != null && uploadHinh.ContentLength > 0)
                {
                    string fileName = DateTime.Now.ToString("yyMMddss") + "_" + Path.GetFileName(uploadHinh.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/img/"), fileName);
                    uploadHinh.SaveAs(path);
                    item.HinhAnh = fileName;
                }

                BannerRepository.Save(list);
                return RedirectToAction("Index");
            }
            return View(banner);
        }

        // 4. Xóa
        public ActionResult Delete(int id)
        {
            var list = BannerRepository.GetAll();
            var item = list.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                // Xóa file ảnh cũ nếu cần (Optional)
                // string path = Server.MapPath("~/Content/img/" + item.HinhAnh);
                // if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

                list.Remove(item);
                BannerRepository.Save(list);
            }
            return RedirectToAction("Index");
        }
    }
}