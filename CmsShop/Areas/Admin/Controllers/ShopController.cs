using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.Linq;
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
        // POST: Admin/Shop/Categories/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            // deklaracja id
            string id;

            using (Db db = new Db())
            {
                // czy kategoria jest unikalna
                if (db.Categories.Any(x=>x.Name==catName))
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


        // POST: Admin/Shop/Categories/ReorderCategories
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
        //GET: Admin/Shop/Categories/ReorderCategories
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (Db db= new Db())
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



    }
}