using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Capa_entidad;

namespace TerMasterr.Controllers
{
    public class Admin_localController : Controller
    {
        //private static List<Conductor> conductores = new List<Conductor>
        //{
        //    new Conductor { Id = 1, Nombre = "Juan Pérez", PlacaBus = "ABC123", Horario = "", Orden = 1 },
        //    new Conductor { Id = 2, Nombre = "María López", PlacaBus = "DEF456", Horario = "", Orden = 2 },
        //    new Conductor { Id = 3, Nombre = "Carlos Rodríguez", PlacaBus = "GHI789", Horario = "", Orden = 3 },
        //    new Conductor { Id = 4, Nombre = "Ana Martínez", PlacaBus = "JKL012", Horario = "", Orden = 4 },
        //    new Conductor { Id = 5, Nombre = "Pedro Sánchez", PlacaBus = "MNO345", Horario = "", Orden = 5 }
        //};
        //// GET: Admin_local
        //public ActionResult Index()
        //{
        //    var conductoresOrdenados = conductores.OrderBy(c => c.Orden).ToList();
        //    return View(conductoresOrdenados);
        //}
        //[HttpPost]
        //public ActionResult ActualizarHorario(int id, string horario)
        //{
        //    var conductor = conductores.FirstOrDefault(c => c.Id == id);
        //    if (conductor != null)
        //    {
        //        conductor.Horario = horario;
        //    }
        //    return Json(new { success = true });
        //}

        //[HttpPost]
        //public ActionResult ReordenarConductores(List<int> nuevoOrden)
        //{
        //    for (int i = 0; i < nuevoOrden.Count; i++)
        //    {
        //        var conductor = conductores.FirstOrDefault(c => c.Id == nuevoOrden[i]);
        //        if (conductor != null)
        //        {
        //            conductor.Orden = i + 1;
        //        }
        //    }
        //    return Json(new { success = true });
        //}

        private static List<Bus> _buses = new List<Bus>
        {
            new Bus { Id = 1, Number = "001", Route = "Downtown - Suburb", Capacity = 50, ImageUrl = "/images/placeholder.jpg" },
            new Bus { Id = 2, Number = "002", Route = "Airport - City Center", Capacity = 40, ImageUrl = "/images/placeholder.jpg" },
            new Bus { Id = 3, Number = "003", Route = "University - Shopping Mall", Capacity = 30, ImageUrl = "/images/placeholder.jpg" }
        };
        public ActionResult Bus()
        {
            return View(_buses);
        }
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
    }
    
}