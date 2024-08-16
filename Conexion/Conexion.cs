using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Configuration;

namespace ConexionMongoDB
{
    public class Conexion
    {
        private readonly IMongoDatabase _database;

        public Conexion()
        {
            try
            {
                // Leer la cadena de conexión desde Web.config
                var connectionString = ConfigurationManager.ConnectionStrings["MongoDbConnection"].ConnectionString;

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("La cadena de conexión no puede estar vacía.");
                }

                var mongoClient = new MongoClient(connectionString);

                // Conectar a la base de datos "TerMaster"
                _database = mongoClient.GetDatabase("TerMaster");
            }
            catch (ConfigurationErrorsException ex)
            {
                // Error al leer la configuración
                throw new ApplicationException("Error al leer la configuración de MongoDB.", ex);
            }
            catch (MongoConfigurationException ex)
            {
                // Error en la configuración de MongoDB
                throw new ApplicationException("Error en la configuración de MongoDB.", ex);
            }
            catch (MongoConnectionException ex)
            {
                // Error al conectar a MongoDB
                throw new ApplicationException("No se pudo conectar a la base de datos MongoDB.", ex);
            }
            catch (Exception ex)
            {
                // Otros errores generales
                throw new ApplicationException("Ha ocurrido un error al conectar con MongoDB.", ex);
            }
        }

        // Método genérico para obtener cualquier colección
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            try
            {
                return _database.GetCollection<T>(collectionName);
            }
            catch (MongoException ex)
            {
                // Error al obtener la colección
                throw new ApplicationException($"No se pudo obtener la colección: {collectionName}", ex);
            }
        }
    }
}
