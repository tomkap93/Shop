using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

        [HttpPost]
        public void PlaceOrder()
        {
            // ppobieramy zawartosc koszyka z sesi
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // pobranie nazwy uzytkownika
            string username = User.Identity.Name;

            // deklarujemy numer zamowienia
            int orderId = 0;

            using (Db db = new Db())
            {
                // inicjalizacja OrderDTO
                OrderDTO oredrDTO = new OrderDTO();

                // pobieramu user id
                var user1 = db.Users.FirstOrDefault(x => x.UserName == username);
                int userId = user1.Id;

                // ustawienie orderDTO i zapis 
                oredrDTO.UserId = userId;
                oredrDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(oredrDTO);
                db.SaveChanges();

                // pobieramy id zapisanego zamowienia
                orderId = oredrDTO.OrderId;

                // inicjalizacja OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);
                    db.SaveChanges();
                }
            }
            UserDTO user;
            using (Db db = new Db())
            {

               user = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();

            }

                // wysylanie emaila do admina
                var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("af6b8e39b53e5a", "04bc816627feb2"),
                EnableSsl = true
            };
            client.Send("SklepUTP@example.com", "admin@example.com", "Nowe zamowienie nr:" + orderId, "Dziękujemy za dokonanie zakupu w naszej platformie \n Zamówienie nr : "+ orderId + " czeka na zrealizowanie " + orderId);


            var client1 = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("af6b8e39b53e5a", "04bc816627feb2"),
                EnableSsl = true
            };
            client1.Send("SklepUTP@example.com",user.EmailAddress, "Nowe zamowienie nr:"+orderId, "Dziękujemy za dokonanie zakupu w naszej platformie \n Zamówienie nr : " + orderId + " czeka na zrealizowanie " + orderId);


            // reset session
            Session["cart"] = null;
        }
    }
}