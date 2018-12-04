using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{pages}
        [HttpGet]
        public ActionResult Index(string page = "")
        {
            // ustawiamy adres naszej strony 
            if (page == "")
                page = "home";
            // deklarujemy pageVM i pageDTO
            PageVM model;
            PageDTO dto;
            // sprawdzamy czy nasza stona istnieje 
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                    return RedirectToAction("Index", new { page = "" });
            }
            // pobieramy pageDTO
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }
            // ustawiamy tytuł naszej strony
            ViewBag.PageTitle = dto.Title;

            // sprawdzay czy strona ma pasek boczny 
            if (dto.HasSidebar == true)
                ViewBag.Sidebar = "Tak";
            else
                ViewBag.Sidebar = "Nie";
            // inicjalizować model pageVM
            model = new PageVM(dto);
            // Zwrracamy widog z pageVM

            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            //deklaracja PageVM
            List<PageVM> pageVMList;
            // pobranie stron 
            using (Db db = new Db())
            {
                pageVMList = db.Pages.ToArray()
                    .OrderBy(x => x.Sorting)
                    .Where(x => x.Slug != "home")
                    .Select(x => new PageVM(x)).ToList();

            }

            // zwracamy pageVM list 
            return PartialView(pageVMList);
        }
    }
}