using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStoreOnline.Models;
using PagedList;

namespace BookStoreOnline.Controllers
{
    public class HomeController : Controller
    {
        BookStoreEntities db = new BookStoreEntities();
        public ActionResult Index(int? page)
        {
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View(db.Product.ToList().OrderBy(n=>n.Price).ToPagedList(pageNumber,pageSize));
        }

        
    }
}