using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStoreOnline.Models;
using System.Security.Cryptography;
using System.Web.Helpers;

namespace BookStoreOnline.Controllers
{
    public class UserController : Controller
    {
        BookStoreEntities db = new BookStoreEntities();
        // GET: User
       
        public ActionResult Index()
        {
            
            return View();
        }
        [HttpGet]
       

        
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(User taikhoan)
        {
            if (ModelState.IsValid)
            {
               
                var user = db.User.FirstOrDefault(k => k.Email == taikhoan.Email);

                if (user != null)
                {
                    
                    if (Crypto.VerifyHashedPassword(user.Password, taikhoan.Password))
                    {
                        if (user.Role != null && user.Role.RoleName == "Admin")
                        {
                            Session["TaiKhoan"] = user;
                            return RedirectToAction("Index", "Admin/ThongKe");
                        }
                        else
                        {
                            // For regular users, redirect to the home page
                            Session["TaiKhoan"] = user;
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                // If the email or password is incorrect, display an error message
                ViewBag.ThongBao = "Email hoặc mật khẩu không chính xác";
            }

            return View();
        }
        public ActionResult LogOut()
        {
            Session["TaiKhoan"] = null;
            Session["GioHang"] = null;

            return RedirectToAction("Login", "User");
        }

        [HttpGet]
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(User cus,string rePass)
        {
            if (ModelState.IsValid)
            {
                
                var checkEmail = db.User.FirstOrDefault(c => c.Email == cus.Email);
                if (checkEmail != null)
                {
                    ViewBag.ThongBaoEmail = "Đã có tài khoản đăng nhập bằng Email này";
                    return View();
                }
                if(cus.Password == rePass)
                {
                    cus.Password = Crypto.HashPassword(cus.Password);
                    cus.RoleID = 2;
                    db.User.Add(cus);
                    db.SaveChanges();
                    return RedirectToAction("Login","User");
                }
                else
                {
                    ViewBag.ThongBao = "Mật khẩu xác nhận không chính xác";
                    return View();
                }
            }
            return View();
        }
        [HttpGet]
        public ActionResult Profile()
        {
            if (Session["TaiKhoan"] != null)
            {
                User loggedInUser = (User)Session["TaiKhoan"];
                return View(loggedInUser);
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            // Ensure the user is logged in
            if (Session["TaiKhoan"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
        {
            if (ModelState.IsValid)
            {
                // Get the logged-in user from the session
                User loggedInUser = (User)Session["TaiKhoan"];

                // Retrieve the user from the database based on the logged-in user's ID
                var userDb = db.User.FirstOrDefault(u => u.UserID == loggedInUser.UserID);

                // Check if the old password matches the stored hashed password
                if (userDb != null && userDb.Password != null && Crypto.VerifyHashedPassword(userDb.Password, oldPassword))
                {
                    // Check if the new password matches the confirmation
                    if (newPassword == confirmNewPassword)
                    {
                        // Update the password in the database
                        userDb.Password = Crypto.HashPassword(newPassword);
                        db.SaveChanges();

                        ViewBag.ChangePasswordSuccess = "Thay đổi mật khẩu thành công!";
                        return View();
                    }
                    else
                    {
                        ViewBag.ChangePasswordError = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                    }
                }
                else
                {
                    ViewBag.ChangePasswordError = "Mật khẩu cũ không chính xác.";
                }
            }


            return View();
        }


    }
}