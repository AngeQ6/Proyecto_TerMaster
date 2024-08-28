using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Capa_entidad
{
    public class Conductor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public ObjectId Id { get; set; }
        public int id_conductor { get; set; }
        public string nombre { get; set; }
        public Int64 telefono { get; set; }
        public string correo { get; set; }
        public string contraseña { get; set; }
        public string placa_bus_asignado { get; set; }
    }
}
