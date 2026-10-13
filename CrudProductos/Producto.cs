using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CrudProductos
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public decimal Precio { get; set; }
        public int Existencias { get; set; }

        public override string ToString()
        {
            return $"{Id} | {Nombre} | {Categoria} | " +
                   $"Precio: {Precio:F2} | Existencias: {Existencias}";
        }
    }
}
