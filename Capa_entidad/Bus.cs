using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Capa_entidad
{
    public class Bus
    {
        //[BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        //public ObjectId Id { get; set; } // ID del documento generado por mongoDB
        //[BsonElement("placa")]
        //public string Placa { get; set; }

        //[BsonElement("numero")]
        //public string Numero { get; set; }

        //[BsonElement("ruta")]
        //public string Ruta { get; set; }

        //[BsonElement("capacidad")]
        //public int Capacidad { get; set; }

        //[BsonElement("estado")]
        //public string Estado { get; set; }

        //[BsonElement("imagen")]
        //public byte[] Imagen { get; set; }
        public int Id { get; set; }
        public string Number { get; set; }
        public string Route { get; set; }
        public int Capacity { get; set; }
        public string ImageUrl { get; set; }
    }
}
