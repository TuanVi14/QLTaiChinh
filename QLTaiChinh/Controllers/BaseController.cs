using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QLTaiChinh.Data;

namespace QLTaiChinh.Controllers
{
    public class BaseController : Controller
    {
        protected readonly QuanLyTaiChinhCaNhanContext _db;

        public BaseController(QuanLyTaiChinhCaNhanContext context)
        {
            _db = context;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId != null)
            {
                var nguoiDung = _db.NguoiDungs
                    .FirstOrDefault(u => u.NguoiDungId == userId);
                ViewData["NguoiDung"] = nguoiDung;
            }
            base.OnActionExecuted(context);
        }
    }
}
