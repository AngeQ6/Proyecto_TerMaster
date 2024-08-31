using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TerMasterr.Controllers
{
    public class HuellaController : Controller
    {
        // GET: Huella
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Authenticate()
        {
            // Simular el proceso de autenticación
            bool isAuthenticated = new Random().Next(2) == 0;

            return Json(new
            {
                success = isAuthenticated,
                message = isAuthenticated ? "Autenticación exitosa" : "Autenticación fallida. Intente de nuevo."
            });
        }
    }
}