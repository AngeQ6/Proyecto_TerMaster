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
<<<<<<< HEAD
        public ActionResult Login(Conductor model, AdminG model1, Admin_local model2)
=======
        public ActionResult Login(string id, string contraseña)
>>>>>>> 1bf7e240fcb36336a4e986c54d2f83dcb828f6fb
        {
            if (ModelState.IsValid)
            {
                try
                {
<<<<<<< HEAD
                    
                    // Buscar el usuario en la colección de conductores
                    var conductor = _context.GetCollection<Conductor>("Conductor")
                        .Find(c => c.id_conductor == model.id_conductor && c.contraseña == model.contraseña)
=======

                    // Buscar el usuario en la colección de conductores
                    var conductor = _context.GetCollection<Conductor>("Conductor")
                        .Find(c => c.id_conductor.ToString() == id && c.contraseña == contraseña)
>>>>>>> 1bf7e240fcb36336a4e986c54d2f83dcb828f6fb
                        .FirstOrDefault();

                    if (conductor != null)
                    {
<<<<<<< HEAD
                        System.Diagnostics.Debug.WriteLine("Conductor encontrado");
                        Session["UserId"] = conductor.id_conductor;
=======

                        Session["id_conductor"] = conductor.id_conductor;
>>>>>>> 1bf7e240fcb36336a4e986c54d2f83dcb828f6fb
                        return RedirectToAction("Index", "Conductor");
                    }

                    // Buscar el usuario en la colección de administradores generales
                    var adminGeneral = _context.GetCollection<AdminG>("AdminG")
<<<<<<< HEAD
                        .Find(a => a.id_admin_general == model1.id_admin_general && a.contraseña_admin_general == model1.contraseña_admin_general)
=======
                        .Find(a => a.id_admin_general.ToString() == id && a.contraseña_admin_general == contraseña)
>>>>>>> 1bf7e240fcb36336a4e986c54d2f83dcb828f6fb
                        .FirstOrDefault();

                    if (adminGeneral != null)
                    {
<<<<<<< HEAD
                        System.Diagnostics.Debug.WriteLine("Admin General encontrado");
                        Session["UserId"] = adminGeneral.id_admin_general;
=======

                        Session["IdAdminG"] = adminGeneral.id_admin_general;
>>>>>>> 1bf7e240fcb36336a4e986c54d2f83dcb828f6fb
                        return RedirectToAction("Index", "Administrador_general");
                    }

                    // Buscar el usuario en la colección de administradores locales
                    var adminLocal = _context.GetCollection<Admin_local>("Admin_local")
<<<<<<< HEAD
                        .Find(a => a.id_admin_local == model2.id_admin_local && a.contraseña_admin_local == model2.contraseña_admin_local)
=======
                        .Find(a => a.id_admin_local.ToString() == id && a.contraseña_admin_local == contraseña)
>>>>>>> 1bf7e240fcb36336a4e986c54d2f83dcb828f6fb
                        .FirstOrDefault();

                    if (adminLocal != null)
                    {
<<<<<<< HEAD
                        System.Diagnostics.Debug.WriteLine("Admin Local encontrado");
                        Session["UserId"] = adminLocal.id_admin_local;
=======

                        Session["IdAdmin_local"] = adminLocal.id_admin_local;
>>>>>>> 1bf7e240fcb36336a4e986c54d2f83dcb828f6fb
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
<<<<<<< HEAD

=======
            catch (Exception ex)
            {
                // Manejo de errores: almacenar el mensaje de error en TempData
                TempData["ErrorMessage"] = "Ocurrió un error al registrar el conductor: " + ex.Message;

                // Redirigir al usuario de vuelta al formulario de registro para que intente nuevamente
                return RedirectToAction("Registro_conductor", "Login");
            }
        }
>>>>>>> 1bf7e240fcb36336a4e986c54d2f83dcb828f6fb
        /////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
    }


}

