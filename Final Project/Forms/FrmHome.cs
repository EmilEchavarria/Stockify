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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace Final_Project.Forms
{
    public partial class FrmHome : Form
    {
        private readonly ProductService _productService;
        private List<CartItem> cartItems; // Lista para almacenar los productos del carrito

        public FrmHome()
        {
            InitializeComponent();

            cartItems = new List<CartItem>();

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
        private void BtnAddCart_Click(object sender, EventArgs e)
        {
            // Obtener el ID del producto desde el TextBox
            if (int.TryParse(TxtSearchID.Text.Trim(), out int productId))
            {
                // Llamar al método de la capa de negocio para obtener el producto
                Product product = _productService.SearchProductByID(productId);

                if (product != null)
                {
                    // Leer la cantidad del TextBox de cantidad
                    if (int.TryParse(TxtQuantity.Text.Trim(), out int quantity) && quantity > 0)
                    {
                        // Verificar si el producto ya está en el carrito
                        var existingItem = cartItems.FirstOrDefault(item => item.ProductID == product.ProductID);

                        if (existingItem != null)
                        {
                            // Si el producto ya existe en el carrito, actualizar la cantidad
                            existingItem.Quantity += quantity;
                        }
                        else
                        {
                            // Si no existe, agregar un nuevo item al carrito
                            cartItems.Add(new CartItem
                            {
                                ProductID = product.ProductID,
                                ProductName = product.ProductName,
                                Price = product.Price,
                                Quantity = quantity
                            });
                        }

                        // Mostrar los productos en el DataGridView del carrito
                        DisplayCartItems();

                        // Mostrar el mensaje con las opciones de continuar comprando o ir al carrito
                        var result = MessageBox.Show(
                            $"{quantity} unidades de {product.ProductName} han sido añadidas al carrito. ¿Qué deseas hacer ahora?",
                            "Producto añadido",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        // Si el usuario selecciona "Sí" (Ir al carrito)
                        if (result == DialogResult.Yes)
                        {
                            // Cambiar a la pestaña del carrito
                            TabControl.SelectedTab = Carrito;  // Cambia "tabControl1" y "tabPageCarrito" por los nombres correctos
                        }

                        // Si el usuario selecciona "No" (Continuar comprando)
                        else
                        {
                            // El usuario continúa comprando, no haces nada y el control sigue en la misma pestaña
                            // Solo se cerrará el mensaje y el usuario puede seguir navegando por la tienda.
                        }
                    }
                    else
                    {
                        // Si la cantidad ingresada no es válida o menor que 1
                        MessageBox.Show("Por favor, ingrese una cantidad válida mayor a 0.", "Cantidad inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    // Si el producto no se encuentra en la base de datos
                    MessageBox.Show("Producto no encontrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Si el ID de producto no es válido
                MessageBox.Show("Por favor, ingrese un ID de producto válido.", "ID inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void DisplayCartItems()
        {
            DgvCart.DataSource = null;
            DgvCart.Columns.Clear(); // Limpiar columnas anteriores

            DgvCart.DataSource = cartItems;

            // Renombrar columnas
            DgvCart.Columns["ProductID"].HeaderText = "Código";
            DgvCart.Columns["ProductName"].HeaderText = "Nombre";
            DgvCart.Columns["Price"].HeaderText = "Precio";
            DgvCart.Columns["Quantity"].HeaderText = "Cantidad";
            DgvCart.Columns["SubTotal"].HeaderText = "SubTotal";

            // Agregar columna de botón para eliminar
            DataGridViewButtonColumn btnEliminar = new DataGridViewButtonColumn();
            btnEliminar.HeaderText = "Acción";
            btnEliminar.Text = "Eliminar";
            btnEliminar.UseColumnTextForButtonValue = true;
            btnEliminar.Name = "Eliminar";

            DgvCart.Columns.Add(btnEliminar);

            // 🔢 Calcular el total
            decimal total = cartItems.Sum(item => item.SubTotal);
            LblTotal.Text = "Total: RD$ " + total.ToString("N2");
        }


        private void FrmHome_Load(object sender, EventArgs e)
        {
            DgvCart.CellClick += DgvCart_CellClick;

        }

        private void DgvCart_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Asegurarse de que no se hizo clic en el encabezado y que se hizo clic en la columna "Eliminar"
            if (e.RowIndex >= 0 && DgvCart.Columns[e.ColumnIndex].Name == "Eliminar")
            {
                // Obtener el ID del producto de la fila seleccionada
                int productId = Convert.ToInt32(DgvCart.Rows[e.RowIndex].Cells["ProductID"].Value);

                // Confirmar eliminación
                var confirmResult = MessageBox.Show("¿Estás seguro que deseas eliminar este producto del carrito?",
                                                    "Confirmar eliminación",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question);

                if (confirmResult == DialogResult.Yes)
                {
                    
                    var itemToRemove = cartItems.FirstOrDefault(item => item.ProductID == productId);
                    if (itemToRemove != null)
                    {
                        cartItems.Remove(itemToRemove);
                        DisplayCartItems(); 
                    }
                }
            }
        }

        private void BtnPay_Click(object sender, EventArgs e)
        {
            decimal total = cartItems.Sum(item => item.SubTotal);

            if (total > 0)
            {
                FrmPayment frmPayment = new FrmPayment(total);
                var result = frmPayment.ShowDialog(); 

                cartItems.Clear();
                DisplayCartItems(); 
            }
            else
            {
                MessageBox.Show("El carrito está vacío.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


    }
}
