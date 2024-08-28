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
//using SendGrid;
//using SendGrid.Helpers.Mail;

namespace TerMasterr.Controllers
{
    public class Administrador_generalController : Controller
    {
        ///////////////////////////////////// VISTAS ///////////////////////////////////
        // GET: Administrador_general
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
            return View();
        }
        public ActionResult Reportes()
        {
            return View();
        }
        public ActionResult QR()
        {
            return View();
        }
        //////////////////////////////////////////////////////////////////////////////////////


        /////////////////////////// METODOS /////////////////////////////////////////////

        public ActionResult Generar_QR()
        {
            string qrContent = "https://192.168.1.4:45456/Conductor/RegistrarAsistencia";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(15);

            return File(qrCodeImage, "image/png");
        }

        

        [HttpPost]
        public async Task<ActionResult> Registrar_administrador_local(int id_admin_local, string nombre_admin_local, string apellido_admin_local, string correo_admin_local, long telefono_admin_local)
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
                    contraseña_admin_local = contraseñaGenerada
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
    }
}