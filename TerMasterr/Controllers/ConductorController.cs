﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TerMasterr.Controllers
{
    public class ConductorController : Controller
    {
        // GET: Conductor
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Modificar_datos_personales()
        {
            return View();
        }

        public ActionResult Registrar_entrada()
        {
            return View();
        } 
    }
}