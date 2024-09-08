using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public ActionResult Asignar_horarios()

        {
            var conductor = _context.GetCollection<Conductor>("Conductor").Find(c => true).ToList();
            return View(conductor);
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

        public ActionResult AddBus(Bus bus)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si ya existe un bus con la misma placa
                    var busExistente = _context.GetCollection<Bus>("Bus")
                        .Find(b => b.placa == bus.placa)
                        .FirstOrDefault();

                    if (busExistente != null)
                    {
                        TempData["ErrorMessage"] = "Ya existe un bus registrado con la misma placa.";
                        return RedirectToAction("Bus");
                    }

                    // Verificar si el conductor ya está asignado a otro bus
                    var conductorAsignado = _context.GetCollection<Bus>("Bus")
                        .Find(b => b.id_conductor == bus.id_conductor)
                        .FirstOrDefault();

                    if (conductorAsignado != null)
                    {
                        TempData["ErrorMessage"] = "El conductor ya está asignado a otro bus.";
                        return RedirectToAction("Bus");
                    }

                    // Insertar el nuevo bus en la colección
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
        public ActionResult Eliminar_bus(int id)
        {
            try
            {
                // Buscar el bus por su Id
                var bus = _context.GetCollection<Bus>("Bus")
                    .Find(b => b.id_conductor == id)
                    .FirstOrDefault();

                if (bus == null)
                {
                    TempData["ErrorMessage"] = "No se encontró un bus con el ID proporcionado.";
                    return RedirectToAction("Bus");
                }

                // Eliminar el bus
                _context.GetCollection<Bus>("Bus").DeleteOne(b => b.id_conductor == id);
                TempData["SuccessMessage"] = "Bus eliminado exitosamente.";
                return RedirectToAction("Bus");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el bus: " + ex.Message;
                return RedirectToAction("Bus");
            }
        }
        public ActionResult GetBus(int id)
        {
            // Busca el bus por su ID
            var bus = _context.GetCollection<Bus>("Bus").Find(x => x.id_conductor == id).FirstOrDefault();

            if (bus != null)
            {
                // Devuelve los datos del bus en formato JSON
                return Json(bus, JsonRequestBehavior.AllowGet);
            }

            return HttpNotFound("Bus no encontrado");
        }

        [HttpPost]
        public ActionResult EditBus(Bus bus)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Actualizar los datos del bus
                    var filter = Builders<Bus>.Filter.Eq("id_conductor", bus.id_conductor);
                    var update = Builders<Bus>.Update
                        .Set("placa", bus.placa)
                        .Set("id_conductor", bus.id_conductor)
                        .Set("nombre", bus.nombre);

                    _context.GetCollection<Bus>("Bus").UpdateOne(filter, update);

                    TempData["SuccessMessage"] = "Bus editado exitosamente.";
                    return RedirectToAction("Bus");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error al editar el bus: " + ex.Message;
                    return RedirectToAction("Bus");
                }
            }

            return View(bus);
        }


        [HttpPost]
        public ActionResult Get_conductor(int id)
        {
            var conductor = _context.GetCollection<Conductor>("Conductor")
                            .Find(c => c.id_conductor == id)
                            .FirstOrDefault();

            if (conductor == null)
            {
                return Json(new { success = false, message = "Conductor no encontrado" });
            }

            return Json(new
            {
                success = true,
                id_conductor = conductor.id_conductor,
                nombre = conductor.nombre,
                Estado = conductor.Estado
            });
        }


    }

}