using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Capa_entidad;
using ConexionMongoDB;
using MongoDB.Bson;

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

        public ActionResult Validar_cod_conductor()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Validar_cod_conductor(string id_pueblo)
        {
            bool isValid = false;

            try
            {
                // Convertir id_pueblo a entero si es necesario
                if (!int.TryParse(id_pueblo, out int codigoInt))
                {
                    TempData["Mensaje"] = "Código inválido";
                    return RedirectToAction("Error", "Login");
                }

                // Obtener la colección desde MongoDB
                var collection = _context.GetCollection<Pueblo>("Pueblo");

                // Construir el filtro usando Builders<T>.Filter
                var filtro = Builders<Pueblo>.Filter.Eq("id_pueblo", codigoInt);
                var resultado = collection.Find(filtro).FirstOrDefault();

                // Verificar si el documento existe
                isValid = resultado != null;

                if (isValid)
                {
                    TempData["Mensaje_exito"] = "Codigo validado";
                    return RedirectToAction("Registro_conductor", "Login");
                }
                else
                {
                    TempData["Mensaje"] = "Código inválido";
                    return RedirectToAction("Validar_cod_conductor", "Login");
                }
            }
            catch (Exception ex)
            {
                // Manejar errores
                TempData["Mensaje"] = $"Error: {ex.Message}";
                return RedirectToAction("Validar_cod_conductor", "Login");
            }
        }


    }
}
