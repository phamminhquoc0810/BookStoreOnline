using BookStoreOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookStoreOnline.Areas.Admin.Controllers
{
    public class ThongKeController : Controller
    {
        BookStoreEntities db = new BookStoreEntities();
        // GET: Admin/ThongKe
        public ActionResult Index()
        {
            ViewBag.numOnline = HttpContext.Application["SoNguoiOnline"].ToString();
            ViewBag.numOrder = ThongKeOrder();
            ViewBag.numUser = ThongKeUser();
            ViewBag.numProduct = ThongKeProduct();
            ViewBag.TongDoanhThu = ThongKeTongDoanhThu();
            return View();
        }        

        public double ThongKeUser()
        {
            double numUser = db.User.Count();
            return numUser;
        }
        public double ThongKeOrder()
        {
            double slddh = db.Order.Count();
            return slddh;
        }

        public double ThongKeProduct()
        {
            double slproduct = db.Product.Count();
            return slproduct;
        }

        public decimal ThongKeTongDoanhThu()
        {
            decimal TongDoanhThu = 0;
            if (db.OrderDetail.Any())
            {

                TongDoanhThu = db.OrderDetail.Sum(n => n.Quantity * n.UnitPrice) ?? 0;
            }
            return TongDoanhThu;
        }

        //public decimal ThongKeTongDoanhThuThang(int thang, int nam)
        //{
        //    var lstOrder = db.Order.Where(n => n.DateOrder.Value.Month == thang && n.DateOrder.Value.Year == nam);
        //    decimal TongTien = 0;
        //    foreach(var item in lstOrder)
        //    {
        //        TongTien += decimal.Parse(item.OrderDetail.Sum(n => n.Quantity * n.UnitPrice).Value.ToString());
        //    }
        //    return TongTien;
        //}

    }
}