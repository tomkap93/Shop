using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            // deklaracja listy PageVM
            List<PageVM> pagesList;

      

            using (Db db = new Db())
            {
                // inicializacja listy
                pagesList = db.Pages.ToArray().OrderBy(x=>x.Sorting).Select(x=> new PageVM(x)).ToList();
            }

            //zwracamy strony do widoku 
            return View(pagesList);
        }
        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }
        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            // Sprawdzanie model state
            
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                string slug;
                // Inicializacja PageDTO
                PageDTO dto = new PageDTO();
            

                //Gdy adres strony nie jest wypełniony to jest przypisywany tytuł 
                if (string.IsNullOrWhiteSpace(model.Slug))      
                    slug = model.Title.Replace(" ", "-").ToLower();
                else
                    slug = model.Slug.Replace(" ", "-").ToLower();

                //zapobiegamy dodaniu takiej samej nazwy strony 
                if (db.Pages.Any(x=>x.Title==model.Title)|| db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Ten tytuł lub adres strony już istnieje.");
                    return View(model);
                }


                dto.Title = model.Title;
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 1000;

                //zapis dto
                db.Pages.Add(dto);
                db.SaveChanges();

            }

            TempData["SM"] = "Dodałeś nową stronę";

            return RedirectToAction("AddPage");
        }
    }
}