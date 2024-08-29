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
            // Nombre de la zona horaria para Colombia en Windows
            string timeZoneIdWindows = "SA Pacific Standard Time";
            // Nombre de la zona horaria para Colombia en Unix/Linux
            string timeZoneIdUnix = "America/Bogota";

            TimeZoneInfo colombiaTimeZone;

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

            // Intentar encontrar la zona horaria en Unix/Linux
            try
            {
                colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneIdUnix);
                Console.WriteLine($"Zona horaria encontrada: {colombiaTimeZone.DisplayName} ({colombiaTimeZone.Id})");
            }
            catch (TimeZoneNotFoundException)
            {
                Console.WriteLine("La zona horaria 'America/Bogota' no se encontró en este sistema.");
            }

            // En caso de que ninguna esté disponible
            if (colombiaTimeZone == null)
            {
                Console.WriteLine("No se pudo encontrar la zona horaria para Colombia en este sistema.");
            }
        }
    }
}