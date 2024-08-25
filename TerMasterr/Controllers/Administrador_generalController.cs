using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
        public ActionResult Registrar_administrador_pueblo()
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
            byte[] qrCodeImage = qrCode.GetGraphic(20);

            return File(qrCodeImage, "image/png");
        }

    }
}