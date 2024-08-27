﻿using MongoDB.Driver;
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
        [HttpGet]
        public JsonResult RegistrarAsistencia(string qrContent)
        {
            // Verificar que el contenido del QR sea el correcto
            if (qrContent != "https://192.168.1.4:45455/Conductor/RegistrarAsistencia")
            {
                return Json(new { success = false, message = "QR no detectado. Asegúrese de estar escaneando el código correcto." }, JsonRequestBehavior.AllowGet);
            }

            // Verificar que el conductor esté autenticado
            if (Session["IdConductor"] == null)
            {
                return Json(new { success = false, message = "No se pudo registrar la asistencia. Por favor, intente iniciar sesión de nuevo." }, JsonRequestBehavior.AllowGet);
            }

            // Obtener el ID del conductor desde la sesión
            var idConductor = Session["IdConductor"].ToString();

            // Usar _context para obtener la colección de conductores y asistencias
            var conductoresCollection = _context.GetCollection<BsonDocument>("Conductor");
            var asistenciasCollection = _context.GetCollection<BsonDocument>("Asistencia");

            // Obtener la información del conductor desde la colección para obtener la placa del bus asignado
            var filtroConductor = Builders<BsonDocument>.Filter.Eq("id_conductor", idConductor);
            var conductor = conductoresCollection.Find(filtroConductor).FirstOrDefault();

            if (conductor == null)
            {
                return Json(new { success = false, message = "Conductor no encontrado en la base de datos." }, JsonRequestBehavior.AllowGet);
            }

            var placaBus = conductor["placa_bus_asignado"].AsString;

            // Obtener la fecha actual
            var fechaActual = DateTime.Now;

            // Verificar si ya existe un registro de entrada sin salida para este conductor
            var filtroAsistencia = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("id_conductor", idConductor),  // Usar id_conductor en lugar de ObjectId
                Builders<BsonDocument>.Filter.Eq("fecha_salida", BsonNull.Value)
            );

            var registroExistente = asistenciasCollection.Find(filtroAsistencia).FirstOrDefault();

            if (registroExistente == null)
            {
                // Registrar nueva entrada
                var nuevoRegistro = new BsonDocument
            {
                { "fecha_ingreso", fechaActual },
                { "id_conductor", idConductor },  // Usar id_conductor en lugar de ObjectId
                { "placa_bus", placaBus }  // Usar la placa obtenida del conductor
            };

                asistenciasCollection.InsertOne(nuevoRegistro);

                return Json(new { success = true, message = "Entrada registrada correctamente." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // Actualizar el registro existente con la fecha de salida
                var update = Builders<BsonDocument>.Update.Set("fecha_salida", fechaActual);
                asistenciasCollection.UpdateOne(filtroAsistencia, update);

                return Json(new { success = true, message = "Salida registrada correctamente." }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
