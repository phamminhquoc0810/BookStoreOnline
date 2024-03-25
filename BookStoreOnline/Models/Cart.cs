using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BookStoreOnline.Models;
using PayPal.Api;

namespace BookStoreOnline.Models
{
    public class CartItem
    {
        public Product store_product { get; set; }
        public int store_quantity { get; set; }
    }
    public class Cart
    {
       List<CartItem> items = new List<CartItem>();
        public IEnumerable<CartItem> Items { get { return items; } }
        public void Add(Product _pro, int _quantity = 1)
        {
            var item = items.FirstOrDefault(s =>s.store_product.ProductID == _pro.ProductID);
            if(item == null)
            {
                items.Add(new CartItem
                {
                    store_product = _pro,
                    store_quantity = _quantity
                });
            }
            else
            {
                item.store_quantity += _quantity;
            }
        }
        public void Update_Quantity_Shopping(int id, int _quantity)
        {
            var item = items.Find(s => s.store_product.ProductID == id);
            if (item != null)
            {
                
                if (_quantity >= 0)
                {
                    item.store_quantity = _quantity;
                }
                else
                {
                   
                }
            }
        }
        public double Total_Money()
        {
            var total = items.Sum(s => s.store_product.Price * s.store_quantity);
            return (double)total;
        }
        public void Remove_CartItem(int id)
        {
            items.RemoveAll(s => s.store_product.ProductID == id);
        }
        public int Total_Quantity()
        {
            return items.Sum(s => s.store_quantity);
        }
        public void ClearCart()
        {
            items.Clear();
        }
    }
}
