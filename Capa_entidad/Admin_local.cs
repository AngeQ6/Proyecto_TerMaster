using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_entidad
{
    public class Admin_local
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public int id_admin_local { get; set; }
        public string nombre_admin_local { get; set; }
        public string apellido_admin_local { get; set; }
        public long telefono_admin_local { get; set; }
        public string correo_admin_local { get; set; }
        public string contraseña_admin_local { get; set; }
        public int id_pueblo { get; set; }
        public string estado { get; set; }
        public int ImagenUrl { get; set; }
    }
}
