using CmsShop.Models.ViewModels.Cart;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            if (cart.Count==0||Session["cart"]==null)
            {
                ViewBag.Message = "Twój koszyk jest pusty";
                return View();
            }

            // Obliczenie wartości podusuwującej koszyka 
            // i przekazanie do viewbag
            decimal total = 0M;
            foreach (var item in cart)
            {
                total += item.Total;
            }
            ViewBag.GrandTotal = total;
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //inicjalizacja cart VM
            CartVM model = new CartVM();
            // inicjalizacja ilość i cena
            int qty = 0;
            decimal price = 0M;

            //sprawdzamy czy mamy dane koszyka zapisane w sesi
            if (Session["cart"]!=null)
            {
                //pobieranie wartości z sesji 
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity*item.Price;
                }
            }
            else
            {
                // ustawiamy ilość i cena na 0 
                qty = 0;
                price = 0M;
            }


            return PartialView(model);
        }
    }
}