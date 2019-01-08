using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class ShopController : Controller
    {
        private int productIDtoReview;
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }
        public ActionResult CategoryMenuPartial()
        {
            // deklarujemy categoryVm list
            List<CategoryVM> categoryVMList;
            // inicjalizacja listy 
            using (Db db = new Db())
            {
                categoryVMList = db.Categories
                                    .ToArray()
                                    .OrderBy(x => x.Sorting)
                                    .Select(x => new CategoryVM(x))
                                    .ToList();
            }
            // zwramy partial z lista 
            return PartialView(categoryVMList);
        }
        //GET: / shop/Category
        public ActionResult Category(string name)
        {
            // deklaracia productVMList
            List<ProductVM> productVMList;
            // pobranie z bazy produktów

            using (Db db = new Db())
            {
                // pobranie id categorui
                CategoryDTO categoryDTO = db.Categories
                    .Where(x => x.Slug == name)
                    .FirstOrDefault();
                int catId = categoryDTO.Id;
                // inicjalizcja produktow
                productVMList = db.Products
                    .ToArray()
                    .Where(x => x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();


                // pobieramy nazwe categori 
                 //  var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
               // ViewBag.CategoryName = productCat.CategoryName;
               // zmieniłem ponieważ jeżeli nie ma kategoria produktów wyrzuca expetion null point reference
                ViewBag.CategoryName = categoryDTO.Name;

            }
            //zwracamy widok z listą produktów z danej kategorii 
            return View(productVMList);
        }
        //GET: / shop/product-szczegoly/name
        [ActionName("product-szczegol")]
        public ActionResult ProductDetails(string name)
        {
            // deklaracja product VM i product dto
            ProductVM model;
            ProductDTO dto;
            // inicjalizacja product id
            int id = 0;
            using (Db db = new Db())
            {// sprazamy czy product istniej

                if (!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index" ,"Shop");
                }
                // inicjalizacja productDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();
                // pobranie=id
                id = dto.Id;
                // inicjalizacja modelu 
                model = new ProductVM(dto);
            }

            // pobrać gallery obrazków dla wybrango produktu
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));
            //zwracamy wdok z modelem 

       
            return View("ProductDetails",model);
        }

        [HttpGet]
        public ActionResult ReviewCreate(int id)
        {
            ReviewVM vm = new ReviewVM();
    
                vm.ProductId = id;

            return PartialView(vm);
        }
      
        //Post: / shop/ReviewCreate
        [HttpPost]
        public ActionResult ReviewCreate(ReviewVM model)
        {
            string username = User.Identity.Name;
            ProductVM model1;
            if (!string.IsNullOrWhiteSpace(model.Review))
            {

                
                using (Db db = new Db())
                {
                    

                    var user = db.Users.FirstOrDefault(x => x.UserName == username);
           
                ReviewDTO reviewDTO  = new ReviewDTO();
                    reviewDTO.ProductId = model.ProductId;
                reviewDTO.UserId = user.Id;
                reviewDTO.UserName = user.UserName;
                reviewDTO.CreateAt = DateTime.Now;
                reviewDTO.Review = model.Review;

                db.Reviews.Add(reviewDTO);
                db.SaveChanges();

                    // doświerzenie widoku 
                    ProductDTO dtoproduktu;
                    dtoproduktu = db.Products.Where(x => x.Id == model.ProductId).FirstOrDefault();
                    model1 = new ProductVM(dtoproduktu);
                    model1.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + dtoproduktu.Id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
                }
                return PartialView();
                // return View("ProductDetails", model1);
            }
            else
            {
                return PartialView();
            }
           
            
        }
        //Get: / shop/Review
    
        public ActionResult Review(int id)
        {
           
            List<ReviewVM> listReviewVM;
            using (Db db= new Db())
            {
                
                listReviewVM = db.Reviews.ToArray().Where(x => x.ProductId == id).Select(x=>new ReviewVM(x)).ToList();
            }
            return PartialView(listReviewVM);

            
        }

        //Get: / shop/ReviewDelete
        public ActionResult ReviewDelete(int id,int idProduktu)
        {
            ProductVM model;
            ProductDTO dtoproduktu;
            using (Db db = new Db())
            {
                dtoproduktu = db.Products.Where(x => x.Id == idProduktu).FirstOrDefault();
                model = new ProductVM(dtoproduktu);
                // pobieramy komentarza do usuniecia
                ReviewDTO dto = db.Reviews.Find(id);
                // usunięcie komentarza
                db.Reviews.Remove(dto);
                //zapisanie zmian
                db.SaveChanges();
            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + idProduktu + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            return View("ProductDetails",model);
           
        }
    }
}