using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Capa_entidad
{
    public class Pueblo
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]

        public ObjectId _id { get; set; }
        public int id_pueblo { get; set; }
        public string nombre_pueblo { get; set; }
    }
}
