using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStoreOnline.Models;

namespace BookStoreOnline.Controllers
{
    public class OrderController : Controller
    {
        BookStoreEntities db = new BookStoreEntities();
        // GET: Order
        public ActionResult Index(int id)
        {
            return View(db.Order.Where(o => o.UserID == id).ToList());
        }

        
    }
}