using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace OnlineMarket.Models
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string TieuDe { get; set; }
        public string AnhBia { get; set; }
        public string MoTaNgan { get; set; }
        public string NoiDung { get; set; }
        public string TacGia { get; set; }
        public string LoaiTin { get; set; } // health, nutrition, recipe...
        public DateTime NgayDang { get; set; }
        public int LuotXem { get; set; }
    }

    // Helper đọc/ghi file JSON (Giống hệt Banner)
    public static class BlogRepository
    {
        private static string _filePath = HttpContext.Current.Server.MapPath("~/App_Data/blogs.json");

        public static List<BlogPost> GetAll()
        {
            if (!File.Exists(_filePath)) return new List<BlogPost>();
            string json = File.ReadAllText(_filePath);
            return new JavaScriptSerializer().Deserialize<List<BlogPost>>(json) ?? new List<BlogPost>();
        }

        public static void Save(List<BlogPost> list)
        {
            string json = new JavaScriptSerializer().Serialize(list);
            File.WriteAllText(_filePath, json);
        }
    }
}