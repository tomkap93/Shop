using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using PagedList;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
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
    }
}