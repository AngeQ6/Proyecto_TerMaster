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
    public class LoginController : Controller
    {
        /////////////////////////////////// CONEXION BD ////////////////////////////
        
        private readonly Conexion _context;
        public LoginController()
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
        /////////////////////////////////////////////////////////////////////////////////





        /////////////////////////////////////////////// VISTAS ///////////////////////////////
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Registro_conductor()
        {
            return View();
        }
        public ActionResult Recp_contraseña()
        {
            return View();
        }
        public ActionResult Validar_cod_conductor()
        {
            return View();
        }
        //////////////////////////////////////////////////////////////////


        //////////////////////////////// METODOS ////////////////////////////////////

        [HttpPost]
        public ActionResult Login(Conductor model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Buscar el conductor en la base de datos
                    var conductor = _context.GetCollection<Conductor>("Conductor")
                                            .Find(c => c.id_conductor == model.id_conductor && c.contraseña == model.contraseña)
                                            .FirstOrDefault();

                    if (conductor != null)
                    {
                        // Establecer el ID del conductor en la sesión
                        Session["id_conductor"] = conductor.id_conductor;

                        // Redirigir a la acción "Index" del controlador "Conductor"
                        return RedirectToAction("Index", "Conductor");
                    }
                    else
                    {
                        ViewBag.Error = "Número de identificación o contraseña incorrecta";
                        return View();
                    }
                }
                catch (ApplicationException ex)
                {
                    ViewBag.Error = "Error al procesar la solicitud: " + ex.Message;
                    return View();
                }
            }

            ViewBag.Error = "ModelState no es válido";
            return View();
        }




        [HttpPost]
        public ActionResult Validar_cod_conductor(string id_pueblo)
        {
            bool isValid = false;

            try
            {
                // Convertir id_pueblo a entero si es necesario
                if (!int.TryParse(id_pueblo, out int codigoInt))
                {
                    TempData["Mensaje"] = "Código inválido";
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
                    TempData["Mensaje_exito"] = "Codigo validado";
                    // Devolver a la vista para mostrar el mensaje
                    return View("Validar_cod_conductor");
                }
                else
                {
                    TempData["Mensaje"] = "Código inválido";
                    return RedirectToAction("Validar_cod_conductor", "Login");
                }
            }
            catch (Exception ex)
            {
                // Manejar errores
                TempData["Mensaje"] = $"Error: {ex.Message}";
                return RedirectToAction("Validar_cod_conductor", "Login");
            }
        }
        [HttpPost]
        public async Task<ActionResult> Registrar(string id_conductor, string nombre, string contraseña, string telefono, string correo)
        {
            try
            {
                var conexion = new Conexion();
                var coleccionConductores = conexion.GetCollection<Conductor>("Conductor");

                // Validar si el conductor ya existe
                var conductorExistente = await coleccionConductores.Find(c => c.id_conductor == Convert.ToInt32(id_conductor)).FirstOrDefaultAsync();

                if (conductorExistente != null)
                {
                    // Si el conductor ya existe, mostrar un mensaje de error
                    TempData["ErrorMessage"] = "El conductor con este ID ya existe. Por favor, use otro ID.";
                    return RedirectToAction("Registro_conductor", "Login");
                }

                // Si el conductor no existe, proceder con el registro
                var nuevo_conductor = new Conductor
                {
                    id_conductor = Convert.ToInt32(id_conductor),
                    nombre = nombre,
                    contraseña = contraseña,
                    telefono = Convert.ToInt64(telefono),
                    correo = correo
                };

                await coleccionConductores.InsertOneAsync(nuevo_conductor);

                // Si la inserción es exitosa, almacenar un mensaje de éxito en TempData
                TempData["SuccessMessage"] = "Registro exitoso. Ahora puedes iniciar sesión.";

                // Devolver la vista de registro para mostrar el mensaje
                return View("Registro_conductor");
            }
            catch (Exception ex)
            {
                // Manejo de errores: almacenar el mensaje de error en TempData
                TempData["ErrorMessage"] = "Ocurrió un error al registrar el conductor: " + ex.Message;

                // Redirigir al usuario de vuelta al formulario de registro para que intente nuevamente
                return RedirectToAction("Registro_conductor", "Login");
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////


    }
}
