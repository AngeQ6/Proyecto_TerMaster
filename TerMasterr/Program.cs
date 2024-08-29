using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TerMasterr
{
    class Program
    {
        static void Main()
        {
            // Nombre de la zona horaria para Colombia en Windows y Unix/Linux
            string timeZoneIdWindows = "SA Pacific Standard Time";
            string timeZoneIdUnix = "America/Bogota";

            // Inicializar la variable de la zona horaria como null
            TimeZoneInfo colombiaTimeZone = null;

            // Intentar encontrar la zona horaria en Windows
            try
            {
                colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneIdWindows);
                Console.WriteLine($"Zona horaria encontrada: {colombiaTimeZone.DisplayName} ({colombiaTimeZone.Id})");
            }
            catch (TimeZoneNotFoundException)
            {
                Console.WriteLine("La zona horaria 'SA Pacific Standard Time' no se encontró en este sistema.");
            }

            // Si no se encontró la zona horaria de Windows, intentar encontrarla en Unix/Linux
            if (colombiaTimeZone == null)
            {
                try
                {
                    colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneIdUnix);
                    Console.WriteLine($"Zona horaria encontrada: {colombiaTimeZone.DisplayName} ({colombiaTimeZone.Id})");
                }
                catch (TimeZoneNotFoundException)
                {
                    Console.WriteLine("La zona horaria 'America/Bogota' no se encontró en este sistema.");
                }
            }

            // En caso de que ninguna esté disponible
            if (colombiaTimeZone == null)
            {
                Console.WriteLine("No se pudo encontrar la zona horaria para Colombia en este sistema.");
            }
        }
    }
}