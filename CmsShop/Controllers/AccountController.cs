﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
        //GET:/account/create-account
        [ActionName("create-account")]
        public ActionResult CreatAccount()
        {
            return View("CreateAccount");
        }
    }
}