using System.Linq;
using System.Web.Mvc;
using OnlineMarket.Models;

namespace OnlineMarket.Controllers
{
    public class BlogController : Controller
    {
        // GET: Blog
        public ActionResult Index()
        {
            // Lấy dữ liệu từ file JSON
            var list = BlogRepository.GetAll().OrderByDescending(x => x.NgayDang).ToList();
            return View(list);
        }

        // Xem chi tiết
        public ActionResult Details(int id)
        {
            var list = BlogRepository.GetAll();
            var blog = list.FirstOrDefault(x => x.Id == id);

            if (blog == null) return HttpNotFound();

            // Tăng lượt xem và lưu lại vào JSON
            blog.LuotXem++;
            BlogRepository.Save(list);

            return View(blog);
        }
    }
}