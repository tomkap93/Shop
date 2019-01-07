using CmsShop.Models.Data;
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
        [HttpGet]
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

    
        // GET: /account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {

            return View("CreateAccount");
        }
        //GET:/account/login
        [HttpGet]
        public ActionResult Login()

        {         // sprawdzanie czy uzytkownik nie jest juz zalogowany
            string userName = User.Identity.Name;
            if (!string.IsNullOrEmpty(userName))
                return RedirectToAction("user-profile");

            // zwracamy widok
            return View();
        }

        //POST:/account/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //sprawdzenie model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //sprawdzamy uzytkownika
            bool isValid = false;
            using (Db db=new Db())
            {
                if (db.Users.Any(x=>x.UserName.Equals(model.UserName)&& x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }
            if (!isValid)
            {
                ModelState.AddModelError("", "Nie prawidłowa nazwa użytkownika lub hasło");

                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.UserName,model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.UserName,model.RememberMe));
            }          
        }
        //GET:/account/logout
        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }

        [HttpPost]
          [ActionName("create-account")]
   public ActionResult CreateAccount(UserVM model)
        {
            // sprawdzenie model state
            if(!ModelState.IsValid)
            {
                return View("CreateAccount",model);
            }
            // sprawdzenie hasła 
            if(!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("","Hasła nie są identyczne");
                return View("CreateAccount", model);
            }
      
            using (Db db = new Db())
            {
                //sprawdzanie czy nazwa użytkownika jest unikalna 
                if (db.Users.Any(x=> x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", "Nazwa użytkownika "+model.UserName+" jest już zajęta");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }
                // utworzenie użytkownika
                UserDTO dto = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    UserName = model.UserName,
                    Password = model.Password,


                };
                // dodanie użytkownika do bazy
                db.Users.Add(dto);
                // zapis na bazie
                db.SaveChanges();

                // dodanie roli 
                UserRoleDTO userRoleDTO = new UserRoleDTO() {
                    UserId = dto.Id,
                    RoleId = 2
                };
                // ustawienie roli
                db.UserRoles.Add(userRoleDTO);
                // dodanie do bazy
                db.SaveChanges();

            }

            // tempdate komunikat
            TempData["SM"] = "Jesteś teraz zarejestrowwany i możesz się zalogować!!";
        



            return Redirect("~/account/login");
        }

     

    }
}