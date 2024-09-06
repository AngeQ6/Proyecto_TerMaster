using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Capa_entidad;
using ConexionMongoDB;

namespace TerMasterr.Controllers
{
    public class Admin_localController : Controller
    {
        ///////////////////////////////////////// CONEXION /////////////////////////////////////////
        #region
        private readonly Conexion _context;
        public Admin_localController()
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
        //////////////////////////////////////////////////////////////////////////////////
        #endregion

        ////////////////////////// VISAS /////////////////////////////////////////
        #region
        public ActionResult Bus()
        {
            return View(_buses);
        } 
        
        public ActionResult Conductores()
        {
            return View();
        }
        
        public ActionResult Asignar_horarios()
        {
            return View();
        }

        public ActionResult Editar_datos_personales()
        {
            return View();
        }
        
        public ActionResult Reportes()
        {
            return View();
        }
        ////////////////////////////////////////////////////////////////////////////
        #endregion
        private static List<Bus> _buses = new List<Bus>
        {
            new Bus { Id = 1, Number = "001", Route = "Downtown - Suburb", Capacity = 50, ImageUrl = "/images/placeholder.jpg" },
            new Bus { Id = 2, Number = "002", Route = "Airport - City Center", Capacity = 40, ImageUrl = "/images/placeholder.jpg" },
            new Bus { Id = 3, Number = "003", Route = "University - Shopping Mall", Capacity = 30, ImageUrl = "/images/placeholder.jpg" }
        };
        
        [HttpPost]
        public ActionResult Add(Bus bus)
        {
            if (ModelState.IsValid)
            {
                bus.Id = _buses.Max(b => b.Id) + 1;
                if (string.IsNullOrEmpty(bus.ImageUrl))
                {
                    bus.ImageUrl = "/images/placeholder.jpg";
                }
                _buses.Add(bus);
                return RedirectToAction("Index");
            }
            return View("Bus", _buses);
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var bus = _buses.FirstOrDefault(b => b.Id == id);
            if (bus != null)
            {
                _buses.Remove(bus);
            }
            return RedirectToAction("Bus");
        }
        public ActionResult Gestion_conductor()
        {
            return View();
        }
    }
    
}