using DataLayer.Implementations;  // Referencia a la capa de datos
using FeatureLayer;  // Referencia a la capa de entidades
using System.Collections.Generic;

namespace BusinessLayer.Implementations
{
    public class ProductService
    {
        private readonly ProductRepository _productRepository;

        // Constructor que recibe una instancia del repositorio
        public ProductService(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // Método para agregar un producto
        public void AddProduct(string productName, string description, decimal price, int stock, string status, byte[] image)
        {
            var product = new Product(productName, description, price, stock, status, image);
            _productRepository.InsertProduct(product);
        }

        // Método para buscar productos por nombre
        public List<Product> SearchProductsByName(string searchName)
        {
            return _productRepository.SearchProductByName(searchName);
        }


    }
}
