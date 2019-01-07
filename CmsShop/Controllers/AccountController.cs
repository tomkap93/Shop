﻿using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CmsShop.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        // GET: /account/login
        [HttpGet]
        public ActionResult Login()
        {
            // sprawdzanie czy uzytkownik nie jest juz zalogowany
            string userName = User.Identity.Name;
            if (!string.IsNullOrEmpty(userName))
                return RedirectToAction("user-profile");

            // zwracamy widok
            return View();
        }

        // POST: /account/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // sprawdzenie model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // sprawdzamy uzytkownika
            bool isValid = false;
            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.UserName.Equals(model.UserName) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if (!isValid)
            {
                ModelState.AddModelError("", "Nieprawidłowa nazwa użytkownika lub hasło");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.UserName, model.RememberMe));
            }
        }

        // GET: /account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {

            return View("CreateAccount");
        }

        // POST: /account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            // sprawdzenie model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            // sprawdzenie hasła
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Hasła nie pasują do siebie");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                // sprawdzenie czy nazwa uzytkownika jest unikalna
                if (db.Users.Any(x => x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", "Nazwa użytkownika " + model.UserName + " jest już zajęta");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }

                // utworzenie uzytkownika
                UserDTO usserDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    UserName = model.UserName,
                    Password = model.Password,
                };

                // dodanie uzytkownika
                db.Users.Add(usserDTO);
                // zapis na bazie
                db.SaveChanges();

                // dodanie roli dla uzytkownika
                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = usserDTO.Id,
                    RoleId = 2
                };

                // dodanie roli
                db.UserRoles.Add(userRoleDTO);
                // zapis na bazie
                db.SaveChanges();
            }

            // TempData komunikat
            TempData["SM"] = "Jesteś teraz zarejestrowany i możesz się zalogować!";


            return Redirect("~/account/login");
        }

        // GET: /account/logout
     
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return Redirect("~/account/login");
        }

        public ActionResult UserNavPartial()
        {
            // pobieramy username;
            string userName = User.Identity.Name;
            // deklarujemy moodel
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                // pobieramy użytkownika
               UserDTO dto= db.Users.FirstOrDefault(x => x.UserName == userName);

                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName=dto.LastName,
                };

            }


            return PartialView(model);
        }

        //[Authorize]

        // GET: /account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            // pobieramy nazwe uzytkownika
            string username = User.Identity.Name;

            //deklarujemy model
            UserProfileVM model;

            using (Db db = new Db())
            {
                // pobieramy uzytkownika
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);

                model = new UserProfileVM(dto);
            }

            return View("UserProfile", model);
        }

       // POST: /account/user-profile
       [HttpPost]
       [ActionName("user-profile")]

        public ActionResult UserProfile(UserProfileVM model)
        {
            // sprawdzenie model state 
            if (!ModelState.IsValid)
            {
                return View("UserProfile",model);
            }
            // sprawdzay hasła
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("","Hasła nie są zgodne");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                // Pobieramy nazwe użytkownika
                string userName = User.Identity.Name;

                // sprawdzenie czy nazwa użytkownika jest unikalna
                if (db.Users.Where(x=>x.Id!=model.Id).Any(x=>x.UserName==userName))
                {
                    ModelState.AddModelError("", "Nazwa użytkownika "+ model.UserName + " zajęta");
                    model.UserName = "";
                    return View("UserProfile", model);
                }
                // edycj dto
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.UserName = model.UserName;
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }
                // ustawienie komunikatu zmiennej tempdata
                
                db.SaveChanges();
            }
            TempData["SM"] = " Edytowałeś swój profil";
            return Redirect("~/account/user-profile");
        }

      

            //// GET: /account/Orders
            //[Authorize(Roles = "User")]
            //public ActionResult Orders()
            //{
            //    // inicjalizacja listy zamowien dla uzytkownika
            //    List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            //    using (Db db = new Db())
            //    {
            //        // pobieramy id uzytkownika
            //        UserDTO user = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
            //        int userId = user.Id;

            //        // pobieramy zamowienia dla uzytkownika
            //        List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

            //        foreach (var order in orders)
            //        {
            //            // inicjalizacja slownika produktów
            //            Dictionary<string, int> productsAndQty = new Dictionary<string, int>();
            //            decimal total = 0m;

            //            // pobieramy szczegoly zamowienia
            //            List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

            //            foreach (var orderDetails in orderDetailsDTO)
            //            {
            //                // pobieramy produkt
            //                ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

            //                // pobieramy cene
            //                decimal price = product.Price;

            //                // pobieramy nazwe
            //                string productName = product.Name;

            //                // dodajemy produkt do slownika
            //                productsAndQty.Add(productName, orderDetails.Quantity);

            //                total += orderDetails.Quantity * price;
            //            }

            //            ordersForUser.Add(new OrdersForUserVM()
            //            {
            //                OrderNumber = order.OrderId,
            //                Total = total,
            //                ProductsAndQty = productsAndQty,
            //                CreatedAt = order.CreatedAt
            //            });
            //        }
            //    }

            //    return View(ordersForUser);
            //}

        }
}