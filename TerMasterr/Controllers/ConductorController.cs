using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Capa_entidad;
using ConexionMongoDB;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace TerMasterr.Controllers
{
   
    public class ConductorController : Controller
    {
        private readonly Conexion _context;
        public ConductorController()
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
        // GET: Conductor
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Modificar_datos_personales(int? id_conductor)
        {
            return View();
        }

        public JsonResult Obtener_datos_conductor()
        {
            try
            {
                // Supongamos que obtienes el ID del conductor desde la sesión
                int idConductor = (int)Session["id_conductor"]; // Asegúrate de que este ID esté correctamente almacenado en la sesión

                // Crear una instancia de la clase Conexion
                Conexion conexion = new Conexion();

                // Obtener la colección de conductores
                var coleccionConductores = conexion.GetCollection<Conductor>("Conductor");

                // Buscar el conductor por ID
                var conductor = coleccionConductores.Find(c => c.id_conductor == idConductor).FirstOrDefault();

                if (conductor == null)
                {
                    return Json(new { success = false, message = "Conductor no encontrado" }, JsonRequestBehavior.AllowGet);
                }

                // Devolver los datos del conductor como JSON
                return Json(new
                {
                    success = true,
                    id_conductor = conductor.id_conductor,
                    nombre = conductor.nombre,
                    correo = conductor.correo,
                    telefono = conductor.telefono
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Manejar excepciones y devolver un mensaje de error
                return Json(new { success = false, message = "Error al obtener los datos del conductor: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }




        [HttpPost]
        public JsonResult Modificar_datos_conductor(Conductor updatedConductor)
        {
            if (Session["id_conductor"] != null)
            {
                int id_conductor = (int)Session["id_conductor"];

                var filter = Builders<Conductor>.Filter.Eq(c => c.id_conductor, id_conductor);
                var update = Builders<Conductor>.Update
                                                .Set(c => c.nombre, updatedConductor.nombre)
                                                .Set(c => c.correo, updatedConductor.correo)
                                                .Set(c => c.telefono, updatedConductor.telefono);

                var result = _context.GetCollection<Conductor>("Conductor").UpdateOne(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }

    }
}
