using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineMarket.Models;

namespace OnlineMarket.Controllers
{
    public class BlogManageController : Controller
    {
        // 1. Danh sách tin
        public ActionResult Index()
        {
            var list = BlogRepository.GetAll().OrderByDescending(x => x.NgayDang).ToList();
            return View(list);
        }

        // 2. Tạo mới
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)] // Cho phép nhập HTML
        public ActionResult Create(BlogPost blog, HttpPostedFileBase uploadHinh)
        {
            // Xử lý ảnh
            if (uploadHinh != null && uploadHinh.ContentLength > 0)
            {
                string fileName = DateTime.Now.ToString("yyMMddss") + "_" + Path.GetFileName(uploadHinh.FileName);
                string path = Path.Combine(Server.MapPath("~/Content/img/blog/"), fileName);

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(Server.MapPath("~/Content/img/blog/")))
                    Directory.CreateDirectory(Server.MapPath("~/Content/img/blog/"));

                uploadHinh.SaveAs(path);
                blog.AnhBia = fileName;
            }
            else
            {
                blog.AnhBia = "default-blog.jpg";
            }

            var list = BlogRepository.GetAll();
            blog.Id = list.Any() ? list.Max(x => x.Id) + 1 : 1;
            blog.NgayDang = DateTime.Now;
            blog.LuotXem = 0;

            list.Add(blog);
            BlogRepository.Save(list);

            return RedirectToAction("Index");
        }

        // 3. Xóa
        public ActionResult Delete(int id)
        {
            var list = BlogRepository.GetAll();
            var item = list.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                list.Remove(item);
                BlogRepository.Save(list);
            }
            return RedirectToAction("Index");
        }
    }
}