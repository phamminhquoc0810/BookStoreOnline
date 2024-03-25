using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStoreOnline.Models;
using PagedList;

namespace BookStoreOnline.Areas.Admin.Controllers
{
    public class QLUsersController : Controller
    {
        BookStoreEntities db = new BookStoreEntities();
        // GET: Admin/QLUser
        public ActionResult Index(int? page, string search)
        {
            var allAccount = db.User.AsQueryable();
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            if (!string.IsNullOrEmpty(search))
            {
                // Nếu có giá trị tìm kiếm, lọc danh sách thể loại theo tên
                allAccount = allAccount.Where(c => c.UserName.Contains(search));
                ViewBag.Search = search;
            }
            return View(allAccount.OrderBy(c => c.UserID).ToPagedList(pageNumber, pageSize));
            
            
        }
    }
}