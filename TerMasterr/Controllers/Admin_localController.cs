using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Capa_entidad;
using ConexionMongoDB;
using MongoDB.Bson;
using MongoDB.Driver;

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
                ViewBag.ErrorMessage = "Error al conectar con la base de datos: " + ex.Message;
            }
        }
        //////////////////////////////////////////////////////////////////////////////////
        #endregion

        ////////////////////////// VISAS /////////////////////////////////////////
        #region
        public ActionResult Bus()
        {
            var buses = _context.GetCollection<Bus>("Bus").Find(c => true).ToList();
            var conductores = _context.GetCollection<Conductor>("Conductor").Find(c => true).ToList();

            ViewBag.Conductores = conductores; // Enviamos la lista de conductores a la vista
            return View(buses); // Enviamos la lista de buses a la vista
        }


        public ActionResult Gestion_conductor()
        {
            var conductor = _context.GetCollection<Conductor>("Conductor").Find(c => true).ToList();
            return View(conductor);
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


        [HttpPost]
        public ActionResult AddBus(Bus bus)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Insertar el bus en la colección
                    _context.GetCollection<Bus>("Bus").InsertOne(bus);
                    TempData["SuccessMessage"] = "Bus agregado exitosamente.";
                    return RedirectToAction("Bus");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error al agregar el bus: " + ex.Message;
                    return RedirectToAction("Bus");
                }
            }

            // Si el modelo no es válido, devolver la vista con el modelo actual
            return View(bus);

        }



    }

}