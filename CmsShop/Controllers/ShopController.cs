using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class ShopController : Controller
    {
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


    }
}