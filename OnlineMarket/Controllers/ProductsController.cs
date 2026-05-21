using OnlineMarket;
using System;
using System.Linq;
using System.Web.Mvc;

public class ProductsController : Controller
{
    private QL_OnlineMarketEntities db = new QL_OnlineMarketEntities();

    [HttpGet]
    public JsonResult GetAll()
    {
        var data = db.SanPhams.Select(p => new
        {
            id = p.MASP,
            ten = p.TENSP,
            gia = p.GIA,
            soluong = p.SOLUONGTON
        }).ToList();

        return Json(data, JsonRequestBehavior.AllowGet);
    }
 
    [HttpPost]
    [AllowAnonymous]
    public JsonResult Create(
    string ten,
    decimal gia,
    int soluong,
    string mota,
    string hinhanh,
    string donvitinh,
    string trangthai
)
    {
        try
        {
          
            if (string.IsNullOrEmpty(ten))
                return Json(new { success = false, message = "Tên không được để trống" });

            if (gia <= 0)
                return Json(new { success = false, message = "Giá phải > 0" });

            if (soluong < 0)
                return Json(new { success = false, message = "Số lượng không hợp lệ" });

            var sp = new SanPham
            {
                TENSP = ten,
                GIA = gia,
                SOLUONGTON = soluong,
                MOTA = mota,
                 URL_ANH = hinhanh,
                DONVITINH = donvitinh,
                NGAYTHEM = DateTime.Now,
                TRANGTHAI = trangthai
            };

            db.SanPhams.Add(sp);
            db.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Thêm sản phẩm thành công",
                id = sp.MASP
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = "Lỗi hệ thống",
                error = ex.Message
            });
        }
    }
    [HttpPost]
    public JsonResult Update(int id, string ten, decimal gia, int soluong)
    {
        try
        {
            var sp = db.SanPhams.Find(id);

            if (sp == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            if (string.IsNullOrEmpty(ten) || gia <= 0 || soluong < 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            sp.TENSP = ten;
            sp.GIA = gia;
            sp.SOLUONGTON = soluong;

            db.SaveChanges();

            return Json(new { success = true, message = "Cập nhật thành công" });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                error = ex.Message
            });
        }
    }
    [HttpPost]
    public JsonResult Delete(int id)
    {
        try
        {
            var sp = db.SanPhams.Find(id);

            if (sp == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            db.SanPhams.Remove(sp);
            db.SaveChanges();

            return Json(new { success = true, message = "Xóa thành công" });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                error = ex.Message
            });
        }
    }
    // chi tiết sản phẩm
    [HttpGet]
    public JsonResult GetById(int id)
    {
        var sp = db.SanPhams.Find(id);

        if (sp == null)
        {
            return Json(new { success = false, message = "Không tìm thấy" }, JsonRequestBehavior.AllowGet);
        }

        return Json(new
        {
            id = sp.MASP,
            ten = sp.TENSP,
            gia = sp.GIA,
            soluong = sp.SOLUONGTON,
            hinhanh = sp.URL_ANH,
            mota = sp.MOTA,
        }, JsonRequestBehavior.AllowGet);
    }

    // tìm kiếm sản phẩm
    [HttpGet]
    public JsonResult Search(string keyword)
    {
        var data = db.SanPhams
            .Where(p => p.TENSP.Contains(keyword))
            .Select(p => new {
                id = p.MASP,
                ten = p.TENSP,
                gia = p.GIA
            }).ToList();

        return Json(data, JsonRequestBehavior.AllowGet);
    }
}