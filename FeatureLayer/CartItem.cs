using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureLayer
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; } // Nueva propiedad para la descripción

        // Propiedad para calcular el subtotal
        public decimal SubTotal => Price * Quantity;
    }

}
