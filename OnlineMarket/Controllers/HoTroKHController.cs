using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineMarket.Controllers
{
    public class HoTroKHController : Controller
    {
        // GET: HoTroKH/Index (Trang tổng hợp dịch vụ)
        public ActionResult Index()
        {
            return View();
        }

        // GET: HoTroKH/Chatbox (Trang chatbox riêng)
        public ActionResult Chatbox()
        {
            return View();
        }

        // GET: HoTroKH/Hotline
        public ActionResult Hotline()
        {
            return View();
        }

        // GET: HoTroKH/GuiYeuCau
        public ActionResult GuiYeuCau()
        {
            return View();
        }

        // GET: HoTroKH/ChinhSachDoiTra
        public ActionResult ChinhSachDoiTra()
        {
            return View();
        }

        // GET: HoTroKH/ChinhSachHoanTien
        public ActionResult ChinhSachHoanTien()
        {
            return View();
        }
    }
}