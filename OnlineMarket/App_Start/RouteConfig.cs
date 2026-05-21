using System.Web.Mvc;
using System.Web.Routing;

namespace OnlineMarket
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // THÊM DÒNG NÀY để bỏ qua favicon.ico
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

            // Route đặc biệt cho Admin với tên controller đầy đủ
            routes.MapRoute(
                name: "AdminRoute",
                url: "Admin/{action}/{id}",
                defaults: new { controller = "Admin", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "OnlineMarket.Controllers" }  // THÊM DÒNG NÀY
            );

            // Route mặc định - PHẢI ĐẶT CUỐI CÙNG
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "OnlineMarket.Controllers" }  // THÊM DÒNG NÀY
            );
        }
    }
}