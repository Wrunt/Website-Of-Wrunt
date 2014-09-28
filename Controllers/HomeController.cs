using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WruntsTools.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("/UnidentifiedMap/Index");
        }

        public ActionResult About()
        {
           return Redirect("/UnidentifiedMap/Index");
        }

        public ActionResult Contact()
        {
            return Redirect("/UnidentifiedMap/Index");
        }
    }
}