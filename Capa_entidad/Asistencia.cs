using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Capa_entidad
{
    public class Asistencia
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public int IdAsistencia { get; set; }  // Campo autoincrementado

        public DateTime FechaIngreso { get; set; }

        [BsonIgnoreIfNull]
        public DateTime? FechaSalida { get; set; }  // Se define como null hasta que se registre la salida

        public int IdConductor { get; set; } // Campo de la colección Conductor

        public string PlacaBus { get; set; }  // Campo de la colección Bus
    }
}
