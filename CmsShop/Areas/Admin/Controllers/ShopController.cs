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
        public ActionResult AddNewCategory()
        {
            return View();
        }
    }
}