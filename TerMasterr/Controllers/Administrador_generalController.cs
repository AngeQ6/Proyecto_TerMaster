using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Capa_entidad;
using ConexionMongoDB;
using iTextSharp.text.pdf;
using MongoDB.Driver;
using QRCoder;
//using SendGrid;
//using SendGrid.Helpers.Mail;

namespace TerMasterr.Controllers
{
    public class Administrador_generalController : Controller
    {
        private readonly Conexion _context; // Objeto con el que se maneja la conexion a la base de datos obtenida de la clase Conexion

        public Administrador_generalController() // Constructor del controlador para establecer la conexión con la base de datos
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
        ///////////////////////////////////// VISTAS ///////////////////////////////////
        #region

        // Vista para registrar administradores locales
        public ActionResult Registrar_administrador_local()
        {
            return View();
        }
        // Vista para gestionar pueblos
        public ActionResult Pueblos()
        {
            // Obtiene la lista de pueblos y administradores locales desde la base de datos
            var pueblos = _context.GetCollection<Pueblo>("Pueblo").Find(c => true).ToList();
            var adminlocal = _context.GetCollection<Admin_local>("Admin_local").Find(c => true).ToList();

            // Pasar la lista de administradores locales a la vista
            ViewBag.adminlocales = adminlocal;
            return View(pueblos);
        }
        // Vista para los reportes de ingreso y salida de buses
        public ActionResult Reportes()
        {
            return View();
        } 
        // Vista para la visualización del código con el que se registrará la asistencia QR
        public ActionResult QR()
        {
            return View();
        }
        // Vista para la gestión de buses que ingresaron a la terminal
        public ActionResult Buses(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var collection = _context.GetCollection<Asistencia>("Asistencia"); // Obtener colección Asistencia de la base de datos
            var filtro = collection.Find(b => true); // Crea un filtro básico que obtiene todas las asistencias

            if (fechaInicio.HasValue && fechaFin.HasValue) // Condición que se cumple si se han proporcionado fechas para filtrarlas por rango de fechas
            {
                filtro = collection.Find(b => b.FechaIngreso >= fechaInicio.Value && b.FechaIngreso <= fechaFin.Value);
            }

            // Convierte el filtro en una lista de buses y los envía a la vista
            var buses = filtro.ToList();
            return View(buses);
        }
        // Vista para editar los datos personales del administrador general
        public ActionResult Editar_datos_personales()
        {
            return View();
        }
        // Vista para la gestión de administradores locales
        public ActionResult Gestion_admin_local()
        {
            // Obtiene la lista de administradores locales para llevarlos a la vista
            var admin_local = _context.GetCollection<Admin_local>("Admin_local").Find(c => true).ToList();
            return View(admin_local);
        }
        //////////////////////////////////////////////////////////////////////////////////////
        #endregion

        /////////////////////////// METODOS /////////////////////////////////////////////

        #region

        // Método para generar un código QR
        public ActionResult Generar_QR(bool download = false) 
        {
            string qrContent = "https://192.168.1.4:45455/Conductor/RegistrarAsistencia"; // Contenido del QR el cual puede ser cambiado dependiendo de la URL que nos proporcione la herramienta conveyor
            QRCodeGenerator qrGenerator = new QRCodeGenerator(); // Objeto de la función para generar QR
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q); 
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(15); // Tamaño del QR

            if (download) // Condición que se cumple si el QR es descargado por el usuario
            {
                return File(qrCodeImage, "image/png", "qr_code.png"); // Retorna el QR en un archivo .png 
            }

            return File(qrCodeImage, "image/png"); // Retorna la imagen del QR para la vista
        }

        // Método para registrar a los administradores locales 
        [HttpPost]
        public async Task<ActionResult> Registrar_administrador_local(int id_admin_local, string nombre_admin_local, string apellido_admin_local, string correo_admin_local, long telefono_admin_local, int id_pueblo, string estado) // Datos del administrador local
        {
            try
            {
                var conexion = new Conexion(); // Abre una nueva cnexión a la base de datos
                var coleccion_admi_local = conexion.GetCollection<Admin_local>("Admin_local"); // Obtiene la colección de administradores locales

                // Validar si el administrador local ya existe
                var adminExistente = await coleccion_admi_local.Find(c => c.id_admin_local == id_admin_local).FirstOrDefaultAsync();

                if (adminExistente != null) // Condición que se cumple si el administrador local ya existe 
                {
                    // Mensaje que se muestra en la vista indicando que el administrador local especificado ya existe
                    TempData["ErrorMessage"] = "El administrador local con este ID ya existe. Por favor, use otro ID.";
                    return RedirectToAction("Registrar_administrador_local", "Administrador_general");
                }

                string contraseñaGenerada = GenerarContraseña(); // Método para generar una nueva contraseña para el administrador local
                // Registrar nuevo administrador local
                var nuevo_admin_local = new Admin_local 
                {
                    id_admin_local = id_admin_local,
                    nombre_admin_local = nombre_admin_local,
                    apellido_admin_local = apellido_admin_local,
                    correo_admin_local = correo_admin_local,
                    telefono_admin_local = telefono_admin_local,
                    contraseña_admin_local = contraseñaGenerada, 
                    estado = estado, 
                    id_pueblo = id_pueblo
                };

                await coleccion_admi_local.InsertOneAsync(nuevo_admin_local); // Insertar el nuevo administrador local en la base de datos

                // Enviar la contraseña generada por correo electrónico
                //await EnviarCorreoAsync(correo_admin_local, contraseñaGenerada);

                // Si la inserción y el envío de correo son exitosos, almacenar un mensaje de éxito en TempData
                TempData["SuccessMessage_Reg_admin_local"] = "Registro exitoso. Se ha enviado una contraseña generada al correo proporcionado.";
                return View("Registrar_administrador_local");
            }
            catch (Exception ex)
            {
                // Manejo de cualquier error que pueda ocurrir durante el proceso de registro
                TempData["ErrorMessage_Reg_admin_local"] = "Ocurrió un error al registrar el administrador local: " + ex.Message;
                return RedirectToAction("Registrar_administrador_local", "Administrador_general");
            }
        }

        // Método para validar el id del pueblo
        [HttpPost]
        public async Task<JsonResult> Validar_id_pueblo(int id_pueblo) 
        {
            try
            {
                var conexion = new Conexion(); // Abre una nueva conexión a la base de datos
                var coleccionPueblo = conexion.GetCollection<Pueblo>("Pueblo"); // Obtener la colecciónde pueblos
                var puebloExistente = await coleccionPueblo.Find(p => p.id_pueblo == id_pueblo).FirstOrDefaultAsync(); 

                if (puebloExistente != null) // Condición que se cumple si el pueblo existe
                {
                    return Json(new { existe = true });
                }
                else // Condición que se cumple si el pueblo no existe
                {
                    return Json(new { existe = false });
                }
            }
            catch (Exception ex)
            {
                // Captura de errores que puedan suceder durante el proceso 
                return Json(new { existe = false, error = ex.Message });
            }
        }


        // Método para generar la contraseña de manera aleatoria
        private string GenerarContraseña(int longitud = 6) // Especificación de la cantidad de dígitos para la contraseña
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; // Se especifica los carácteres en los que se va a mostrar la contraseña
            var random = new Random(); // Función que permite escoger la contraseña de manera aleatoria 
            return new string(Enumerable.Repeat(caracteres, longitud)
                                        .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        //public async Task EnviarCorreoAsync(string destinatario, string contrasena)
        //{
        //    var apiKey = ConfigurationManager.AppSettings["SendGridApiKey"];
        //    var cliente = new SendGridClient(apiKey);
        //    var from = new EmailAddress("angelicaquintana06@hotmail.com", "TerMaster");
        //    var subject = "Tu nueva contraseña";
        //    var to = new EmailAddress(destinatario);
        //    var plainTextContent = $"Tu contraseña es: {contrasena}";
        //    var htmlContent = $"<strong>Tu contraseña es: {contrasena}</strong>";
        //    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        //    var response = await cliente.SendEmailAsync(msg);

        //}

        // Método para agregar a los pueblos
        [HttpPost]
        public ActionResult Agregar_pueblo(Pueblo pueblo) 
        { 
            if (ModelState.IsValid) // Condición que se cumple si el modelo de datos es válido 
            {
                try
                {
                    // Verificar si ya existe un pueblo con el mismo nombre 
                    var pueblo_existente = _context.GetCollection<Pueblo>("Pueblo") // Obtener la colección de los pueblos
                        .Find(b => b.nombre_pueblo == pueblo.nombre_pueblo)
                        .FirstOrDefault();

                    if (pueblo_existente != null) // Condición que se cumple si el pueblo ya existe
                    {
                        // Mensaje que indica la existencia del pueblo e impide el registro duplicado
                        TempData["ErrorMessage"] = "Ya existe un pueblo registrado con ese nombre.";
                        return RedirectToAction("Pueblos");
                    }

                    // Verificar si el administrador ya tiene un pueblo asignado
                    var administradorConPueblo = _context.GetCollection<Pueblo>("Pueblo") // Obtener la colección de pueblos
                        .Find(p => p.id_admin_local == pueblo.id_admin_local)
                        .FirstOrDefault();

                    if (administradorConPueblo != null) // Condición uqe se cumple si el administrador local ya tiene un pueblo asignado
                    {
                        // Mensaje que indica que el administrador local ya tiene un pueblo asignado e impide el registro de múltiples datos
                        TempData["ErrorMessage"] = "El administrador ya tiene un pueblo asignado.";
                        return RedirectToAction("Pueblos");
                    }

                    // Generar nuevo ID para el pueblo
                    int nuevoidpueblo = _context.GetNextSequenceValue("Pueblo");

                    // Crear el nuevo pueblo
                    var nuevopueblo = new Pueblo
                    {
                        id_pueblo = nuevoidpueblo,
                        nombre_pueblo = pueblo.nombre_pueblo,
                        id_admin_local = pueblo.id_admin_local,
                        nombre_admin_local = pueblo.nombre_admin_local
                    };

                    // Insertar el nuevo pueblo en la colección
                    _context.GetCollection<Pueblo>("Pueblo").InsertOne(nuevopueblo);

                    TempData["SuccessMessage_RegPueblo"] = "Pueblo agregado exitosamente.";
                    return RedirectToAction("Pueblos");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error al agregar el pueblo: " + ex.Message;
                    return RedirectToAction("Pueblos");
                }
            }

            // Si el modelo no es válido, redirigir a la vista 'Pueblos' con el modelo actual
            TempData["ErrorMessage"] = "Datos inválidos. Por favor, verifica la información ingresada.";
            return RedirectToAction("Pueblos");
        }


        // Obtener los datos del pueblo
        [HttpGet]
        public ActionResult Get_puebloById(int id)
        {
            var pueblo = _context.GetCollection<Pueblo>("Pueblo") // Obtener la colección de pueblos
                                    .Find(c => c.id_pueblo == id) 
                                    .FirstOrDefault();
            if (pueblo == null) // Condición que se cumple si no hay pueblos
            {
                return HttpNotFound();
            }
            // Condición que se cumple si hay pueblos y se obtiene los datos del pueblo especificado
            return Json(new
            {
                id_pueblo = pueblo.id_pueblo,
                nombre_pueblo = pueblo.nombre_pueblo,
                id_admin_local = pueblo.id_admin_local,
                nombre_admin_local = pueblo.nombre_admin_local
            }, JsonRequestBehavior.AllowGet);
        }

        // Método para editar a los pueblos
        [HttpPost]
        public ActionResult Editar_pueblo(Pueblo pueblo)
        { 
            var puelo_existente = _context.GetCollection<Pueblo>("Pueblo") // Obtener la colección de los pueblos
                                              .Find(c => c.id_pueblo == pueblo.id_pueblo)
                                              .FirstOrDefault();

            if (puelo_existente != null) // Condición que se cumple si el pueblo especificado existe en la base de datos
            {
                //Campos que se van a editar
                puelo_existente.nombre_pueblo = pueblo.nombre_pueblo;
                puelo_existente.id_admin_local = pueblo.id_admin_local;
                puelo_existente.nombre_admin_local = pueblo.nombre_admin_local;

                TempData["ErrorMessage_EditPueblo"] = "Error.";
                _context.GetCollection<Pueblo>("Pueblo").ReplaceOne(c => c.id_pueblo == pueblo.id_pueblo, puelo_existente);
            }
            // Mensaje que indica que la actualización de los datos ha sido exitosa
            TempData["SuccessMessage_EditPueblo"] = "Actualizacion exitosa.";
            return RedirectToAction("Pueblos");
        }

        // Método para obtener los datos del administrador general
        [HttpGet]
        public JsonResult Obtener_datos_admin_general()
        {
            try
            {
                // Ontener el ID de la sesión
                int idadmingeneral = (int)Session["id_admin_general"]; // ID de la sesión especificado en el método Login del controlador Login

                // Crear una instancia de la clase Conexion
                Conexion conexion = new Conexion();

                // Obtener la colección de administradores generales
                var coleccionAdmingeneral = conexion.GetCollection<AdminG>("AdminG");

                // Buscar el administrador por ID
                var admingeneral = coleccionAdmingeneral.Find(c => c.id_admin_general == idadmingeneral).FirstOrDefault();

                if (admingeneral == null) // Condición que se cumple si el administrador general no se encuentra en la base de datos
                {
                    // Mensaje que infica que administrador general no fue encontrado 
                    return Json(new { success = false, message = "Administrador general no encontrado" }, JsonRequestBehavior.AllowGet);
                }

                // Devolver los datos del administrador como JSON
                return Json(new
                {
                    success = true,
                    id_admin_general = admingeneral.id_admin_general,
                    nombre_admin_general = admingeneral.nombre_admin_general,
                    correo_admin_general = admingeneral.correo_admin_general,
                    contraseña_admin_general = admingeneral.contraseña_admin_general,
                    telefono_admin_general = admingeneral.telefono_admin_general
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Manejar excepciones y devolver un mensaje de error
                return Json(new { success = false, message = "Error al obtener los datos del Administrador general: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Método para editar los datos personales del administrador general
        [HttpPost]
        public JsonResult Modificar_datos_admin_general(AdminG updatedConductor)
        {
            if (Session["id_admin_general"] != null) // Condición que se cumple si el id de la sesión ha sido autenticada
            {
                int id_admingeneral = (int)Session["id_admin_general"]; // Obtiene el id de la sesión

                // Crear el filtro para buscar por el campo id_admin_general
                var filter = Builders<AdminG>.Filter.Eq(c => c.id_admin_general, id_admingeneral);

                // Crear la actualización sin modificar el campo _id
                var update = Builders<AdminG>.Update
                                             .Set(c => c.nombre_admin_general, updatedConductor.nombre_admin_general)
                                             .Set(c => c.correo_admin_general, updatedConductor.correo_admin_general)
                                             .Set(c => c.contraseña_admin_general, updatedConductor.contraseña_admin_general)
                                             .Set(c => c.telefono_admin_general, updatedConductor.telefono_admin_general);

                var result = _context.GetCollection<AdminG>("AdminG").UpdateOne(filter, update); // Obtiene la colección para actualizarla con los nuevos datos

                if (result.ModifiedCount > 0) // Condición que se cumple si el resultado es correcto
                {
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }

        // Método para descaegar los reportes de la asistencia de buses
        public ActionResult DescargarReportePDF(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var collection = _context.GetCollection<Asistencia>("Asistencia"); // Obtener la colección Asistencia 

            var filtro = collection.Find(b => true);

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                filtro = collection.Find(b => b.FechaIngreso >= fechaInicio.Value && b.FechaIngreso <= fechaFin.Value);
            }

            var buses = filtro.ToList();

            using (var stream = new MemoryStream())
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
        [HttpGet]
        public ActionResult Get_admin_localById(int id)
        {
            //Método de edición de estado de conductor completo
            var admin_local = _context.GetCollection<Admin_local>("Admin_local")
                                    .Find(c => c.id_admin_local == id)
                                    .FirstOrDefault();
            if (admin_local == null)
            {
                return HttpNotFound();
            }

            return Json(new
            {
                id_admin_local = admin_local.id_admin_local,
                nombre_admin_local = admin_local.nombre_admin_local,
                Estado = admin_local.estado
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Editar_admin_local(Admin_local admin_local)
        {
            var admin_localExistente = _context.GetCollection<Admin_local>("Admin_local")
                                              .Find(a => a.id_admin_local == admin_local.id_admin_local)
                                              .FirstOrDefault();

            if (admin_localExistente != null)
            {
                //Campos que se van a editar
                admin_localExistente.estado = admin_local.estado;

                _context.GetCollection<Admin_local>("Admin_local").ReplaceOne(a => a.id_admin_local == admin_local.id_admin_local, admin_localExistente);
            }
            return RedirectToAction("Gestion_admin_local");
        }

        #endregion
    }
}