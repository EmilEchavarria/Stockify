using BusinessLayer.Implementations;
using DataLayer.Implementations;
using FeatureLayer;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.Configuration; // 👈 Necesario para leer App.config
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Final_Project.Forms
{
    public partial class FrmHome : Form
    {
        private readonly ProductService _productService;

        public FrmHome()
        {
            InitializeComponent();

            // Obtener cadena de conexión desde App.config
            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            // Crear conexión y pasarla al repositorio y luego al servicio
            var connection = new MySqlConnection(connectionString);
            var productRepository = new ProductRepository(connection);
            _productService = new ProductService(productRepository);
        }

        private void BtnPSearch_Click(object sender, EventArgs e)
        {
            string searchName = TxtPName.Text.Trim();
            var products = _productService.SearchProductsByName(searchName);

            if (products.Count == 0)
            {
                MessageBox.Show("No se encontró ningún producto con ese nombre.", "Búsqueda vacía", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DgvProducts.DataSource = null; // Opcional: limpiar la grilla
                return;
            }

            DgvProducts.DataSource = products;

            // Personalización de nombres de columnas
            DgvProducts.Columns["ProductID"].HeaderText = "Código";
            DgvProducts.Columns["ProductName"].HeaderText = "Nombre";
            DgvProducts.Columns["Description"].HeaderText = "Descripción";
            DgvProducts.Columns["Price"].HeaderText = "Precio";
            DgvProducts.Columns["Stock"].HeaderText = "Stock";
            DgvProducts.Columns["Status"].HeaderText = "Estado";
        }

        private void BtnSearchID_Click(object sender, EventArgs e)
        {
            // Obtener el ID del producto desde un TextBox
            if (int.TryParse(TxtSearchID.Text.Trim(), out int productId))
            {
                // Llamar al método de la capa de negocio para obtener el producto
                Product product = _productService.SearchProductByID(productId);

                if (product != null)
                {
                    // Mostrar los datos del producto en los controles del formulario (Labels en lugar de TextBoxes)
                    LblPName.Text = product.ProductName;
                    LblPDescription.Text = product.Description;

                    // Usar un formato personalizado para RD$ (República Dominicana)
                    LblPPrice.Text = "RD$ " + product.Price.ToString("N2"); // N2 formatea el número con 2 decimales

                    LblPStock.Text = product.Stock.ToString();
                    LblPStatus.Text = product.Status;

                    // Verificar si la imagen es válida antes de intentar mostrarla
                    if (product.Image != null && product.Image.Length > 0)
                    {
                        try
                        {
                            // Convertir los datos binarios en una imagen y mostrarla en el PictureBox
                            using (var ms = new MemoryStream(product.Image))
                            {
                                PbPImage.Image = Image.FromStream(ms);
                            }

                            // Ajustar la imagen al tamaño del PictureBox
                            PbPImage.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                        catch (Exception ex)
                        {
                            // Si ocurre algún error al cargar la imagen, mostrar un mensaje
                            MessageBox.Show($"Error al cargar la imagen: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            PbPImage.Image = null; // Asegurarse de que no haya imagen corrupta
                        }
                    }
                    else
                    {
                        // Si no hay imagen, limpiar el PictureBox
                        PbPImage.Image = null;
                    }
                }
                else
                {
                    MessageBox.Show("Producto no encontrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    // Limpiar los controles después de la alerta
                    LblPName.Text = "";
                    LblPDescription.Text = "";
                    LblPPrice.Text = "";
                    LblPStock.Text = "";
                    LblPStatus.Text = "";
                    PbPImage.Image = null;  // Limpiar la



                }
            }

        }

        private void BtnBuy_Click(object sender, EventArgs e)
        {

        }
    }
}
