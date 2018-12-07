using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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


    }
}