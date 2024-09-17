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
using System.IO;

namespace TerMasterr.Controllers
{
   
    public class ConductorController : Controller
    {
        ///////////////////////////////////////// CONEXION /////////////////////////////////////////
        #region
        private readonly Conexion _context; // Objeto con el que se maneja la conexion a la base de datos obtenida de la clase Conexion
        public ConductorController() // Constructor del controlador para establecer la conexión con la base de datos
        {
            try
            {
                _context = new Conexion(); // Inicializa la conexión con MongoDB en este controlador
            }
            catch (ApplicationException ex)
            {
                // Captura cualquier error de conexión a la base de datos para ser mostrado en la vista
                ViewBag.ErrorMessage = "Error al conectar con la base de datos: " + ex.Message;
            }
        }
        //////////////////////////////////////////////////////////////////////////////////
        #endregion

        ////////////////////////// VISAS /////////////////////////////////////////
        #region
        // Vista para el escáner de QR
        public ActionResult Index()
        {
            return View();
        }
        // Vista para la modificación de datos personales del conductor
        public ActionResult Modificar_datos_personales(int? id_conductor)
        {
            return View();
        }
        //////////////////////////////////////////////////////////////////////////////////
        #endregion


        ///////////////////////////////////////// METODOS /////////////////////////////////////////
        #region
        
        // Método para obtener los datos del conductor
        public JsonResult Obtener_datos_conductor()
        {
            try
            {
                // Obtener el ID del conductor desde la sesión
                int idConductor = (int)Session["id_conductor"]; // Se usa el id obtenido en la sesión en el método Login

                // Crear una instancia de la clase Conexion
                Conexion conexion = new Conexion();

                // Obtener la colección de conductores
                var coleccionConductores = conexion.GetCollection<Conductor>("Conductor");

                // Buscar el conductor por ID
                var conductor = coleccionConductores.Find(c => c.id_conductor == idConductor).FirstOrDefault();

                if (conductor == null) // Condición que se cumple si el conductor no se encuentra
                {
                    //Mensaje que se devuelve en formato JSON si el conductor no es encontrado
                    return Json(new { success = false, message = "Conductor no encontrado" }, JsonRequestBehavior.AllowGet);
                }

                // Si se encuentra el conductor, devolver los datos en formato JSON
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

        // Método para modificar los datos personales
        [HttpPost]
        public ActionResult Modificar_datos_conductor(Conductor updatedConductor, HttpPostedFileBase imagenUrl)
        {
            try
            {
                if (Session["id_conductor"] != null) // Verificar que el conductor esté autenticado
                {
                    int id_conductor = (int)Session["id_conductor"]; // Obtener el ID del conductor desde la sesión

                    // Guardar la imagenUrl si fue cargada
                    string imageUrl = null;
                    if (imagenUrl != null && imagenUrl.ContentLength > 0)
                    {
                        // Definir la ruta para guardar la imagenUrl
                        string fileName = Path.GetFileName(imagenUrl.FileName);
                        string path = Path.Combine(Server.MapPath("~/Content/Images/"), fileName);

                        // Guardar el archivo en el servidor
                        imagenUrl.SaveAs(path);

                        // Guardar la URL de la imagenUrl
                        imageUrl = Url.Content(Path.Combine("~/Content/Images/", fileName));
                    }

                    // Crear filtro para identificar al conductor
                    var filter = Builders<Conductor>.Filter.Eq(c => c.id_conductor, id_conductor);

                    // Actualizar los datos del conductor
                    var update = Builders<Conductor>.Update
                        .Set(c => c.nombre, updatedConductor.nombre)
                        .Set(c => c.correo, updatedConductor.correo)
                        .Set(c => c.telefono, updatedConductor.telefono);

                    // Si se subió una nueva imagenUrl, actualizar el campo imagenUrlUrl
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        update = update.Set(c => c.ImagenUrl, imageUrl);
                    }

                    // Ejecutar la actualización en MongoDB
                    var result = _context.GetCollection<Conductor>("Conductor").UpdateOne(filter, update);

                    if (result.ModifiedCount > 0) // Condición que se cumple si todo es correcto para la actualización de los datos
                    {
                        // Mensaje de éxito en formato JSON
                        return Json(new { success = true, message = "Datos modificados correctamente" });
                    }
                }
                // Mensaje que se muestra si los datos no pudieron ser modificados
                return Json(new { success = false, message = "No se pudo modificar los datos" });
            }
            catch (Exception ex)
            {
                // Capturar cualquier error y devolver un mensaje de error
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Método para registrar asistencia escaneando el QR
        public JsonResult RegistrarAsistencia(string qrContent)
        {
            // Verificar que el contenido del QR sea el correcto
            if (qrContent != "https://192.168.1.4:45455/Conductor/RegistrarAsistencia") // La URL puede ser cambiada dependiendo de la URL que nos brinde la herramienta Conveyor
            {
                // Mensaje que se muestra si se intenta escanear un QR que no tenga el contenido anteriormente especificado
                return Json(new { success = false, message = "QR incorrecto. Asegúrese de escanear el código correcto." }, JsonRequestBehavior.AllowGet);
            }

            // Verificar que el conductor esté autenticado
            if (Session["id_conductor"] == null)
            {
                // Mensaje que se muestra si el conductor intenta registrar la asistencia sin haber iniciado sesión
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

            if (conductor == null) // Condición que se cumple si el conductor que intenta registrar la asistencia no se encuentra en la base de datos
            {
                // Mensaje para esta condición
                return Json(new { success = false, message = "Conductor no encontrado en la base de datos." }, JsonRequestBehavior.AllowGet);
            }

            var placaBus = conductor.placa_bus_asignado; // Obtén la placa asignada

            // Obtener la zona horaria de Colombia
            TimeZoneInfo colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

            // Obtener la fecha y hora actual en Colombia
            DateTime fechaActualColombia = TimeZoneInfo.ConvertTime(DateTime.Now, colombiaTimeZone);

            // Convertir a UTC para almacenar en MongoDB
            DateTime fechaActualUTC = TimeZoneInfo.ConvertTimeToUtc(fechaActualColombia, colombiaTimeZone);

            // Verificar si ya existe un registro de entrada sin salida para este conductor
            var filtroAsistencia = Builders<Asistencia>.Filter.And(
                Builders<Asistencia>.Filter.Eq(a => a.IdConductor, idConductor),
                Builders<Asistencia>.Filter.Eq(a => a.FechaSalida, null) // Verificar si no hay una fecha de salida registrada
            );

            var registroExistente = asistenciasCollection.Find(filtroAsistencia).FirstOrDefault();

            if (registroExistente == null) // Condición que se cumple si no hay registro de entrada sin salida
            {
                // Obtener el siguiente valor del ID autoincrementado
                int nuevoIdAsistencia = _context.GetNextSequenceValue("Asistencia");

                // Registrar nueva entrada
                var nuevaAsistencia = new Asistencia
                {
                    IdAsistencia = nuevoIdAsistencia,
                    FechaIngreso = fechaActualUTC,
                    IdConductor = idConductor,
                    PlacaBus = placaBus // Agregar la placa del bus
                };

                // Insertar la nueva asistencia en la colección
                asistenciasCollection.InsertOne(nuevaAsistencia);

                // Mensaje que muestra que la entrada fue registrada correctamente y especifica la hora de entrada en el mismo mensaje
                return Json(new { success = true, message = "Entrada registrada correctamente", hora = fechaActualColombia.Hour, minuto = fechaActualColombia.Minute, esPM = fechaActualColombia.Hour >= 12 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // Actualizar el registro existente con la fecha de salida si ya se registró una entrada
                var update = Builders<Asistencia>.Update
                    .Set(a => a.FechaSalida, fechaActualUTC);

                asistenciasCollection.UpdateOne(filtroAsistencia, update);

                // Mensaje que muestra que la salida fue registrada correctamente y especifica la hora de salida en el mismo mensaje
                return Json(new { success = true, message = "Salida registrada correctamente", hora = fechaActualColombia.Hour, minuto = fechaActualColombia.Minute, esPM = fechaActualColombia.Hour >= 12 }, JsonRequestBehavior.AllowGet);
            }
        }
        //////////////////////////////////////////////////////////////////////////////////
        #endregion
    }
}
