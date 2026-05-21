using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization; // Cần tham chiếu System.Web.Extensions

namespace OnlineMarket.Models
{
    public class Banner
    {
        public int Id { get; set; }
        public string TenBanner { get; set; }
        public string HinhAnh { get; set; } // Tên file ảnh
        public string MoTa { get; set; }
        public int ThuTu { get; set; }
        public bool HienThi { get; set; }
    }

    // Helper xử lý lưu file JSON (Thay thế Database)
    public static class BannerRepository
    {
        private static string _filePath = HttpContext.Current.Server.MapPath("~/App_Data/banners.json");

        // Đọc danh sách
        public static List<Banner> GetAll()
        {
            if (!File.Exists(_filePath)) return new List<Banner>();
            string json = File.ReadAllText(_filePath);
            return new JavaScriptSerializer().Deserialize<List<Banner>>(json) ?? new List<Banner>();
        }

        // Lưu danh sách
        public static void Save(List<Banner> banners)
        {
            string json = new JavaScriptSerializer().Serialize(banners);
            File.WriteAllText(_filePath, json);
        }
    }
}