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
using System.Web.Security;

namespace TerMasterr.Controllers
{
    public class LoginController : Controller
    {
        /////////////////////////////////// CONEXION BD ////////////////////////////
        #region
        private readonly Conexion _context; // Objeto con el que se maneja la conexion a la base de datos obtenida de la clase Conexion
        public LoginController()  // Constructor del controlador para establecer la conexión con la base de datos
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
        /////////////////////////////////////////////////////////////////////////////////
        #endregion

        /////////////////////////////////////////////// VISTAS ///////////////////////////////
        #region
        //Vista de inicio de sesión
        public ActionResult Login()
        {
            return View();
        }

        //Vista para el registro de los conductores
        public ActionResult Registro_conductor()
        {
            return View();
        }
        //Vista para la recuperación de la contraseña
        public ActionResult Recp_contraseña()
        {
            return View();
        }
        // Vista para validar el código de pueblo y poder registrarse como conductor
        public ActionResult Validar_cod_conductor()
        {
            return View();
        }
        //////////////////////////////////////////////////////////////////
        #endregion

        //////////////////////////////// METODOS ////////////////////////////////////
        #region

        //Inicio de sesión
        [HttpPost]
        public ActionResult Login(string id, string contraseña)
        {
            if (ModelState.IsValid) // Verificar que el modelo de datos sea válido
            {
                try
                {
                    // Convertir el ID a entero
                    int idIngresado;
                    if (!int.TryParse(id, out idIngresado)) // Verifica si el ID es un número válido
                    {
                        ViewBag.Error = "El ID ingresado no es válido."; // Mensaje de error si no es válido
                        return View(); // Retorna a la vista de login
                    }

                    // Buscar el usuario en la colección de conductores a través de la consulta
                    var conductor = _context.GetCollection<Conductor>("Conductor")
                        .Find(c => c.id_conductor == idIngresado && c.contraseña == contraseña)
                        .FirstOrDefault(); // Busca el conductor con el ID y contraseña

                    if (conductor != null) // Condición que se cumple si se encuentra el conductor
                    {
                        // Almacena información en la sesión y redirige a la página del conductor
                        Session["id_conductor"] = conductor.id_conductor; // Obtiene el id del conductor que inició sesión
                        Session["nombre_usuario"] = conductor.nombre; // Obtiene el nombre del usuario que inició sesión
                        return RedirectToAction("Index", "Conductor"); // Retorna a la vista Index del controlador conductor luego de que los datos ingresados hayan sido válidos
                    } 

                    // Buscar el usuario en la colección de administradores generales
                    var adminGeneral = _context.GetCollection<AdminG>("AdminG")
                        .Find(a => a.id_admin_general == idIngresado && a.contraseña_admin_general == contraseña)
                        .FirstOrDefault(); // Buscar administrador general por su id y contrasela

                    if (adminGeneral != null) // Condición que se cumple si el administrador general es encontrado y los datos son correctos
                    {
                        Session["id_admin_general"] = adminGeneral.id_admin_general; // Obtiene el id del administrador
                        Session["nombre_usuario"] = adminGeneral.nombre_admin_general; // Obtiene el nombre del usuario
                        return RedirectToAction("Gestion_admin_local", "Administrador_general"); // Retorna a la vista de gestión de administradores locales del controlador Administrador_general
                    }

                    // Buscar el usuario en la colección de administradores locales
                    var adminLocal = _context.GetCollection<Admin_local>("Admin_local")
                        .Find(a => a.id_admin_local == idIngresado && a.contraseña_admin_local == contraseña)
                        .FirstOrDefault(); // Busca al administrador local por su id y contraseña

                    if (adminLocal != null) // Condición que se cumple si el administrador local es encontrado y las credenciales ingresadas son correctas
                    {
                        Session["id_admin_local"] = adminLocal.id_admin_local; // Obtiene y guarda el id del adminnistrador local
                        Session["nombre_usuario"] = adminLocal.nombre_admin_local; // Guarda el nombre del usuario
                        Session["PuebloId"] = adminLocal.id_pueblo; // Guarda el PuebloId del administrador local
                        return RedirectToAction("Gestion_conductor", "Admin_local"); // Redirecciona a la vista de gestión de conductores del controlador Admin_local
                    }

                    TempData["ErrorMessage_login"] = "Número de identificación o contraseña incorrecta"; // Mensaje por si no se cumple ninguna de las condiciones anteriores o si los datos ingresados no son correctos
                    return RedirectToAction("Login", "Login"); // Redirecciona nuevamente a la vista de Login para que intente nuevamente iniciar sesión
                }
                catch (ApplicationException ex)
                {
                    // Captura y muestra errores durante el proceso de inicio de sesión
                    TempData["ErrorMessage_login"] = "Error al procesar la solicitud: " + ex.Message; 
                    return View();
                }
            }
            // Si el estado del modelo no es válido
            TempData["ErrorMessage_login"] = "ModelState no es válido";
            return View();
        }

        // Método para cerrar sesión
        public ActionResult Cerrar_sesion()
        {
            FormsAuthentication.SignOut(); // Cierra la autenticación
            Session.Abandon(); // Abandona la sesión actual
            return RedirectToAction("Login", "Login"); // Redirigir al login
        }

        // Validar código de pueblo para el registro del conductor
        [HttpPost]
        public ActionResult Validar_cod_conductor(string id_pueblo) 
        {
            bool isValid = false; // Variable para validar el código
            try
            {
                // Convertir id_pueblo a entero si es necesario
                if (!int.TryParse(id_pueblo, out int codigoInt)) // Valida que el ID sea un número
                {
                    TempData["MensajeError_CodPueblo"] = "Código inválido"; // Mensaje de error
                    return RedirectToAction("Validar_cod_conductor", "Login"); // Redirige a la vista para validar el código 
                }

                // Obtener la colección desde MongoDB
                var collection = _context.GetCollection<Pueblo>("Pueblo");

                // Construir el filtro usando Builders<T>.Filter
                var filtro = Builders<Pueblo>.Filter.Eq("id_pueblo", codigoInt); // Filtrar por ID del pueblo
                var resultado = collection.Find(filtro).FirstOrDefault(); // Buscar el pueblo

                // Verificar si el documento existe
                isValid = resultado != null;

                if (isValid) // Condición que se cumple si el código es válido 
                {
                    TempData["Mensaje_exito"] = $"Código válido para el pueblo {resultado.nombre_pueblo}"; //Mensaje con el nombre del pueblo al que se le validó el código
                    TempData["codigo_pueblo_validado"] = resultado.id_pueblo; // Guardar el código del pueblo validado en TempData
                    return View("Validar_cod_conductor"); // Devolver a la vista para mostrar el mensaje
                }
                else
                {
                    TempData["MensajeError_CodPueblo"] = "Código inválido"; // Mensaje de error si el código no es válido
                    return RedirectToAction("Validar_cod_conductor", "Login"); // Redirecciona a la vista de validar código si el código no es válido
                }
            }
            catch (Exception ex)
            {
                // Manejar errores
                TempData["MensajeError_CodPueblo"] = $"Error: {ex.Message}";
                return RedirectToAction("Validar_cod_conductor", "Login");
            }
        }

        // Método para registrar conudctor
        [HttpPost]
        public async Task<ActionResult> Registrar(string id_conductor, string nombre, string contraseña, string telefono, string correo, string Estado) // Datos del conductor
        {
            try
            {
                var conexion = new Conexion(); // Conexión a la base de datos
                var coleccionConductores = conexion.GetCollection<Conductor>("Conductor"); // Obtener la colección conductores
                // Validar si el conductor ya existe
                var conductorExistente = await coleccionConductores.Find(c => c.id_conductor == Convert.ToInt32(id_conductor)).FirstOrDefaultAsync();

                if (conductorExistente != null) // Condición que se cumple si el conductor ya existe en la base de datos
                {
                    // Si el conductor ya existe, mostrar un mensaje de error
                    TempData["ErrorMessage_reg_conducor"] = "El conductor con este ID ya existe. Por favor, use otro ID."; // Manejo del mensaje si existe el conductor
                    return RedirectToAction("Registro_conductor", "Login"); // Redirecciona a la vista para que intente hacer de nuevo el proceso de registro
                }
                // Verificar que el código del pueblo esté validado
                if (TempData["codigo_pueblo_validado"] == null)
                {
                    TempData["ErrorMessage_reg_conducor"] = "Debes validar un código de pueblo antes de registrarte."; // Mensaje que se muestra si el usuario no ha validado el código de pueblo antes de registrarse
                    return RedirectToAction("Registro_conductor", "Login");
                }
                // Obtener el código del pueblo validado
                int codigoPueblo = Convert.ToInt32(TempData["codigo_pueblo_validado"]);
                // Si el conductor no existe, proceder con el registro
                var nuevo_conductor = new Conductor
                {
                    // Datos para el registro del conductor
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
        // Método para restablecer la contraseña
        [HttpPost]
        public async Task<ActionResult> Recp_contraseña(string id, string contraseña)
        {
            try
            {
                int userId = Convert.ToInt32(id); // Convertir el ID a entero

                // Intenta buscar en la colección de conductores
                var filterConductor = Builders<Conductor>.Filter.Eq(c => c.id_conductor, userId); // Variable para obtener al conductor por su id
                var updateConductor = Builders<Conductor>.Update.Set("contraseña", contraseña); // Variable para actualizar la contraseña
                var resultConductor = await _context.GetCollection<Conductor>("Conductor") // Variable para manejar los resultados
                                        .UpdateOneAsync(filterConductor, updateConductor);

                if (resultConductor.ModifiedCount > 0) // Condiciónque se cumple si el resultado es exitoso
                {
                    TempData["SuccessMessage"] = "La contraseña ha sido restablecida exitosamente.";
                    return RedirectToAction("Recp_contraseña", "Login");
                }

                // Intenta buscar en la colección de AdminG
                var filterAdminG = Builders<AdminG>.Filter.Eq(a => a.id_admin_general, userId); // Variable para obtener al administrador general por su id
                var updateAdminG = Builders<AdminG>.Update.Set("contraseña_admin_general", contraseña); // Variable para actualizar la contraseña
                var resultAdminG = await _context.GetCollection<AdminG>("AdminG") // Variable para manejar los resultados
                                        .UpdateOneAsync(filterAdminG, updateAdminG);

                if (resultAdminG.ModifiedCount > 0) // Condiciónque se cumple si el resultado es exitoso
                {
                    TempData["SuccessMessage"] = "La contraseña ha sido restablecida exitosamente.";
                    return RedirectToAction("Recp_contraseña", "Login");
                }

                // Intenta buscar en la colección de Admin_local
                var filterAdminLocal = Builders<Admin_local>.Filter.Eq(a => a.id_admin_local, userId); // Variable para obtener al administrador local por su id
                var updateAdminLocal = Builders<Admin_local>.Update.Set("contraseña_admin_local", contraseña); // Variable para actualizar la contraseña
                var resultAdminLocal = await _context.GetCollection<Admin_local>("Admin_local") // Variable para manejar los resultados
                                        .UpdateOneAsync(filterAdminLocal, updateAdminLocal);

                if (resultAdminLocal.ModifiedCount > 0) // Condiciónque se cumple si el resultado es exitoso
                {
                    TempData["SuccessMessage"] = "La contraseña ha sido restablecida exitosamente.";
                    return RedirectToAction("Recp_contraseña", "Login");
                }
                // Mensaje que se muestra si el id proporcionado no existe en la base de datos
                TempData["ErrorMessage"] = "No se encontró un usuario con el ID proporcionado.";
            }
            catch (Exception ex)
            {
                // Manejo de errores en el proceso de restablecimiento de contraseña
                TempData["ErrorMessage"] = "Ocurrió un error al restablecer la contraseña: " + ex.Message;
            }

            return RedirectToAction("Recp_contraseña", "Login");
        }

        // Método para verificar si un ID ya está registrado en alguna colección
        public JsonResult VerificarId(int id)
        {
            bool existe = false; // Variable que se utiliza para al existencia del usuario

            // Verificar en la colección de conductores
            var conductor = _context.GetCollection<Conductor>("Conductor")
                                    .Find(c => c.id_conductor == id) 
                                    .FirstOrDefault();

            if (conductor != null) // Condición que se cumple si el conductor existe
            {
                existe = true;
            }
            else
            {
                // Verificar en AdminG
                var adminG = _context.GetCollection<AdminG>("AdminG")
                                     .Find(a => a.id_admin_general == id)
                                     .FirstOrDefault();

                if (adminG != null) // Condición que se cumple si el administrador general existe
                {
                    existe = true;
                }
                else
                {
                    // Verificar en Admin_local
                    var adminLocal = _context.GetCollection<Admin_local>("Admin_local")
                                             .Find(a => a.id_admin_local == id)
                                             .FirstOrDefault();

                    if (adminLocal != null) // Condición que se cumple si el administrador local existe
                    {
                        existe = true;
                    }
                }
            }

            return Json(new { existe = existe }, JsonRequestBehavior.AllowGet); // Resultado de la verificación
        }
    }
        /////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
}
