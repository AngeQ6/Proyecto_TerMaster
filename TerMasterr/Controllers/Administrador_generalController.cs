using System;
using System.Collections.Generic;
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

namespace TerMasterr.Controllers
{
    public class Administrador_generalController : Controller
    {
        private readonly Conexion _context;

        public Administrador_generalController()
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
        ///////////////////////////////////// VISTAS ///////////////////////////////////
        #region
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Registrar_administrador_local()
        {
            return View();
        }
        public ActionResult Pueblos()
        {
            var pueblos = _context.GetCollection<Pueblo>("Pueblo").Find(c => true).ToList();
            var adminlocal = _context.GetCollection<Admin_local>("Admin_local").Find(c => true).ToList();

            ViewBag.adminlocales = adminlocal;
            return View(pueblos);
        }
        public ActionResult Reportes()
        {
            return View();
        }
        public ActionResult QR()
        {
            return View();
        }
        public ActionResult Buses(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var collection = _context.GetCollection<Asistencia>("Asistencia");

            var filtro = collection.Find(b => true);

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                filtro = collection.Find(b => b.FechaIngreso >= fechaInicio.Value && b.FechaIngreso <= fechaFin.Value);
            }

            var buses = filtro.ToList();

            return View(buses);
        }
        public ActionResult Editar_datos_personales()
        {
            return View();
        }
        public ActionResult Gestion_admin_local()
        {
            var admin_local = _context.GetCollection<Admin_local>("Admin_local").Find(c => true).ToList();
            return View(admin_local);
        }
        //////////////////////////////////////////////////////////////////////////////////////
        #endregion

        /////////////////////////// METODOS /////////////////////////////////////////////

        #region
        public ActionResult Generar_QR(bool download = false)
        {
            string qrContent = "https://192.168.1.3:45455/Conductor/RegistrarAsistencia";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(15);

            if (download)
            {
                return File(qrCodeImage, "image/png", "qr_code.png");
            }

            return File(qrCodeImage, "image/png");
        }




        [HttpPost]
        public async Task<ActionResult> Registrar_administrador_local(int id_admin_local, string nombre_admin_local, string apellido_admin_local, string correo_admin_local, long telefono_admin_local, int id_pueblo, string estado)
        {
            try
            {
                var conexion = new Conexion();
                var coleccion_admi_local = conexion.GetCollection<Admin_local>("Admin_local");

                // Validar si el administrador local ya existe
                var adminExistente = await coleccion_admi_local.Find(c => c.id_admin_local == id_admin_local).FirstOrDefaultAsync();

                if (adminExistente != null)
                {
                    TempData["ErrorMessage"] = "El administrador local con este ID ya existe. Por favor, use otro ID.";
                    return RedirectToAction("Registrar_administrador_local", "Administrador_general");
                }

                string contraseñaGenerada = GenerarContraseña();
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

                await coleccion_admi_local.InsertOneAsync(nuevo_admin_local);

                // Enviar la contraseña generada por correo electrónico
                //await EnviarCorreoAsync(correo_admin_local, contraseñaGenerada);

                // Si la inserción y el envío de correo son exitosos, almacenar un mensaje de éxito en TempData
                TempData["SuccessMessage"] = "Registro exitoso. Se ha enviado una contraseña generada al correo proporcionado.";
                return View("Registrar_administrador_local");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al registrar el administrador local: " + ex.Message;
                return RedirectToAction("Registrar_administrador_local", "Administrador_general");
            }
        }

        [HttpPost]
        public async Task<JsonResult> Validar_id_pueblo(int id_pueblo)
        {
            try
            {
                var conexion = new Conexion();
                var coleccionPueblo = conexion.GetCollection<Pueblo>("Pueblo");
                var puebloExistente = await coleccionPueblo.Find(p => p.id_pueblo == id_pueblo).FirstOrDefaultAsync();

                if (puebloExistente != null)
                {
                    return Json(new { existe = true });
                }
                else
                {
                    return Json(new { existe = false });
                }
            }
            catch (Exception ex)
            {
                return Json(new { existe = false, error = ex.Message });
            }
        }




        private string GenerarContraseña(int longitud = 6)
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
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

        
        [HttpPost]
        public ActionResult Agregar_pueblo(Pueblo pueblo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si ya existe un pueblo con el mismo nombre
                    var pueblo_existente = _context.GetCollection<Pueblo>("Pueblo")
                        .Find(b => b.nombre_pueblo == pueblo.nombre_pueblo)
                        .FirstOrDefault();

                    if (pueblo_existente != null)
                    {
                        TempData["ErrorMessage"] = "Ya existe un pueblo registrado con ese nombre.";
                        return RedirectToAction("Pueblos");
                    }

                    // Verificar si el administrador ya tiene un pueblo asignado
                    var administradorConPueblo = _context.GetCollection<Pueblo>("Pueblo")
                        .Find(p => p.id_admin_local == pueblo.id_admin_local)
                        .FirstOrDefault();

                    if (administradorConPueblo != null)
                    {
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



        [HttpGet]
        public ActionResult Get_puebloById(int id)
        {
            var pueblo = _context.GetCollection<Pueblo>("Pueblo")
                                    .Find(c => c.id_pueblo == id)
                                    .FirstOrDefault();
            if (pueblo == null)
            {
                return HttpNotFound();
            }

            return Json(new
            {
                id_pueblo = pueblo.id_pueblo,
                nombre_pueblo = pueblo.nombre_pueblo,
                id_admin_local = pueblo.id_admin_local,
                nombre_admin_local = pueblo.nombre_admin_local
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Editar_pueblo(Pueblo pueblo)
        {
            var puelo_existente = _context.GetCollection<Pueblo>("Pueblo")
                                              .Find(c => c.id_pueblo == pueblo.id_pueblo)
                                              .FirstOrDefault();

            if (puelo_existente != null)
            {
                //Campos que se van a editar
                puelo_existente.nombre_pueblo = pueblo.nombre_pueblo;
                puelo_existente.id_admin_local = pueblo.id_admin_local;
                puelo_existente.nombre_admin_local = pueblo.nombre_admin_local;

                TempData["ErrorMessage_EditPueblo"] = "Error.";
                _context.GetCollection<Pueblo>("Pueblo").ReplaceOne(c => c.id_pueblo == pueblo.id_pueblo, puelo_existente);
            }
            TempData["SuccessMessage_EditPueblo"] = "Actualizacion exitosa.";
            return RedirectToAction("Pueblos");
        }

        [HttpGet]
        public JsonResult Obtener_datos_admin_general()
        {
            try
            {
                // Supongamos que obtienes el ID del conductor desde la sesión
                int idadmingeneral = (int)Session["id_admin_general"]; // Asegúrate de que este ID esté correctamente almacenado en la sesión

                // Crear una instancia de la clase Conexion
                Conexion conexion = new Conexion();

                // Obtener la colección de conductores
                var coleccionAdmingeneral = conexion.GetCollection<AdminG>("AdminG");

                // Buscar el conductor por ID
                var admingeneral = coleccionAdmingeneral.Find(c => c.id_admin_general == idadmingeneral).FirstOrDefault();

                if (admingeneral == null)
                {
                    return Json(new { success = false, message = "Administrador general no encontrado" }, JsonRequestBehavior.AllowGet);
                }

                // Devolver los datos del conductor como JSON
                return Json(new
                {
                    success = true,
                    id_admin_general = admingeneral.id_admin_general,
                    nombre_admin_general = admingeneral.nombre_admin_general,
                    correo_admin_general = admingeneral.correo_admin_general,
                    telefono_admin_general = admingeneral.telefono_admin_general
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Manejar excepciones y devolver un mensaje de error
                return Json(new { success = false, message = "Error al obtener los datos del Administrador general: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Modificar_datos_admin_general(AdminG updatedConductor)
        {
            if (Session["id_admin_general"] != null)
            {
                int id_admingeneral = (int)Session["id_admin_general"];

                // Crear el filtro para buscar por el campo id_admin_general
                var filter = Builders<AdminG>.Filter.Eq(c => c.id_admin_general, id_admingeneral);

                // Crear la actualización sin modificar el campo _id
                var update = Builders<AdminG>.Update
                                             .Set(c => c.nombre_admin_general, updatedConductor.nombre_admin_general)
                                             .Set(c => c.correo_admin_general, updatedConductor.correo_admin_general)
                                             .Set(c => c.telefono_admin_general, updatedConductor.telefono_admin_general);

                var result = _context.GetCollection<AdminG>("AdminG").UpdateOne(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
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