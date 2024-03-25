using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStoreOnline.Models;

namespace BookStoreOnline.Controllers
{
    public class CommentController : Controller
    {
        BookStoreEntities db = new BookStoreEntities();
        // GET: Comment
        public ActionResult Index(int id)
        {
            var comments = db.Comment.Where(c => c.ProductID == id).ToList();
            return View(comments);
        }

        [HttpPost]
        public ActionResult Insert(Comment comment)
        {
            var user = Session["TaiKhoan"] as User;
            if (user != null)
            {
                comment.UserID = user.UserID;
                comment.CommentDate = DateTime.Now;
                comment.IsActive = true;
               
                // Thêm comment vào cơ sở dữ liệu
                db.Comment.Add(comment);
                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

                // Kiểm tra xem comment đã được thêm thành công hay không
                if (comment.CommentID > 0)
                {
                    return RedirectToAction("Index", "ProductDetail", new { id = comment.ProductID });
                }
            }
            return RedirectToAction("Index", "ProductDetail", new { id = comment.ProductID });
        }
    }
}