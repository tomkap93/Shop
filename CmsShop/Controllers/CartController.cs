using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Cart;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            if (cart.Count == 0 || Session["cart"] == null)
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
            if (Session["cart"] != null)
            {
                //pobieranie wartości z sesji 
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }
                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                // ustawiamy ilość i cena na 0 
                qty = 0;
                price = 0M;
            }


            return PartialView(model);
        }


        public ActionResult AddToCartPartial(int id)
        {
            // Inicjalizacja CArt VM List
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //inicjalizacja CartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                // pobieramy product
                ProductDTO product = db.Products.Find(id);
                // sprawdzamy cz ten product jst już w koszyku
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);
                // w zależnośći od tego czy product jest w koszyku bedziemy wykonywać różne opcje 
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName


                    });
                }
                else
                {
                    productInCart.Quantity++;
                }
            }
            // pobieramy wartości ilości i ceny i dodajemy do modelu 
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Price * item.Quantity;
            }
            model.Quantity = qty;
            model.Price = price;
            // zapisać w sesji
            Session["cart"] = cart;


            return PartialView(model);
        }

        public JsonResult IncrementProduct(int productId)
        {
            // inicjalizacja listy cartVM 
            List<CartVM> cart = Session["cart"]as List<CartVM>;

            // pobieramy cartVM
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
            model.Quantity++;

            // przygotowanie danych do JSONa

            var result = new { qty = model.Quantity, price = model.Price };

            return Json(result,JsonRequestBehavior.AllowGet);
        }

        public JsonResult DecrementProduct(int productId)
        {
            // inicjalizacja listy cartVM 
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // pobieramy cartVM
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
            
            if (model.Quantity > 1)
            {
         
                model.Quantity--;
            }
            else
            {
                cart.Remove(model);
                model.Quantity = 0;
            }
    

            // przygotowanie danych do JSONa

            var result = new { qty = model.Quantity, price = model.Price };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public void RemoveProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // pobieramy cartVM
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

         // usuwamy product 
                cart.Remove(model);
           
           
        }

        public ActionResult PayPalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>; // zapisane w sesji


            return PartialView(cart);
        }
    }
}