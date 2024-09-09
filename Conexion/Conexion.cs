using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Configuration;
using Capa_entidad;

namespace ConexionMongoDB
{
    public class Conexion
    {
        private readonly IMongoDatabase db;

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
                db = mongoClient.GetDatabase("TerMaster");
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
                return db.GetCollection<T>(collectionName);
            }
            catch (MongoException ex)
            {
                // Error al obtener la colección
                throw new ApplicationException($"No se pudo obtener la colección: {collectionName}", ex);
            }
        }
        // Método para obtener el siguiente valor del contador
        //public int GetNextSequenceValue(string collectionName)
        //{
        //    var countersCollection = db.GetCollection<Counter>("Counters");
        //    var filter = Builders<Counter>.Filter.Eq(c => c.CollectionName, collectionName);
        //    var update = Builders<Counter>.Update.Inc(c => c.CurrentValue, 1);
        //    var options = new FindOneAndUpdateOptions<Counter>
        //    {
        //        ReturnDocument = ReturnDocument.After,
        //        IsUpsert = true // Crea el documento si no existe
        //    };

        //    var updatedCounter = countersCollection.FindOneAndUpdate(filter, update, options);
        //    return updatedCounter.CurrentValue;
        //}


        public int GetNextSequenceValue(string collectionName)
        {
            var countersCollection = db.GetCollection<Counter>("Counters");
            var filter = Builders<Counter>.Filter.Eq(c => c.CollectionName, collectionName);
            var update = Builders<Counter>.Update.Inc(c => c.CurrentValue, 1);
            var options = new FindOneAndUpdateOptions<Counter>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true // Crea el documento si no existe
            };

            var updatedCounter = countersCollection.FindOneAndUpdate(filter, update, options);
            return updatedCounter.CurrentValue;
        }


    }
}
