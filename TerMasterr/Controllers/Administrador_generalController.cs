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
            string qrContent = "https://otherminttower78.conveyor.cloud/Aprendiz/RegistrarAsistencia";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(15);

            return File(qrCodeImage, "image/png");
        }

        [HttpPost]
        public async Task<ActionResult> Registrar_administrador_local(int id_admin_local, string nombre_admin_local, string apellido_admin_local, string correo_admin_local, int telefono_admin_local)
        {
            try
            {
                var conexion = new Conexion();
                var coleccionConductores = conexion.GetCollection<Admin_local>("Admin_local");

                // Validar si el conductor ya existe
                var conductorExistente = await coleccionConductores.Find(c => c.id_admin_local == id_admin_local).FirstOrDefaultAsync();

                if (conductorExistente != null)
                {
                    // Si el conductor ya existe, mostrar un mensaje de error
                    TempData["ErrorMessage"] = "El administrador local con este ID ya existe. Por favor, use otro ID.";
                    return RedirectToAction("Registrar_administrador_local", "Administrador_general");
                }

                // Generar una contraseña aleatoria
                string contraseñaGenerada = GenerarContraseña();

                // Si el conductor no existe, proceder con el registro
                var nuevo_conductor = new Admin_local
                {
                    id_admin_local = id_admin_local,
                    nombre_admin_local = nombre_admin_local,
                    apellido_admin_local = apellido_admin_local,
                    correo_admin_local = correo_admin_local,
                    telefono_admin_local = telefono_admin_local,
                    contraseña_admin_local = contraseñaGenerada
                };

                await coleccionConductores.InsertOneAsync(nuevo_conductor);

                // Si la inserción es exitosa, almacenar un mensaje de éxito en TempData
                TempData["SuccessMessage"] = "Registro exitoso. Contraseña generada: " + contraseñaGenerada;

                // Devolver la vista de registro para mostrar el mensaje
                return View("Registrar_administrador_local");
            }
            catch (Exception ex)
            {
                // Manejo de errores: almacenar el mensaje de error en TempData
                TempData["ErrorMessage"] = "Ocurrió un error al registrar el administrador: " + ex.Message;

                // Redirigir al usuario de vuelta al formulario de registro para que intente nuevamente
                //return RedirectToAction("Registrar_admin_local", "Administrador_general");
                return View("Registrar_administrador_local");
            }
        }

        // Método para generar una contraseña aleatoria
        private string GenerarContraseña(int longitud = 12)
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(caracteres, longitud)
                                        .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}