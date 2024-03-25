using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.IO;
using System.Web.Mvc;
using BookStoreOnline.Models;
using PagedList;

namespace BookStoreOnline.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private BookStoreEntities db = new BookStoreEntities();

        // GET: Admin/Products
        public ActionResult Index(int? page, string search)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var products = db.Product.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                // Nếu có giá trị tìm kiếm, lọc danh sách sản phẩm theo tên sản phẩm
                var searchTerms = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                products = products.Where(p => searchTerms.Any(term => p.ProductName.Contains(term)));
                ViewBag.Search = search;
            }
            products = products.OrderBy(p => p.ProductID);
            
            return View(products.ToPagedList(pageNumber,pageSize));
        }

        // GET: Admin/Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Product product, HttpPostedFileBase imageBook)
        {
            if (ModelState.IsValid)
            {
                if (imageBook != null)
                {
                    string fileName = Path.GetFileName(imageBook.FileName);
                    string path = Path.Combine(Server.MapPath("~/Images"), fileName);
                    product.ImageProd = fileName;
                    imageBook.SaveAs(path);
                }

                db.Product.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

           ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,Price,Introduce,Author,ImageProd,CategoryID")] Product product, HttpPostedFileBase imageBook)
        {
            if (ModelState.IsValid)
            {
                if (imageBook != null)
                {
                    var fileName = Path.GetFileName(imageBook.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images"), fileName);
                    product.ImageProd = fileName;
                    imageBook.SaveAs(path);
                }

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Product.Find(id);
            db.Product.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
