using CmsShop.Class;
using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Account;
using CmsShop.Models.ViewModels.Shop;
//using iTextSharp.text;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
using MigraDoc.Rendering;
using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using PdfSharp.Pdf;


using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using PdfSharp;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text;

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
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return Redirect("~/account/login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            // pobieramy username
            string username = User.Identity.Name;

            // deklarujemy model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                // pobieramy uzytkownika
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);

                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }

            return PartialView(model);
        }

        [Authorize]
        public ActionResult UserNavPartial2()
        {
            // Get username
            string username = User.Identity.Name;

            // Declare model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                // Get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);
                if (dto != null)
                {
                    // Build the model
                    model = new UserNavPartialVM()
                    {
                        FirstName = dto.FirstName,
                        LastName = dto.LastName
                    };
                }
                else
                    model = new UserNavPartialVM();
            }

            // Return partial view with model
            return PartialView(model);
        }

        [Authorize]
        public ActionResult UserNavPartial3()
        {
            // Get username
            string username = User.Identity.Name;
            string email = string.Empty;

            // Declare model
            UserNavPartialVM3 model;

            using (Db db = new Db())
            {
                // Get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);

                // Build the model
                // Build the model
                model = new UserNavPartialVM3()
                {
                    Email = dto.EmailAddress
                };
            }

            // Return partial view with model
            return PartialView(model);
        }


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
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            // sprawdzenie model state
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            // sprawdzamy hasła
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Hasła nie pasują do siebie.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                // pobieramy nazwe uzytkownika
                string username = User.Identity.Name;

                // sprawdzenie czy nazwa uzytkownika jest unikalna
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.UserName == username))
                {
                    ModelState.AddModelError("", "Nazwa użytkownika " + model.UserName + " zajęta");
                    model.UserName = "";
                    return View("UserProfile", model);
                }

                // edycja DTO
                UserDTO dto = db.Users.Find(model.Id);
                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.UserName = model.UserName;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                // zapis
                db.SaveChanges();
            }


            // ustawienie komunikatu
            TempData["SM"] = "Edytowałeś swój profil!";

            return Redirect("~/account/user-profile");
        }


        // GET: /account/Orders
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            // inicjalizacja listy zamowien dla uzytkownika
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                // pobieramy id uzytkownika
                UserDTO user = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                // pobieramy zamowienia dla uzytkownika
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                foreach (var order in orders)
                {
                    // inicjalizacja slownika produktów
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();
                    decimal total = 0m;

                    // pobieramy szczegoly zamowienia
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        // pobieramy produkt
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // pobieramy cene
                        decimal price = product.Price;

                        // pobieramy nazwe
                        string productName = product.Name;

                        // dodajemy produkt do slownika
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        total += orderDetails.Quantity * price;
                    }

                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            return View(ordersForUser);
        }
        public FileResult Drukuj(int orderid)
        {

            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
        
            string strPDFFileName = string.Format("Faktura Sklep" + dTime.ToString("yyyyMMdd") + "-" + orderid + ".pdf");
            iTextSharp.text.Document doc = new iTextSharp.text.Document();            
            doc.SetMargins(5f, 5f, 5f, 5f);
        
            PdfPTable tableLayout = new PdfPTable(5);
            doc.SetMargins(20f, 10f, 10f, 10f);
            string strAttachment = Server.MapPath("~/Downloadss/" + strPDFFileName);

            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open(); 
            doc.Add(Add_Content_To_PDF(tableLayout, orderid));

            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);


        }
        protected PdfPTable Add_Content_To_PDF(PdfPTable tableLayout , int i)
        {
            DateTime dTime = DateTime.Now;
            using (Db db = new Db())
            {

                UserDTO user = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();

                float[] headers = { 50, 24, 45, 35, 50 }; //Header Widths  
            tableLayout.SetWidths(headers); //Set the pdf headers  
            tableLayout.WidthPercentage = 100; //Set the PDF File witdh percentage  
            tableLayout.HeaderRows = 1;
                //Add Title to the PDF file at the top  

                List<OrderDetailsDTO> orderDetails = db.OrderDetails.Where(x => x.OrderId == i).ToList();
                //   List<OrderDTO> employees = _context.employees.ToList<Employee>();

                string strPDFFileName = string.Format("Faktura Sklep" + dTime.ToString("yyyyMMdd"));
                tableLayout.AddCell(new PdfPCell(new Phrase("Faktura nr "+ i, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 20, 1, new iTextSharp.text.BaseColor(0, 0, 0)))) {
                Colspan = 12, Border = 0, PaddingBottom = 5, HorizontalAlignment = Element.ALIGN_CENTER
            });
                tableLayout.AddCell(new PdfPCell(new Phrase(" Data "+ strPDFFileName, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 15, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });
                tableLayout.AddCell(new PdfPCell(new Phrase(" Sprzedawca" ,new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 15, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });

                tableLayout.AddCell(new PdfPCell(new Phrase("Sklep UTP", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });
                tableLayout.AddCell(new PdfPCell(new Phrase("Adres ", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });
                tableLayout.AddCell(new PdfPCell(new Phrase("Nr tele ", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });
                tableLayout.AddCell(new PdfPCell(new Phrase("NIP ", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });
                tableLayout.AddCell(new PdfPCell(new Phrase("Regon ", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });

                ////Add header  
                AddCellToHeader(tableLayout, "Nr");
            AddCellToHeader(tableLayout, "Product");
            AddCellToHeader(tableLayout, "Cena");
            AddCellToHeader(tableLayout, "Ilość");
            AddCellToHeader(tableLayout,"Wartość");

                ////Add body  

                decimal suma = 0;

            foreach (var item in orderDetails)
            {

                    AddCellToBody(tableLayout, item.Products.Id.ToString());
                    AddCellToBody(tableLayout, item.Products.Name.ToString());
                    AddCellToBody(tableLayout, item.Products.Price.ToString());
                    AddCellToBody(tableLayout, item.Quantity.ToString());
                    AddCellToBody(tableLayout, (item.Products.Price*item.Quantity).ToString());
                    suma +=item.Products.Price * item.Quantity;
                }

                tableLayout.AddCell(new PdfPCell(new Phrase("Suma :" + suma, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });
                tableLayout.AddCell(new PdfPCell(new Phrase("Kupujący", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 15, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });

                tableLayout.AddCell(new PdfPCell(new Phrase("Nr kupującego " + user.Id, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });

                tableLayout.AddCell(new PdfPCell(new Phrase("Imię i nazwisko :" +user.FirstName +" "+ user.LastName, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12, 
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });
                tableLayout.AddCell(new PdfPCell(new Phrase("Adres email " + user.EmailAddress, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });
                tableLayout.AddCell(new PdfPCell(new Phrase("Nr tele "+user.Id, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });
    
            }

            return tableLayout;
        }
        // Method to add single cell to the Header  
        private static void AddCellToHeader(PdfPTable tableLayout, string cellText)
        {

            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.YELLOW)))
            {
                HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(128, 0, 0)
    });
        }

        // Method to add single cell to the body  
        private static void AddCellToBody(PdfPTable tableLayout, string cellText)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.BLACK)))
             {
                HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
    });
        }

    }
}