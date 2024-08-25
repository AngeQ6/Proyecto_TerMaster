using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TerMasterr.Controllers
{
    public class Administrador_generalController : Controller
    {
        // GET: Administrador_general
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Registrar_administrador_pueblo()
        {
            return View();
        }
        public ActionResult Pueblos()
        {
            return View();
        }
        public ActionResult Reportes()
        {
            return View();
        }
    }
}