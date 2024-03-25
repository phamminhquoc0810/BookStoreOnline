using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStoreOnline.Models;
using System.Web.Script.Serialization;
using BookStoreOnline.Common;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Web.Helpers;
using System.Security.Policy;
using System.IO;
using PayPal.Api;

namespace BookStoreOnline.Controllers
{
    public class CartController : Controller
    {
        BookStoreEntities db = new BookStoreEntities();
        // GET: Cart
        public ActionResult Index()
        {
            return View();
        }

        public Cart GetCart()
        {
            Cart cart = Session["GioHang"] as Cart;


            if (cart == null || Session["GioHang"] == null)
            {
                cart = new Cart();
                Session["GioHang"] = cart;
            }
            return cart;
        }

        public ActionResult AddToCart(int id)
        {
            Cart cart = GetCart();
            var product = db.Product.SingleOrDefault(p => p.ProductID == id);

            if (product != null)
            {
                cart.Add(product);
            }
            return RedirectToAction("Index", "ProductDetail");
        }
        private int GetTotalNumber()
        {
            int _t_item = 0;
            Cart cart = Session["GioHang"] as Cart;
            if (cart != null)
                _t_item = cart.Total_Quantity();
            return _t_item;
        }

        public ActionResult AddSingleProduct(int id)
        {
            Cart cart = GetCart();
            var product = db.Product.SingleOrDefault(p => p.ProductID == id);

            if (product != null)
            {
                cart.Add(product);
            }
            return RedirectToAction("GetCartInfo", "Cart");
        }

        public ActionResult RemoveCart(int id)
        {
            Cart cart = Session["GioHang"] as Cart;
            cart.Remove_CartItem(id);
            return RedirectToAction("GetCartInfo", "Cart");
        }

        //private decimal GetTotalPrice()
        //{
        //    decimal totalPrice = 0;
        //    Cart cart = GetCart();

        //    if (cart != null)
        //    {
        //        totalPrice = cart.TotalMoney();
        //    }

        //    return totalPrice;
        //}

        

        public ActionResult GetCartInfo()
        {
            if (Session["GioHang"] == null)
                return RedirectToAction("NullCart", "Cart");
            Cart cart = Session["GioHang"] as Cart;
            List<Cart> cartList = new List<Cart> { cart };
            return View(cartList);
        }
       
        public ActionResult Update_Quantity_Cart(FormCollection product)
        {
            Cart cart = GetCart();
            int id_pro = int.Parse(product["ProductID"]);
            
                int quantity = int.Parse(product["Quantity"]);
                cart.Update_Quantity_Shopping(id_pro, quantity);
                
            
            return RedirectToAction("GetCartInfo", "Cart");
        }

        public ActionResult CartPartial()
        {
            ViewBag.TotalNumber = GetTotalNumber();

            return PartialView();
        }

        public ActionResult NullCart()
        {
            return View();
        }

        public ActionResult InsertOrder(string address, string email, string phone, string paymentMethod)
        {
            Models.Order order = new Models.Order();
            
            order.DateOrder = DateTime.Now.Date;
            order.Address = address;
            order.PaymentMethod = paymentMethod;
            order.Status = 0;
            order.Phone = phone;
            order.Email = email;
            try
            {
                
                var user = (User)Session["TaiKhoan"];

                order.UserID = user.UserID;
                db.Order.Add(order);
                db.SaveChanges();


                Cart cart = GetCart();
                decimal? total = 0;
                foreach (var item in cart.Items)
                {
                    OrderDetail prod = new OrderDetail();
                    prod.IDOrder = order.IDOrder;
                    prod.ProductID = item.store_product.ProductID;
                    prod.Quantity = item.store_quantity;
                    prod.UnitPrice = item.store_product.Price;

                    db.OrderDetail.Add(prod);
                    db.SaveChanges();
                    total += prod.UnitPrice * item.store_quantity;
                }
                ViewBag.Total = total;

                
                // Gửi email thông báo đặt hàng thành công cho khách hàng
                string customerContent = System.IO.File.ReadAllText(Server.MapPath("~/Content/template_Mail/sendCus.html"));
                customerContent = customerContent.Replace("{{UserID}}", user.UserID.ToString());
                customerContent = customerContent.Replace("{{UserName}}", user.UserName);
                customerContent = customerContent.Replace("{{Phone}}", phone);
                customerContent = customerContent.Replace("{{Email}}", email);
                customerContent = customerContent.Replace("{{Address}}", address);
                customerContent = customerContent.Replace("{{Total}}", ViewBag.Total.ToString("N0"));
                new MailHelper().SendMail(email, "Đơn hàng mới từ BookStore Online", customerContent);

                

                // Gửi email thông báo đặt hàng cho admin
                string adminContent = System.IO.File.ReadAllText(Server.MapPath("~/Content/template_Mail/sendAd.html"));
                adminContent = adminContent.Replace("{{UserID}}", user.UserID.ToString());
                adminContent = adminContent.Replace("{{UserName}}", user.UserName);
                adminContent = adminContent.Replace("{{Phone}}", phone);
                adminContent = adminContent.Replace("{{Email}}", email);
                adminContent = adminContent.Replace("{{Address}}", address);
                adminContent = adminContent.Replace("{{Total}}", ViewBag.Total.ToString("N0"));
                var adminEmail = ConfigurationManager.AppSettings["EmailAdmin"].ToString();
                new MailHelper().SendMail(adminEmail, "Thông báo đơn hàng mới từ BookStore", adminContent);

                Session["GioHang"] = null;
                return RedirectToAction("OrderSuccess","Cart");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đặt hàng thất bại: " + ex.ToString();
                return RedirectToAction("OrderFailure","Cart");
            }

            
        }
        public ActionResult OrderSuccess()
        {
            return View();
        }

        public ActionResult OrderFailure()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"]?.ToString();

            // Lấy đối tượng Exception từ TempData (nếu có)
            Exception exception = TempData["Exception"] as Exception;
            // Log thông tin Exception
            System.Diagnostics.Debug.WriteLine($"Exception in OrderFailure: {exception?.Message}");


            // Truyền đối tượng Exception vào view để hiển thị chi tiết
            return View(exception);
        }

        private void UpdateOrderStatus(string orderId, string paymentMethod)
        {
            var order = db.Order.FirstOrDefault(o => o.IDOrder.ToString() == orderId);
            if (order != null)
            {
                order.Status = 1; // Đã thanh toán
                order.PaymentMethod = paymentMethod; // Đặt giá trị phương thức thanh toán
                db.SaveChanges();
                Session["GioHang"] = null;
            }
        }
        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            // Lấy apiContext  
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                // A resource representing a Payer that funds a payment Payment Method as paypal  
                // Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    // this section will be executed first because PayerID doesn't exist  
                    // it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Cart/PaymentWithPayPal?";
                    // here we are generating guid for storing the paymentID received in session  
                    // which will be used in the payment execution  
                    var guid = Convert.ToString((new Random()).Next(100000));
                    // CreatePayment function gives us the payment approval url  
                    // on which payer is redirected for PayPal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);

                    // get links returned from PayPal in response to Create function call  
                    var approvalUrl = createdPayment.links.FirstOrDefault(link => link.rel.ToLower().Trim().Equals("approval_url"))?.href;

                    if (approvalUrl != null)
                    {
                        // Saving the paymentID in the session
                        Session[guid] = createdPayment.id;

                        return Redirect(approvalUrl);
                    }
                }
                else
                {
                    // This function executes after receiving all parameters for the payment  
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session["PaypalPaymentId"] as string);
                    // If executed payment failed then we will show a payment failure message to the user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("OrderFailure");
                    }
                    else
                    {
                        var orderId = Session[guid] as string;
                        UpdateOrderStatus(orderId, "PayPal");
                        SaveOrderDetails(orderId);
                        return RedirectToAction("OrderSuccess");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PaymentWithPayPal: {ex.Message}");
                return View("OrderFailure");
            }
            // On successful payment, show the success page to the user.  
            return View("OrderSuccess");
        }

        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            var executedPayment = this.payment.Execute(apiContext, paymentExecution);

            // Kiểm tra trạng thái thanh toán sau khi được xác nhận
            if (executedPayment.state == "approved")
            {
                var orderId = Session[paymentId] as string;
                UpdateOrderStatus(orderId, "PayPal");
                SaveOrderDetails(orderId);
            }
            Session["GioHang"] = null;
            return executedPayment;
        }
        private void SaveOrderDetails(string orderId)
        {
            // Retrieve the order from the database based on orderId
            var order = db.Order.FirstOrDefault(o => o.IDOrder.ToString() == orderId);
            if (order != null)
            {
                
                Cart cart = Session["GioHang"] as Cart;

                if (cart != null)
                {
                    
                    foreach (var item in cart.Items)
                    {
                        OrderDetail prod = new OrderDetail
                        {
                            IDOrder = order.IDOrder,
                            ProductID = item.store_product.ProductID,
                            Quantity = item.store_quantity,
                            UnitPrice = item.store_product.Price
                        };

                        db.OrderDetail.Add(prod);
                    }
                    db.SaveChanges();
                }
            }
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            try
            {
                var listSanpham = Session["GioHang"] as Cart;

                // Create a orderId consistently to use throughout the process
                var paypalOrderId = Guid.NewGuid().ToString("N");

                var itemList = new ItemList()
                {
                    items = new List<Item>()
                };

                foreach (var item in listSanpham.Items)
                {
                    itemList.items.Add(new Item()
                    {
                        name = item.store_product.ProductName,
                        currency = "USD",
                        price = Math.Round(item.store_product.Price ?? 0m / 24500, 2).ToString(),
                        quantity = item.store_quantity.ToString(),
                        sku = item.store_product.ProductID.ToString()
                    });
                }

                var payer = new Payer()
                {
                    payment_method = "paypal"
                };

                var redirUrls = new RedirectUrls()
                {
                    cancel_url = redirectUrl + "&Cancel=true",
                    return_url = redirectUrl
                };

                var details = new Details()
                {
                    tax = "0",
                    shipping = "0",
                    subtotal = listSanpham.Total_Money().ToString(),
                };

                var amount = new Amount()
                {
                    currency = "USD",
                    total = listSanpham.Total_Money().ToString(),
                    details = details
                };
                var transactionList = new List<Transaction>();

                transactionList.Add(new Transaction()
                {
                    description = $"Invoice #{paypalOrderId}",
                    invoice_number = paypalOrderId, // Use a consistent orderId
                    amount = amount,
                    item_list = itemList
                });

                this.payment = new Payment()
                {
                    intent = "sale",
                    payer = payer,
                    transactions = transactionList,
                    redirect_urls = redirUrls
                };

                var createdPayment = this.payment.Create(apiContext);

                // Save paymentId to session
                Session["PaypalPaymentId"] = createdPayment.id;

                // Save orderId to session for use in subsequent steps
                Session[paypalOrderId] = createdPayment.id;

                // Save payment information to the order
                UpdateOrderStatus(paypalOrderId, "PayPal");

                return createdPayment;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CreatePayment: {ex.Message}");
                return null;
            }
        }

    }
}