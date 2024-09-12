using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_entidad
{
    public class AdminG
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public int id_admin_general { get; set; }
        public string nombre_admin_general { get; set; }
        public string correo_admin_general { get; set; }
        public long telefono_admin_general { get; set; }
        public string contraseña_admin_general { get; set; }
    }
}
