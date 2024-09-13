using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Capa_entidad;
using ConexionMongoDB;
using iTextSharp.text.pdf;
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
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Bus() 
        {
            // Obtén el ID del pueblo desde la sesión
            var idPuebloStr = Session["PuebloId"]?.ToString(); //Obtener el id del pueblo de la sesión

            if (string.IsNullOrEmpty(idPuebloStr) || !int.TryParse(idPuebloStr, out int idPueblo)) // Validar que el id del pueblo no esté vacío y combierte el id del pueblo a int
            {
                return RedirectToAction("Login", "Login"); // Redirige si no hay ID de pueblo en la sesión o es inválido
            }

            // Filtrar conductores asociados al pueblo
            var conductoresEnPueblo = _context.GetCollection<Conductor>("Conductor")
                .Find(c => c.código_pueblo == idPueblo)
                .ToList();

            // Filtrar buses asociados a los conductores filtrados
            var idsConductoresEnPueblo = conductoresEnPueblo.Select(c => c.id_conductor).ToList();
            var busesFiltrados = _context.GetCollection<Bus>("Bus")
                .Find(b => idsConductoresEnPueblo.Contains(b.id_conductor))
                .ToList();

            ViewBag.Conductores = conductoresEnPueblo; // Enviamos la lista de conductores a la vista
            return View(busesFiltrados); // Enviamos la lista de buses a la vista
        }
        public ActionResult Gestion_conductor()
        {
            try
            {
                // Verificar que el usuario esté autenticado
                if (Session["PuebloId"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }

                // Obtener el ID del pueblo del administrador local
                int puebloId = (int)Session["PuebloId"];

                // Buscar los conductores que pertenecen al pueblo del administrador local
                var conductores = _context.GetCollection<Conductor>("Conductor")
                                          .Find(c => c.código_pueblo == puebloId)
                                          .ToList();

                return View(conductores);
            }
            catch (FormatException ex)
            {
                // Log the exception details
                TempData["ErrorMessage"] = "Error al deserializar los datos: " + ex.Message;
                // Redirect or handle as necessary
                return RedirectToAction("Error");
            }
            catch (Exception ex)
            {
                // Log the exception details
                TempData["ErrorMessage"] = "Error inesperado: " + ex.Message;
                // Redirect or handle as necessary
                return RedirectToAction("Error");
            }
        }

        public ActionResult Editar_datos_personales()
        {
            return View();
        }

        public ActionResult Reportes()
        {
            // Obtén el ID del pueblo desde la sesión
            var idPuebloStr = Session["PuebloId"]?.ToString();

            if (string.IsNullOrEmpty(idPuebloStr) || !int.TryParse(idPuebloStr, out int idPueblo))
            {
                return RedirectToAction("Login", "Login"); // Redirige si no hay ID de pueblo en la sesión o es inválido
            }

            // Filtrar conductores asociados al pueblo
            var conductoresEnPueblo = _context.GetCollection<Conductor>("Conductor")
                .Find(c => c.código_pueblo == idPueblo)
                .ToList();

            // Filtrar buses asociados a los conductores filtrados
            var idsConductoresEnPueblo = conductoresEnPueblo.Select(c => c.id_conductor).ToList();
            var busesFiltrados = _context.GetCollection<Bus>("Bus")
                .Find(b => idsConductoresEnPueblo.Contains(b.id_conductor))
                .ToList();

            // Filtrar registros de asistencia asociados a los buses filtrados
            var idsBusesFiltrados = busesFiltrados.Select(b => b.placa).ToList();
            var asistenciasFiltradas = _context.GetCollection<Asistencia>("Asistencia")
                .Find(a => idsBusesFiltrados.Contains(a.PlacaBus))
                .ToList();

            // Enviar datos a la vista
            ViewBag.Conductores = conductoresEnPueblo; // Lista de conductores
            ViewBag.Buses = busesFiltrados; // Lista de buses
            return View(asistenciasFiltradas); // Lista de asistencias

        }

        public ActionResult Buses(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var idPuebloStr = Session["PuebloId"]?.ToString();

            var collection = _context.GetCollection<Asistencia>("Asistencia");

            var filtro = collection.Find(b => true);

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                filtro = collection.Find(b => b.FechaIngreso >= fechaInicio.Value && b.FechaIngreso <= fechaFin.Value);
            }

            var buses = filtro.ToList();

            return View(buses);
        }

        ////////////////////////////////////////////////////////////////////////////
        #endregion


        ////////////////////////////////////// METODOS /////////////////////////////
        #region

        public ActionResult AddBus(Bus bus)
        {
            // Verifica si los datos del modelo son válidos antes de continuar.
            if (ModelState.IsValid)
            {
                try
                {
                    // Busca en la colección `Bus` si ya existe un bus con la misma placa.
                    var busExistente = _context.GetCollection<Bus>("Bus")
                        .Find(b => b.placa == bus.placa)
                        .FirstOrDefault();

                    // Si se encuentra un bus con la misma placa, se muestra un mensaje de error
                    // y se redirige al usuario de vuelta a la vista de buses.
                    if (busExistente != null)
                    {
                        TempData["ErrorMessage_RegBus"] = "Ya existe un bus registrado con la misma placa.";
                        return RedirectToAction("Bus");
                    }

                    // Busca al conductor relacionado con el ID del conductor asociado al bus.
                    var conductorParaActualizar = _context.GetCollection<Conductor>("Conductor")
                        .Find(c => c.id_conductor == bus.id_conductor)
                        .FirstOrDefault();

                    // Si el conductor es encontrado en la base de datos:
                    if (conductorParaActualizar != null)
                    {
                        // Verifica si el conductor ya tiene un bus asignado.
                        // Si el campo `placa_bus_asignado` no está vacío (tiene un valor), se evita asignar otro bus.
                        if (!string.IsNullOrEmpty(conductorParaActualizar.placa_bus_asignado))
                        {
                            // Si ya tiene un bus asignado, muestra un mensaje de error y redirige a la vista de buses.
                            TempData["ErrorMessage_RegBus"] = "Este conductor ya tiene un bus asignado.";
                            return RedirectToAction("Bus");
                        }

                        // Si no hay un bus asignado para el conductor, se agrega el nuevo bus a la colección `Bus`.
                        _context.GetCollection<Bus>("Bus").InsertOne(bus);

                        // Actualiza el campo `placa_bus_asignado` del conductor con la placa del nuevo bus.
                        conductorParaActualizar.placa_bus_asignado = bus.placa;

                        // Guarda los cambios del conductor en la base de datos, reemplazando el documento original
                        // con el conductor actualizado que ahora tiene la placa del bus.
                        _context.GetCollection<Conductor>("Conductor").ReplaceOne(
                            c => c.id_conductor == bus.id_conductor,
                            conductorParaActualizar
                        );

                        // Muestra un mensaje de éxito al agregar el bus y actualizar la placa del conductor.
                        TempData["SuccessMessage_RegBus"] = "Bus agregado exitosamente y placa registrada.";
                        return RedirectToAction("Bus");
                    }

                    // Si no se encuentra el conductor, se muestra un mensaje de error y se redirige a la vista de buses.
                    TempData["ErrorMessage_RegBus"] = "El conductor no fue encontrado.";
                    return RedirectToAction("Bus");
                }
                catch (Exception ex)
                {
                    // En caso de que ocurra cualquier error durante el proceso, captura la excepción
                    // y muestra un mensaje de error, luego redirige a la vista de buses.
                    TempData["ErrorMessage_RegBus"] = "Error al agregar el bus: " + ex.Message;
                    return RedirectToAction("Bus");
                }
            }

            // Si el modelo no es válido (por ejemplo, si faltan datos requeridos), se retorna la vista
            // con el modelo `bus` actual para mostrar los errores de validación.
            return View(bus);
        }


        public ActionResult Eliminar_bus(int id)
        {
            try
            {
                // Buscar el bus por su Id (usando id_conductor como referencia).
                var bus = _context.GetCollection<Bus>("Bus")
                    .Find(b => b.id_conductor == id)
                    .FirstOrDefault();

                // Si no se encuentra el bus, retornar un mensaje de error.
                if (bus == null)
                {
                    TempData["ErrorMessage_eliminacionBus"] = "No se encontró un bus con el ID proporcionado.";
                    return RedirectToAction("Bus");
                }

                // Eliminar el bus de la colección 'Bus'.
                _context.GetCollection<Bus>("Bus").DeleteOne(b => b.id_conductor == id);

                // Buscar el conductor asociado al bus eliminado.
                var conductorParaActualizar = _context.GetCollection<Conductor>("Conductor")
                    .Find(c => c.id_conductor == id)
                    .FirstOrDefault();

                // Si se encuentra el conductor, eliminar la placa asociada.
                if (conductorParaActualizar != null)
                {
                    // Establecer la placa del bus asignado a null (o cadena vacía) para desasociarlo del conductor.
                    conductorParaActualizar.placa_bus_asignado = null; // También puedes usar string.Empty si prefieres una cadena vacía.

                    // Actualizar el documento del conductor en la colección 'Conductor'.
                    _context.GetCollection<Conductor>("Conductor").ReplaceOne(
                        c => c.id_conductor == id,
                        conductorParaActualizar
                    );
                }

                // Mensaje de éxito si ambas operaciones (eliminación del bus y actualización del conductor) se completaron.
                TempData["SuccessMessage_eliminacionBus"] = "Bus eliminado exitosamente.";
                return RedirectToAction("Bus");
            }
            catch (Exception ex)
            {
                // En caso de error, se captura la excepción y se muestra un mensaje de error.
                TempData["ErrorMessage_eliminacionBus"] = "Error al eliminar el bus: " + ex.Message;
                return RedirectToAction("Bus");
            }
        }


        [HttpGet]
        public ActionResult Get_busByPlaca(string placa)
        {
            var bus = _context.GetCollection<Bus>("Bus")
                              .Find(c => c.placa == placa)
                              .FirstOrDefault();
            if (bus == null)
            {
                return HttpNotFound();
            }

            return Json(new
            {
                placa = bus.placa,
                id_conductor = bus.id_conductor,
                nombre = bus.nombre
            }, JsonRequestBehavior.AllowGet);
        }







        //[HttpPost]
        //public ActionResult Edit_bus(Bus bus)
        //{
        //    try
        //    {
        //        var busExistente = _context.GetCollection<Bus>("Bus")
        //                                   .Find(b => b.Id == bus.Id)  // Buscar el bus por su ObjectId
        //                                   .FirstOrDefault();

        //        if (busExistente != null)
        //        {
        //            // Actualizar los campos del bus
        //            var update = Builders<Bus>.Update
        //                                      .Set(b => b.placa, bus.placa)
        //                                      .Set(b => b.id_conductor, bus.id_conductor)
        //                                      .Set(b => b.nombre, bus.nombre);

        //            _context.GetCollection<Bus>("Bus").UpdateOne(b => b.Id == bus.Id, update);
        //        }

        //        return RedirectToAction("Bus", "Admin_local");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Manejar el error
        //        return RedirectToAction("ErrorPage", new { message = ex.Message });
        //    }
        //}

        //[HttpPost]
        //public ActionResult Edit_bus(string oldPlaca, string placa, int id_conductor, string nombre)
        //{
        //    try
        //    {
        //        // Buscar el bus por la placa
        //        var busExistente = _context.GetCollection<Bus>("Bus")
        //                                   .Find(b => b.placa == oldPlaca)
        //                                   .FirstOrDefault();

        //        if (busExistente != null)
        //        {
        //            // Actualizar los campos del bus
        //            var update = Builders<Bus>.Update
        //                                      .Set(b => b.placa, placa)
        //                                      .Set(b => b.id_conductor, id_conductor)
        //                                      .Set(b => b.nombre, nombre);

        //            _context.GetCollection<Bus>("Bus").UpdateOne(b => b.placa == oldPlaca, update);
        //        }

        //        return RedirectToAction("Bus", "Admin_local");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Manejar el error
        //        return RedirectToAction("ErrorPage", new { message = ex.Message });
        //    }
        //}

        [HttpPost]
        public ActionResult Edit_bus(string oldPlaca, string placa, int id_conductor, string nombre)
        {
            try
            {
                var busCollection = _context.GetCollection<Bus>("Bus");

                // Buscar el bus por la placa antigua
                var filter = Builders<Bus>.Filter.Eq(b => b.placa, oldPlaca);

                // Crear el objeto de actualización
                var update = Builders<Bus>.Update
                    .Set(b => b.placa, placa)
                    .Set(b => b.id_conductor, id_conductor)
                    .Set(b => b.nombre, nombre);

                // Realizar la actualización
                var result = busCollection.UpdateOne(filter, update);

                if (result.ModifiedCount == 0)
                {
                    // No se encontró el bus o no se realizaron cambios
                    TempData["ErrorMessage_EditBus"] = "No se pudo actualizar el bus. Verifica que la placa sea correcta.";
                    return RedirectToAction("Bus", "Admin_local");
                }

                TempData["SuccessMessage_EditBus"] = "Bus actualizado correctamente.";
                return RedirectToAction("Bus", "Admin_local");
            }
            catch (Exception ex)
            {
                // Manejar el error
                return RedirectToAction("ErrorPage", new { message = ex.Message });
            }
        }


        [HttpGet]
        public ActionResult Get_ConductorById(int id)
        { 
            //Método de edición de estado de conductor completo
            var conductor = _context.GetCollection<Conductor>("Conductor")
                                    .Find(c => c.id_conductor == id)
                                    .FirstOrDefault();
            if (conductor == null)
            {
                return HttpNotFound();
            }

            return Json(new
            {
                id_conductor = conductor.id_conductor,
                nombre = conductor.nombre,
                estado = conductor.Estado
            }, JsonRequestBehavior.AllowGet); 
        }
        [HttpPost]
        public ActionResult EditarConductor(Conductor conductor)
        {
            var conductorExistente = _context.GetCollection<Conductor>("Conductor")
                                              .Find(c => c.id_conductor == conductor.id_conductor)
                                              .FirstOrDefault();

            if (conductorExistente != null)
            {
                //Campos que se van a editar
                conductorExistente.Estado = conductor.Estado;

                _context.GetCollection<Conductor>("Conductor").ReplaceOne(c => c.id_conductor == conductor.id_conductor, conductorExistente);
            }
            return RedirectToAction("Gestion_conductor");
        }
        //public ActionResult EliminarConductor(int id)
        //{
        //    try
        //    {
        //        // Obtener la colección de conductores de la base de datos
        //        var conductorCollection = _context.GetCollection<Conductor>("Conductor");

        //        // Intentar eliminar el conductor por el campo id_conductor
        //        var result = conductorCollection.DeleteOne(c => c.id_conductor == id);

        //        // Verificar si se eliminó algún documento
        //        if (result.DeletedCount > 0)
        //        {
        //            TempData["SuccessMessage"] = "Conductor eliminado correctamente.";
        //        }
        //        else
        //        {
        //            TempData["ErrorMessage"] = "No se encontró el conductor con el ID especificado.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Error al eliminar el conductor: " + ex.Message;
        //    }

        //    // Redirigir a la acción para gestionar conductores
        //    return RedirectToAction("Gestion_conductor");
        //}

        
        public JsonResult Obtener_datos_admin_local()
        {
            try
            {
                // Verificar si el ID del admin local está en la sesión
                if (Session["id_admin_local"] == null)
                {
                    return Json(new { success = false, message = "No se encontró el ID del administrador local en la sesión." }, JsonRequestBehavior.AllowGet);
                }

                // Obtener el ID del admin local desde la sesión
                int id_admin_local = (int)Session["id_admin_local"];

                // Crear una instancia de la clase Conexion
                Conexion conexion = new Conexion();

                // Obtener la colección de administradores locales
                var coleccionAdminLocal = conexion.GetCollection<Admin_local>("Admin_local");

                // Buscar el administrador local por ID
                var admin_local = coleccionAdminLocal.Find(c => c.id_admin_local == id_admin_local).FirstOrDefault();

                if (admin_local == null)
                {
                    return Json(new { success = false, message = "Administrador local no encontrado." }, JsonRequestBehavior.AllowGet);
                }

                // Devolver los datos del administrador local como JSON
                return Json(new
                {
                    success = true,
                    id_admin_local = admin_local.id_admin_local,
                    nombre_admin_local = admin_local.nombre_admin_local,
                    apellido_admin_local = admin_local.apellido_admin_local,
                    correo_admin_local = admin_local.correo_admin_local,
                    telefono_admin_local = admin_local.telefono_admin_local
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Manejar excepciones y devolver un mensaje de error
                return Json(new { success = false, message = "Error al obtener los datos del administrador local: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpPost]
        public JsonResult Modificar_datos_admin_local(Admin_local updatedAdminLocal)
        {
            try
            {
                // Verificar si el ID del admin local está en la sesión
                if (Session["id_admin_local"] == null)
                {
                    return Json(new { success = false, message = "No se encontró el ID del administrador local en la sesión." });
                }

                int id_admin_local = (int)Session["id_admin_local"];

                // Crear filtro para buscar el administrador local
                var filter = Builders<Admin_local>.Filter.Eq(c => c.id_admin_local, id_admin_local);

                // Crear la actualización
                var update = Builders<Admin_local>.Update
                                                  .Set(c => c.nombre_admin_local, updatedAdminLocal.nombre_admin_local)
                                                  .Set(c => c.apellido_admin_local, updatedAdminLocal.apellido_admin_local)
                                                  .Set(c => c.correo_admin_local, updatedAdminLocal.correo_admin_local)
                                                  .Set(c => c.telefono_admin_local, updatedAdminLocal.telefono_admin_local);

                // Ejecutar la actualización
                var result = _context.GetCollection<Admin_local>("Admin_local").UpdateOne(filter, update);

                // Verificar si se realizó la actualización
                if (result.ModifiedCount > 0)
                {
                    return Json(new { success = true, message = "Datos actualizados correctamente." });
                }
                else
                {
                    return Json(new { success = false, message = "No se realizaron modificaciones." });
                }
            }
            catch (Exception ex)
            {
                // Manejar excepciones y devolver un mensaje de error
                return Json(new { success = false, message = "Error al modificar los datos: " + ex.Message });
            }
        }

        public ActionResult DescargarReportePDF(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var collection = _context.GetCollection<Asistencia>("Asistencia");

            var filtro = collection.Find(b => true);

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                filtro = collection.Find(b => b.FechaIngreso >= fechaInicio.Value && b.FechaIngreso <= fechaFin.Value);
            }

            var buses = filtro.ToList();

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                // Crear un documento PDF
                var pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();

                // Añadir título al PDF
                pdfDoc.Add(new iTextSharp.text.Paragraph("Reporte de Buses"));
                pdfDoc.Add(new iTextSharp.text.Paragraph(" ")); // Espacio en blanco

                // Crear una tabla
                PdfPTable table = new PdfPTable(4); // 4 columnas
                table.AddCell("Placa");
                table.AddCell("ID del Conductor");
                table.AddCell("Fecha de Ingreso");
                table.AddCell("Fecha de Salida");

                // Rellenar la tabla con los datos de los buses
                foreach (var bus in buses)
                {
                    table.AddCell(bus.PlacaBus);
                    table.AddCell(bus.IdConductor.ToString());
                    table.AddCell(bus.FechaIngreso.ToString("yyyy-MM-dd hh:mm:ss"));
                    table.AddCell(bus.FechaSalida?.ToString("yyyy-MM-dd hh:mm:ss") ?? "N/A");
                }

                pdfDoc.Add(table);
                pdfDoc.Close();

                var content = stream.ToArray();
                return File(content, "application/pdf", "ReporteBuses.pdf");
            }
        }
        /////////////////////////////////////////////////////////////////////////////
        #endregion


    }

}