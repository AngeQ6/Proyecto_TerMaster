using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Capa_entidad;
using ConexionMongoDB;
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
        public ActionResult Buses()
        {
            return View();
        }
        public ActionResult Editar_datos_personales()
        {
            return View();
        }
        //////////////////////////////////////////////////////////////////////////////////////
        #endregion

        /////////////////////////// METODOS /////////////////////////////////////////////

        #region
        public ActionResult Generar_QR()
        {
            string qrContent = "https://192.168.1.3:45455/Conductor/RegistrarAsistencia";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(15);
            return File(qrCodeImage, "image/png");
        }

        

        [HttpPost]
        public async Task<ActionResult> Registrar_administrador_local(int id_admin_local, string nombre_admin_local, string apellido_admin_local, string correo_admin_local, long telefono_admin_local, int id_pueblo)
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
        public ActionResult Validar_cod_pueblo(string id_pueblo)
        {
            bool isValid = false;
            try
            {
                // Convertir id_pueblo a entero si es necesario
                if (!int.TryParse(id_pueblo, out int codigoInt))
                {
                    TempData["MensajeError_CodPueblo"] = "Código inválido";
                    return RedirectToAction("Error", "Login");
                }

                // Obtener la colección desde MongoDB
                var collection = _context.GetCollection<Pueblo>("Pueblo");

                // Construir el filtro usando Builders<T>.Filter
                var filtro = Builders<Pueblo>.Filter.Eq("id_pueblo", codigoInt);
                var resultado = collection.Find(filtro).FirstOrDefault();

                // Verificar si el documento existe
                isValid = resultado != null;

                if (isValid)
                {
                    TempData["Mensaje_exito"] = $"Código válido para el pueblo {resultado.nombre_pueblo}"; //Mensaje con el nombre del pueblo al que se le validó el código
                    TempData["codigo_pueblo_validado"] = resultado.id_pueblo; // Guardar el código del pueblo validado en TempData
                    return View("Validar_cod_conductor"); // Devolver a la vista para mostrar el mensaje
                }
                else
                {
                    TempData["MensajeError_CodPueblo"] = "Código inválido";
                    return RedirectToAction("Validar_cod_conductor", "Login");
                }
            }
            catch (Exception ex)
            {
                // Manejar errores
                TempData["MensajeError_CodPueblo"] = $"Error: {ex.Message}";
                return RedirectToAction("Validar_cod_conductor", "Login");
            }
        }


        private string GenerarContraseña(int longitud = 12)
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
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

        #endregion
    }
}