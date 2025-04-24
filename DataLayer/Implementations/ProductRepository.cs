using FeatureLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DataLayer.Implementations
{
    // Change the access modifier from internal to public
    public class ProductRepository
    {
        private readonly DbConnection _dbConnection;

        // Constructor with dependency injection
        public ProductRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Method to insert a product, now public
        public void InsertProduct(Product product)
        {
            using (DbCommand command = _dbConnection.CreateCommand())
            {
                command.CommandText = "InsertProduct"; // Name of the stored procedure in MySQL
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters WITHOUT the @ symbol
                var paramName = command.CreateParameter();
                paramName.ParameterName = "p_ProductName";
                paramName.Value = product.ProductName;
                command.Parameters.Add(paramName);

                var paramDesc = command.CreateParameter();
                paramDesc.ParameterName = "p_Description";
                paramDesc.Value = product.Description;
                command.Parameters.Add(paramDesc);

                var paramPrice = command.CreateParameter();
                paramPrice.ParameterName = "p_Price";
                paramPrice.Value = product.Price;
                command.Parameters.Add(paramPrice);

                var paramStock = command.CreateParameter();
                paramStock.ParameterName = "p_Stock";
                paramStock.Value = product.Stock;
                command.Parameters.Add(paramStock);

                var paramStatus = command.CreateParameter();
                paramStatus.ParameterName = "p_Status";
                paramStatus.Value = product.Status;
                command.Parameters.Add(paramStatus);

                var paramImage = command.CreateParameter();
                paramImage.ParameterName = "p_Image";
                paramImage.Value = product.Image;
                paramImage.DbType = DbType.Binary;
                command.Parameters.Add(paramImage);

                // Open connection if it is closed
                if (_dbConnection.State != ConnectionState.Open)
                    _dbConnection.Open();

                command.ExecuteNonQuery();
            }
        }

        // Method to search for products by name
        public List<Product> SearchProductByName(string searchName)
        {
            var products = new List<Product>();

            using (DbCommand command = _dbConnection.CreateCommand())
            {
                command.CommandText = "SearchProductByName"; // Name of the stored procedure
                command.CommandType = CommandType.StoredProcedure;

                var paramSearchName = command.CreateParameter();
                paramSearchName.ParameterName = "p_SearchName";
                paramSearchName.Value = searchName;
                command.Parameters.Add(paramSearchName);

                // Open connection if it is closed
                if (_dbConnection.State != ConnectionState.Open)
                    _dbConnection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int productId = reader.GetInt32(reader.GetOrdinal("ProductID"));
                        string productName = reader.GetString(reader.GetOrdinal("ProductName"));
                        string description = reader.GetString(reader.GetOrdinal("Description"));
                        decimal price = reader.GetDecimal(reader.GetOrdinal("Price"));
                        int stock = reader.GetInt32(reader.GetOrdinal("Stock"));
                        string status = reader.GetString(reader.GetOrdinal("Status"));

                        // Assign null to Image because this SP does not return the image
                        var product = new Product(productId, productName, description, price, stock, status, null);
                        products.Add(product);
                    }
                }
            }

            return products;
        }

        // Method to search for a product by its ID (includes the image)
        public Product SearchProductByID(int productId)
        {
            Product product = null;

            using (DbCommand command = _dbConnection.CreateCommand())
            {
                command.CommandText = "SearchProductByID"; // Name of the stored procedure
                command.CommandType = CommandType.StoredProcedure;

                var paramProductID = command.CreateParameter();
                paramProductID.ParameterName = "p_ProductID";
                paramProductID.Value = productId;
                command.Parameters.Add(paramProductID);

                // Open connection if it is closed
                if (_dbConnection.State != ConnectionState.Open)
                    _dbConnection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string productName = reader.GetString(reader.GetOrdinal("ProductName"));
                        string description = reader.GetString(reader.GetOrdinal("Description"));
                        decimal price = reader.GetDecimal(reader.GetOrdinal("Price"));
                        int stock = reader.GetInt32(reader.GetOrdinal("Stock"));
                        string status = reader.GetString(reader.GetOrdinal("Status"));

                        byte[] image = reader["Image"] as byte[];

                        // Assign the values read from the reader to a Product object
                        product = new Product(productId, productName, description, price, stock, status, image);
                    }
                }
            }

            return product;
        }
    }
}
