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
        #region
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
        #endregion

        /////////////////////////////////////////////// VISTAS ///////////////////////////////
        #region
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
        #endregion

        //////////////////////////////// METODOS ////////////////////////////////////
        #region
        [HttpPost]
        public ActionResult Login(string id, string contraseña)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Buscar el usuario en la colección de conductores
                    var conductor = _context.GetCollection<Conductor>("Conductor")
                        .Find(c => c.id_conductor.ToString() == id && c.contraseña == contraseña)
                        .FirstOrDefault();

                    if (conductor != null)
                    {

                        Session["id_conductor"] = conductor.id_conductor;
                        return RedirectToAction("Index", "Conductor");
                    }

                    // Buscar el usuario en la colección de administradores generales
                    var adminGeneral = _context.GetCollection<AdminG>("AdminG")
                        .Find(a => a.id_admin_general.ToString() == id && a.contraseña_admin_general == contraseña)
                        .FirstOrDefault();

                    if (adminGeneral != null)
                    {
                       

                        Session["IdAdminG"] = adminGeneral.id_admin_general;
                        return RedirectToAction("Index", "Administrador_general");
                    }

                    // Buscar el usuario en la colección de administradores locales
                    var adminLocal = _context.GetCollection<Admin_local>("Admin_local")
                        
                        .Find(a => a.id_admin_local.ToString() == id && a.contraseña_admin_local == contraseña)
                        .FirstOrDefault();

                    if (adminLocal != null)
                    {

                        Session["IdAdmin_local"] = adminLocal.id_admin_local;
                        return RedirectToAction("Index", "Admin_local");
                    }

                    ViewBag.Error = "Número de identificación o contraseña incorrecta";
                    return View();
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
                    TempData["Mensaje_exito"] = $"Código válido para el pueblo {resultado.nombre_pueblo}"; //Mensaje con el nombre del pueblo al que se le validó el código
                    TempData["codigo_pueblo_validado"] = resultado.id_pueblo; // Guardar el código del pueblo validado en TempData
                    return View("Validar_cod_conductor"); // Devolver a la vista para mostrar el mensaje
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
        public async Task<ActionResult> Registrar(string id_conductor, string nombre, string contraseña, string telefono, string correo, string Estado)
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
                    TempData["ErrorMessage_reg_conducor"] = "El conductor con este ID ya existe. Por favor, use otro ID.";
                    return RedirectToAction("Registro_conductor", "Login");
                }
                // Verificar que el código del pueblo esté validado
                if (TempData["codigo_pueblo_validado"] == null)
                {
                    TempData["ErrorMessage_reg_conducor"] = "Debes validar un código de pueblo antes de registrarte.";
                    return RedirectToAction("Registro_conductor", "Login");
                }
                // Obtener el código del pueblo validado
                int codigoPueblo = Convert.ToInt32(TempData["codigo_pueblo_validado"]);
                // Si el conductor no existe, proceder con el registro
                var nuevo_conductor = new Conductor
                {
                    id_conductor = Convert.ToInt32(id_conductor),
                    nombre = nombre,
                    contraseña = contraseña,
                    telefono = Convert.ToInt64(telefono),
                    correo = correo,
                    Estado = Estado,
                    código_pueblo = codigoPueblo // Asociar el conductor con el pueblo validado
                };
                
                await coleccionConductores.InsertOneAsync(nuevo_conductor);
                // Si la inserción es exitosa, almacenar un mensaje de éxito en TempData
                TempData["SuccessMessage_reg_conducor"] = "Registro exitoso. Ahora puedes iniciar sesión.";
                // Devolver la vista de registro para mostrar el mensaje
                return View("Registro_conductor");
            }
            catch (Exception ex)
            {
                // Manejo de errores: almacenar el mensaje de error en TempData
                TempData["ErrorMessage_reg_conducor"] = "Ocurrió un error al registrar el conductor: " + ex.Message;
                // Redirigir al usuario de vuelta al formulario de registro para que intente nuevamente
                return RedirectToAction("Registro_conductor", "Login");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Recp_contraseña(string id, string contraseña)
        {
            try
            {
                int userId = Convert.ToInt32(id);

                // Intenta buscar en la colección de conductores
                var filterConductor = Builders<Conductor>.Filter.Eq(c => c.id_conductor, userId);
                var updateConductor = Builders<Conductor>.Update.Set("contraseña", contraseña);
                var resultConductor = await _context.GetCollection<Conductor>("Conductor")
                                        .UpdateOneAsync(filterConductor, updateConductor);

                if (resultConductor.ModifiedCount > 0)
                {
                    TempData["SuccessMessage"] = "La contraseña ha sido restablecida exitosamente.";
                    return RedirectToAction("Recp_contraseña", "Login");
                }

                // Intenta buscar en la colección de AdminG
                var filterAdminG = Builders<AdminG>.Filter.Eq(a => a.id_admin_general, userId);
                var updateAdminG = Builders<AdminG>.Update.Set("contraseña_admin_general", contraseña);
                var resultAdminG = await _context.GetCollection<AdminG>("AdminG")
                                        .UpdateOneAsync(filterAdminG, updateAdminG);

                if (resultAdminG.ModifiedCount > 0)
                {
                    TempData["SuccessMessage"] = "La contraseña ha sido restablecida exitosamente.";
                    return RedirectToAction("Recp_contraseña", "Login");
                }

                // Intenta buscar en la colección de Admin_local
                var filterAdminLocal = Builders<Admin_local>.Filter.Eq(a => a.id_admin_local, userId);
                var updateAdminLocal = Builders<Admin_local>.Update.Set("contraseña_admin_local", contraseña);
                var resultAdminLocal = await _context.GetCollection<Admin_local>("Admin_local")
                                        .UpdateOneAsync(filterAdminLocal, updateAdminLocal);

                if (resultAdminLocal.ModifiedCount > 0)
                {
                    TempData["SuccessMessage"] = "La contraseña ha sido restablecida exitosamente.";
                    return RedirectToAction("Recp_contraseña", "Login");
                }

                TempData["ErrorMessage"] = "No se encontró un usuario con el ID proporcionado.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al restablecer la contraseña: " + ex.Message;
            }

            return RedirectToAction("Recp_contraseña", "Login");
        }
        public JsonResult VerificarId(int id)
        {
            bool existe = false;

            // Verificar en la colección de conductores
            var conductor = _context.GetCollection<Conductor>("Conductor")
                                    .Find(c => c.id_conductor == id)
                                    .FirstOrDefault();

            if (conductor != null)
            {
                existe = true;
            }
            else
            {
                // Verificar en AdminG
                var adminG = _context.GetCollection<AdminG>("AdminG")
                                     .Find(a => a.id_admin_general == id)
                                     .FirstOrDefault();

                if (adminG != null)
                {
                    existe = true;
                }
                else
                {
                    // Verificar en Admin_local
                    var adminLocal = _context.GetCollection<Admin_local>("Admin_local")
                                             .Find(a => a.id_admin_local == id)
                                             .FirstOrDefault();

                    if (adminLocal != null)
                    {
                        existe = true;
                    }
                }
            }

            return Json(new { existe = existe }, JsonRequestBehavior.AllowGet);
        }
    }
        /////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
}



