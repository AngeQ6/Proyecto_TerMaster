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
        [BsonRepresentation(BsonType.Int32)]

        public int id_conductor { get; set; }
        public string contraseña { get; set; }
        public string nombre { get; set; }
    }
}
