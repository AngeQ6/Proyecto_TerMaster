using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Capa_entidad;
using ConexionMongoDB;

namespace TerMasterr.Controllers
{
    public class LoginController : Controller
    {
        private readonly Conexion _context;
        public LoginController()
        {
            try
            {
                _context = new Conexion();
            }
            catch (ApplicationException ex)
            {
                // Captura cualquier error de conexión
                ViewBag.ErrorMessage = "Error al conectar con la base de datos: " + ex.Message;
            }
        }

        // GET: Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        
        public ActionResult Registro_conductor()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Conductor model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var conductor = _context.GetCollection<Conductor>("Conductor")
                                            .Find(c => c.id_conductor == model.id_conductor && c.contraseña == model.contraseña)
                                            .FirstOrDefault();

                    if (conductor != null)
                    {
                        return RedirectToAction("Index", "Conductor");
                    }
                    else
                    {
                        ViewBag.Error = "Número de identificación o contraseña incorrecta";
                        return View();
                    }
                }
                catch (ApplicationException ex)
                {
                    ViewBag.Error = "Error al procesar la solicitud: " + ex.Message;
                    return View();
                }
            }

            ViewBag.Error = "ModelState no es válido";
            return View();

        }


    }
}