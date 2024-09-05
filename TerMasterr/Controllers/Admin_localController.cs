using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Capa_entidad;
using ConexionMongoDB;
using MongoDB.Driver;

namespace TerMasterr.Controllers
{
    public class Admin_localController : Controller
    {
        private readonly Conexion _context;
        private readonly IMongoCollection<Asistencia> _asistencias;
        private readonly IMongoCollection<Conductor> _conductores;

        public Admin_localController()
        {
            try
            {
                _context = new Conexion();
                _asistencias = _context.GetCollection<Asistencia>("asistencia");
                _conductores = _context.GetCollection<Conductor>("conductor");
            }
            catch (ApplicationException ex)
            {
                ViewBag.ErrorMessage = "Error al conectar con la base de datos: " + ex.Message;
            }
        }

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
                return RedirectToAction("Bus");
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

        public ActionResult Lista_conductor()
        {
            var asistencias = _asistencias.Find(_ => true).ToList();
            var conductores = _conductores.Find(_ => true).ToList();

            var viewModel = asistencias.Select(a => new
            {
                a.IdConductor,
                a.PlacaBus,
                a.FechaIngreso,
                NombreConductor = conductores.FirstOrDefault(c => c.id_conductor == a.IdConductor)?.nombre
            }).ToList();

            return View(viewModel);
        }
    }
}
