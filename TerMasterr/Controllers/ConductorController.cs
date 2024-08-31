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
        ///////////////////////////////////////// CONEXION /////////////////////////////////////////
<<<<<<< HEAD
=======
        #region
>>>>>>> 1bf7e240fcb36336a4e986c54d2f83dcb828f6fb
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
        //////////////////////////////////////////////////////////////////////////////////
<<<<<<< HEAD
        


        ////////////////////////// VISAS /////////////////////////////////////////
        // GET: Conductor
=======
        #endregion


        ////////////////////////// VISAS /////////////////////////////////////////
        #region
>>>>>>> 1bf7e240fcb36336a4e986c54d2f83dcb828f6fb
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Modificar_datos_personales(int? id_conductor)
        {
            return View();
        }
<<<<<<< HEAD
        public ActionResult Reg_huella(int id_conductor)
        {
            ViewBag.IdConductor = id_conductor;
            return View();
        }
        //////////////////////////////////////////////////////////////////////////////////


        ///////////////////////////////////////// METODOS /////////////////////////////////////////

        public class HuellaRequest
        {
            public byte[] HuellaDactilar { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult> RegistrarHuella(int id_conductor, HuellaRequest request)
        {
            if (request?.HuellaDactilar == null)
            {
                return Json(new { success = false, message = "Datos de huella inválidos" });
            }

            try
            {
                var conductoresCollection = _context.GetCollection<Conductor>("Conductores");
                var filter = Builders<Conductor>.Filter.Eq(c => c.id_conductor, id_conductor);
                var update = Builders<Conductor>.Update.Set(c => c.huella_dactilar, request.HuellaDactilar);

                var result = await conductoresCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return Json(new { success = true, message = "Huella dactilar registrada con éxito" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo actualizar la huella dactilar" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al registrar la huella: " + ex.Message });
            }
        }
=======
        //////////////////////////////////////////////////////////////////////////////////
        #endregion



        ///////////////////////////////////////// METODOS /////////////////////////////////////////
        #region
        
>>>>>>> 1bf7e240fcb36336a4e986c54d2f83dcb828f6fb

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
<<<<<<< HEAD
            if (qrContent != "https://192.168.1.4:45455/Conductor/RegistrarAsistencia")
=======
            if (qrContent != "https://goodaquadog63.conveyor.cloud/Conductor/RegistrarAsistencia")
>>>>>>> 1bf7e240fcb36336a4e986c54d2f83dcb828f6fb
            {
                return Json(new { success = false, message = "QR incorrecto. Asegúrese de escanear el código correcto." }, JsonRequestBehavior.AllowGet);
            }

            // Verificar que el conductor esté autenticado
            if (Session["id_conductor"] == null)
            {
                return Json(new { success = false, message = "No se pudo registrar la asistencia. Por favor, intente iniciar sesión de nuevo." }, JsonRequestBehavior.AllowGet);
            }

            // Obtener el ID del conductor desde la sesión
            int idConductor = Convert.ToInt32(Session["id_conductor"]);

            // Usar _context para obtener la colección de conductores y asistencias
            var conductoresCollection = _context.GetCollection<Conductor>("Conductor");
            var asistenciasCollection = _context.GetCollection<Asistencia>("Asistencia");

            // Obtener la información del conductor desde la colección usando el modelo Conductor
            var filtroConductor = Builders<Conductor>.Filter.Eq(c => c.id_conductor, idConductor);
            var conductor = conductoresCollection.Find(filtroConductor).FirstOrDefault();

            if (conductor == null)
            {
                return Json(new { success = false, message = "Conductor no encontrado en la base de datos." }, JsonRequestBehavior.AllowGet);
            }

            var placaBus = conductor.placa_bus_asignado;  // Asegúrate de tener este campo en tu modelo Conductor

            // Obtener la zona horaria de Colombia
            TimeZoneInfo colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

            // Obtener la fecha y hora actual en Colombia
            DateTime fechaActualColombia = TimeZoneInfo.ConvertTime(DateTime.Now, colombiaTimeZone);

            // Convertir a UTC para almacenar en MongoDB
            DateTime fechaActualUTC = TimeZoneInfo.ConvertTimeToUtc(fechaActualColombia, colombiaTimeZone);

            // Verificar si ya existe un registro de entrada sin salida para este conductor
            var filtroAsistencia = Builders<Asistencia>.Filter.And(
                Builders<Asistencia>.Filter.Eq(a => a.IdConductor, idConductor),
                Builders<Asistencia>.Filter.Eq(a => a.FechaSalida, null)
            );

            var registroExistente = asistenciasCollection.Find(filtroAsistencia).FirstOrDefault();

            if (registroExistente == null)
            {
                // Obtener el siguiente valor del ID autoincrementado
                int nuevoIdAsistencia = _context.GetNextSequenceValue("Asistencia");

                // Registrar nueva entrada
                var nuevaAsistencia = new Asistencia
                {
                    IdAsistencia = nuevoIdAsistencia,
                    FechaIngreso = fechaActualUTC,  // Usar la fecha ajustada a Colombia
                    IdConductor = idConductor,
                    PlacaBus = placaBus
                };

                asistenciasCollection.InsertOne(nuevaAsistencia);

                return Json(new { success = true, message = "Entrada registrada correctamente", hora = fechaActualColombia.Hour, minuto = fechaActualColombia.Minute, esPM = fechaActualColombia.Hour >= 12 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // Actualizar el registro existente con la fecha de salida
                var update = Builders<Asistencia>.Update.Set(a => a.FechaSalida, fechaActualUTC);  // Usar la fecha ajustada a Colombia
                asistenciasCollection.UpdateOne(filtroAsistencia, update);

                return Json(new { success = true, message = "Salida registrada correctamente", hora = fechaActualColombia.Hour, minuto = fechaActualColombia.Minute, esPM = fechaActualColombia.Hour >= 12 }, JsonRequestBehavior.AllowGet);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////
<<<<<<< HEAD

=======
        #endregion
>>>>>>> 1bf7e240fcb36336a4e986c54d2f83dcb828f6fb
    }
}
