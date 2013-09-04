using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cuatro.Common;

namespace thunsaker.cuatro.demo.Controllers {
    public class HomeController : Controller {
        //
        // GET: /Home/
        public ActionResult Index() {
            return View();
        }

        //
        // GET: /Home/Profile
        public ActionResult Profile(FoursquareUser user) {
            if (user != null)
                return View(user);
            else
                return View();
        }

        public ActionResult Error() {
            if (Request["error"] != null)
                @ViewBag.ErrorMessage = Request["error"];

            return View();
        }
    }
}