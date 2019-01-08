using CmsShop.Areas.Admin.Models.ViewModels.Shop;
using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        [HttpGet]
        public ActionResult Categories()
        {
            // deklaracja listy
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray()
                                                .OrderBy(x => x.Sorting)
                                                .Select(x => new CategoryVM(x))
                                                .ToList();
            }


            return View(categoryVMList);
        }
        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            // deklaracja id
            string id;

            using (Db db = new Db())
            {
                // czy kategoria jest unikalna
                if (db.Categories.Any(x => x.Name == catName))
                    return "tytulzajety";

                // inicjalizacja DTO

                CategoryDTO dto = new CategoryDTO();
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 1000;
                // zapis do bazy
                db.Categories.Add(dto);
                db.SaveChanges();
                //pobieramy id

                id = dto.Id.ToString();
            }

            return id;
        }


        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public ActionResult ReorderCategories(int[] id)

        {

            using (Db db = new Db())
            {
                //inicializacja licznika
                int count = 1;
                // deklaracja dto
                CategoryDTO dto;
                //sortowanie kategorii
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;
                    //zapis na bazie 
                    db.SaveChanges();
                    count++;

                }


            }

            return View();
        }
        //GET: Admin/Shop/ReorderCategories
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                // pobieramy kategorie do usuniecia
                CategoryDTO dto = db.Categories.Find(id);
                // usunięcie kategorii
                db.Categories.Remove(dto);
                //zapisanie zmian
                db.SaveChanges();
            }
            return RedirectToAction("Categories");
        }
        // POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                // sprawdzenie czy kategoria jest unikalna
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "tytulzajety";

                // edycja kategorii

                CategoryDTO dto = db.Categories.Find(id);
                // edycja kategorii
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();
                db.SaveChanges();



            }




            return "ok";
        }


        //GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            // inicjalizacja modelu
            ProductVM model = new ProductVM();
            // pobieramy liste kategorii
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

            }


            return View(model);
        }
        //POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            // sprawdzenie modelu state
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }

            }
            // spradzenie czy nazwa produktu jest unialna

            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "ta nazwa produktu jest zajęta");
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }
            // deklaracja product id
            int id;
            // dodawanie produktu i zapis na bazie 
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDto.Name;

                db.Products.Add(product);
                db.SaveChanges();

                // pobranie id  dodanego productu 
                id = product.Id;

            }
            // ustawiamy komunikat ze product został dodany 
            TempData["SM"] = "Dodałeś produkt";
            #region Upload Image
            // utworzenie potrzebnej struktury katalogów 
            var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(orginalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");
            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);
            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);
            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);
            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);
            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            if (file != null && file.ContentLength > 0)
            {
                // sprawdzenie rozszerzenia pliku czy mamy do czynienia z obraziem 
                string ext = file.ContentType.ToLower();
                if (ext != "image/jpg" &&
                   ext != "image/jpeg" &&
                   ext != "image/png" &&
                   ext != "image/pjpg" &&
                   ext != "image/gif" &&
                   ext != "image/x-png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Obrazek nie został przesłany - nie prawidłowe rozszerzenie obrazu ");
                        return View(model);
                    }
                }

                // inicjalizacja nazwy obrazka
                string imageName = file.FileName;
                //
                //zapis nazwy obrazka do bazy 
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);
                // zapis orginalny obrazek 
                file.SaveAs(path);

                //zapisujemy miniaturke i zmieniamy rozmiar zdjęcia 
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);


            }


            #endregion
            return RedirectToAction("AddProduct");
        }

        //GET: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            // Deklaracja listy Produktów
            List<ProductVM> listOfProductVM;

            // ustawiamy nr strony 
            var pageNumber = page ?? 1;
            using (Db db = new Db())
            {

                // inicjalizacja listy produktów    
                listOfProductVM = db.Products.ToArray()
                                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                    .Select(x => new ProductVM(x))
                                    .ToList();
                // lista kategori do dropDownList
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                //ustawiamy wybraną kategorie 
                ViewBag.SelectedCat = catId.ToString();
            }

            // ustawianie stronnicowania
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 5);
            ViewBag.OnePageOfProducts = onePageOfProducts;
            //zwracamy widok z listą produktów
            return View(listOfProductVM);
        }
        //GET: Admin/Shop/EditProduct
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            // deklaracja productVM

            ProductVM model;

            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                // sprawdzenie czy produkct istnieje 

                if (dto == null)
                {
                    return Content("Ten product nie istnieje");

                }
                // inicjalizacja modelu 
                model = new ProductVM(dto);

                // lista katgori 
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                // ustawiamy zdjecia
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));

            }


            return View(model);
        }

        //Post: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {

            ///pobranie id productu
            int id = model.Id;

            //pobranie kategori i dla listy rozwijanej
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            //ustawiamy zdjecia
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            //sprawdzamy model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // sprawdzenie unikalośći nazwy produktu 

            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Ta nazwa produktu jest zajęta");
                    return View(model);
                }
            }

            //Edycja productu , zapis na bazie 

            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Price = model.Price;
                dto.Description = model.Description;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;
                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDto.Name;

                db.SaveChanges();


            }
            // ustawienie tempdata
            TempData["SM"] = "Edytowałes product";

            #region Image UpLoad
            // sprawdzenie czy plik jest i ca ma jakies dane 
            if (file != null && file.ContentLength > 0)
            {
                // sprawdzenie rozszerzenia pliku 
                string ext = file.ContentType.ToLower();
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/png" &&
                    ext != "image/x-png"
                    )
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Obraz został przesłany - nie prawdłowe rozszerzenie obrazu.");
                        return View(model);
                    }

                }
                //  utworenie potrebnej struktury katalogów


                var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));


                var pathString1 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //usuwamy start pliki z katalogów
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                    file2.Delete();

                foreach (var file3 in di2.GetFiles())
                    file3.Delete();

                // zapis nazwe obrazka na bazie 
                string imageName = file.FileName;
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }



                // zapis nowych plików do katalogów

                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);
                // zapis orginalny obrazek 
                file.SaveAs(path);

                //zapisujemy miniaturke i zmieniamy rozmiar zdjęcia 
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }

            #endregion



            return RedirectToAction("EditProduct");
        }
        //GET: Admin/Shop/DeleteProduct
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            // sniecie prodkt z bazy 

            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }
            // usunięcie folder produktu z wszystkimi plikami

            var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            var pathString = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);


            return RedirectToAction("Products");
        }

        //Post: Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public ActionResult SaveGalleryImages(int id)
        {
            //pętla po obrazkach 
            foreach (string FileName in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[FileName];
                // sprawdzenie czy mamy plik i czy nie jest pusty
                if (file != null && file.ContentLength>0)
                {
                    //Utowrzenie potrzebnej struktury katalogów
                    var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                    string pathString1 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    var path1 = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    // Zapis obrazków i miniaturek 
                    file.SaveAs(path1);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200); // zmiana rozmiaru
                    img.Save(path2);

                }
            }
            return View();
        }

        // POST: Admin/Shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

        //GET: Admin/Shop/Orders
        public ActionResult Orders()
        {

            List<OrdersForAdminVM> ordersForAdminVM = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();
                foreach (var order in orders)
                {
                    // inicjalizacja słownik dla produktów 
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();
                    decimal total = 0M;
                    //Inicjalizacja ordersDeatalsDTO
                    List<OrderDetailsDTO> orderDeatalsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();
                    // pobieramy użytkownika
                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string userName = user.UserName;
                    foreach (var orderDeatal in orderDeatalsList)
                    {
                        // pobieramy product 
                        ProductDTO product = db.Products.Where(x => x.Id == orderDeatal.ProductId).FirstOrDefault();
                        //
                        // pobieramy cene productu 
                        decimal price = product.Price;
                        // pobieramy nazwa productu
                        string productName = product.Name;
                        // dodać produćt do słownika 
                        productsAndQty.Add(productName, orderDeatal.Quantity);
                        // ustawiamy wartośc total
                        total += orderDeatal.Quantity * price;
                    }
                    ordersForAdminVM.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        UserName = userName,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    }
                        );
                }

            }

            return View(ordersForAdminVM);
        }

        public FileResult Drukuj(int OrderNumber)
        {

            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created   
            string strPDFFileName = string.Format("SamplePdf" + dTime.ToString("yyyyMMdd") + "-" + ".pdf");
            iTextSharp.text.Document doc = new iTextSharp.text.Document();

            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table with 5 columns  
            PdfPTable tableLayout = new PdfPTable(5);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //Create PDF Table  

            //file will created in this path  
            string strAttachment = Server.MapPath("~/Downloadss/" + strPDFFileName);


            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            //Add Content to PDF   
            doc.Add(Add_Content_To_PDF(tableLayout, OrderNumber));

            // Closing the document  
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);


        }
        protected PdfPTable Add_Content_To_PDF(PdfPTable tableLayout, int i)
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
                tableLayout.AddCell(new PdfPCell(new Phrase("Faktura nr " + i, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 20, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
                tableLayout.AddCell(new PdfPCell(new Phrase(" Data " + strPDFFileName, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 15, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
                {
                    Colspan = 12,
                    Border = 0,
                    PaddingBottom = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });
                tableLayout.AddCell(new PdfPCell(new Phrase(" Sprzedawca", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 15, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
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
                AddCellToHeader(tableLayout, "Wartość");

                ////Add body  

                decimal suma = 0;

                foreach (var item in orderDetails)
                {

                    AddCellToBody(tableLayout, item.Products.Id.ToString());
                    AddCellToBody(tableLayout, item.Products.Name.ToString());
                    AddCellToBody(tableLayout, item.Products.Price.ToString());
                    AddCellToBody(tableLayout, item.Quantity.ToString());
                    AddCellToBody(tableLayout, (item.Products.Price * item.Quantity).ToString());
                    suma += item.Products.Price * item.Quantity;
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

                tableLayout.AddCell(new PdfPCell(new Phrase("Imię i nazwisko :" + user.FirstName + " " + user.LastName, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
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
                tableLayout.AddCell(new PdfPCell(new Phrase("Nr tele " + user.Id, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, new iTextSharp.text.BaseColor(0, 0, 0))))
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
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                BackgroundColor = new iTextSharp.text.BaseColor(128, 0, 0)
            });
        }

        // Method to add single cell to the body  
        private static void AddCellToBody(PdfPTable tableLayout, string cellText)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.BLACK)))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
            });
        }

    }
}