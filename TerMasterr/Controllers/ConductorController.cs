using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TerMasterr.Controllers
{
    public class ConductorController : Controller
    {
        // GET: Conductor
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ModalView()
        {
            return View();
        }

        public ActionResult ModalContent()
        {
            // Lógica para obtener el contenido que deseas mostrar
            return PartialView("_PartialModalContent"); // Devuelve otro PartialView o contenido que necesites
        }
    }
}