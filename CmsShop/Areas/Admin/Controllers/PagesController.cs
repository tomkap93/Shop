using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        [HttpGet]
        public ActionResult Index()
        {
            // deklaracja listy PageVM
            List<PageVM> pagesList;



            using (Db db = new Db())
            {
                // inicializacja listy
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
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

            if (!ModelState.IsValid)
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
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
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

        // GET: Admin/Pages/EditPage
        [HttpGet]
        public ActionResult EditPage(int id)
        {

            // deklaracja pageVM
            PageVM model;

            using (Db db = new Db())
            {
                // pobieramy strone z bazy danych o przekazanym id
                PageDTO dto = db.Pages.Find(id);
                // sprawdzamy czy taka strona istnieje
                if (dto == null)
                {
                    return Content("Strona nie istnieje");
                }
                model = new PageVM(dto);
            }


            return View(model);
        }
        // POST: Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                //Pobranie id strony 
                int id = model.Id;
                string slug = "home";

                //pobranie strony do edycji

                PageDTO dto = db.Pages.Find(id);

                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }
                // sprawdzenie unikalność strony i adresu
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Strona lub asres strony już istnieje.");
                }
                // modifikacja DTO
                dto.Title = model.Title;
                dto.Slug = slug;
                dto.HasSidebar = model.HasSidebar;
                dto.Body = model.Body;

                //zapis do bazy danych
                db.SaveChanges();

            }
            //komunikat
            TempData["SM"] = "Wyedytowałeś stronę";

            //Redirect
            return RedirectToAction("EditPage");


        }
        // GET: Admin/Pages/Details/id
        [HttpGet]
        public ActionResult Details(int id)
        {
            //deklaracja PageVM
            PageVM model;
            using (Db db = new Db())
            {
                //Pobranie strony o id 
                PageDTO dto = db.Pages.Find(id);
                if (dto == null)
                {
                    return Content("Strona o podanym id nie istnieje");
                }
                // inicjalizacja PageVM 
                model = new PageVM(dto);

            }

            return View(model);
        }

        // GET: Admin/Pages/Delete/id
        [HttpGet]
        public ActionResult Delete(int id)
        {
            using (Db db = new Db())
            {
                //pobieranie strony do usuniecia
                PageDTO dto = db.Pages.Find(id);
                // wybranej usuwanie strony z bazy
                db.Pages.Remove(dto);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        // POST: Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                int count = 1;
                PageDTO dto;
                //sortowanie stron zapis na bazie
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }

            }
            return View();
        }

        // GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Dekleracja Sidebar
            SidebarVM model;
            using (Db db = new Db())
            {
                //pobiramy SlidebarDTO
                SidebarDTO dto = db.Sidebar.Find(1);

                // inicializacja modelu
                model = new SidebarVM(dto);

            }
            return View(model);
        }
        // POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            //Dekleracja Sidebar

            using (Db db = new Db())
            {//pobiramy SlidebarDTO z bazy 
                SidebarDTO dto = db.Sidebar.Find(1);
                dto.Body = model.Body;
                db.SaveChanges();
            }
            TempData["SM"] = "Zmodyfikowałeś Pasek Boczny.";

            /// przekierowanie 
            return RedirectToAction("EditSidebar");
        }
    }
    }