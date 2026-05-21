using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineMarket.Models;

namespace OnlineMarket.Controllers
{
    public class HomeController : Controller
    {
        QL_OnlineMarketEntities SP = new QL_OnlineMarketEntities();
        public ActionResult Index()
        {
            List<SanPham> lst = SP.SanPhams.ToList();
            return View(lst);
        }
        public ActionResult Blog()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        // Action này sẽ đọc file JSON và trả về danh sách Banner
        public ActionResult BannerPartial()
        {
            var list = OnlineMarket.Models.BannerRepository.GetAll()
                        .Where(x => x.HienThi == true)
                        .OrderBy(x => x.ThuTu)
                        .ToList();
            return PartialView("_BannerPartial", list);
        }
    }
}