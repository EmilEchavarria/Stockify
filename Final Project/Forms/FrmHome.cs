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
        private Timer refreshTimer;
        private Timer refreshFacturasTimer;


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



        private void FrmHome_Load(object sender, EventArgs e)
        {
            DgvCart.CellClick += DgvCart_CellClick;
            CargarFacturasConDetalles();      // 👉 Muestra facturas

            CargarComprasRealizadas(); // Cargar las compras realizadas al cargar el formulario

            // Configurar el Timer para que ejecute la actualización cada 1000 milisegundos (1 segundo)
            refreshTimer = new Timer();
            refreshTimer.Interval = 15000; // 1000 milisegundos = 1 segundo
            refreshTimer.Tick += (s, args) => CargarComprasRealizadas(); // Llama a la función cada vez que se cumpla el intervalo
            refreshTimer.Start(); // Inicia el Timer



            // Configurar el Timer para actualizar las facturas cada 15 segundos
            refreshFacturasTimer = new Timer();
            refreshFacturasTimer.Interval = 15000; // 15000 milisegundos = 15 segundos
            refreshFacturasTimer.Tick += (s, args) => CargarFacturasConDetalles(); // Llama a la función cada vez que se cumpla el intervalo
            refreshFacturasTimer.Start(); // Inicia el Timer
        }

        private void FrmHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            refreshTimer.Stop(); // Detener el Timer al cerrar el formulario
            refreshTimer.Dispose(); // Liberar recursos del Timer
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
            // Verificar si hay items en el carrito
            if (cartItems == null || cartItems.Count == 0)
            {
                MessageBox.Show("El carrito está vacío. Agregue productos antes de pagar.",
                               "Carrito vacío",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                return;
            }

            // Calcular el total del carrito
            decimal total = cartItems.Sum(item => item.SubTotal);

            // Crear instancia de FrmPayment pasando ambos parámetros requeridos
            FrmPayment paymentForm = new FrmPayment(total, cartItems);

            // Mostrar el formulario de pago de forma modal
            DialogResult result = paymentForm.ShowDialog();

            // Procesar el resultado si el pago fue exitoso
            if (result == DialogResult.OK)
            {
                // Limpiar el carrito después de pago exitoso
                cartItems.Clear();

                // Actualizar la visualización del carrito (ejemplo)
                RefrescarVisualizacionCarrito();

                // Opcional: Mostrar mensaje de confirmación
                MessageBox.Show("¡Pago completado con éxito!",
                               "Éxito",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Information);
            }
        }

        // Método para actualizar la visualización del carrito
        private void RefrescarVisualizacionCarrito()
        {
            // Aquí va tu lógica para actualizar la UI del carrito
            // Por ejemplo:
            DgvCart.DataSource = null;
            DgvCart.DataSource = cartItems;
            LblTotal.Text = "RD$ 0.00"; // Reiniciar el total mostrado

            // O si usas controles diferentes:
            // listBoxCarrito.Items.Clear();
            // lblTotal.Text = "Total: RD$ 0.00";
        }

      


        private void CargarComprasRealizadas()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (var cmd = new MySqlCommand("GetAllSales", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (var adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            DgvComprasRealizadas.DataSource = dt;

                            // Opcional: Renombrar columnas para mostrar en español
                            DgvComprasRealizadas.Columns["SaleID"].HeaderText = "ID Venta";
                            DgvComprasRealizadas.Columns["ClientName"].HeaderText = "Cliente";
                            DgvComprasRealizadas.Columns["SaleDate"].HeaderText = "Fecha";
                            DgvComprasRealizadas.Columns["Total"].HeaderText = "Total";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar las compras realizadas: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void CargarFacturasConDetalles()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Primero obtenés todas las facturas
                    using (var cmdAll = new MySqlCommand("SELECT InvoiceID FROM Invoices", connection))
                    {
                        using (var reader = cmdAll.ExecuteReader())
                        {
                            List<int> invoiceIds = new List<int>();
                            while (reader.Read())
                            {
                                invoiceIds.Add(reader.GetInt32("InvoiceID"));
                            }
                            reader.Close();

                            DataTable dtGlobal = new DataTable();

                            foreach (var invoiceId in invoiceIds)
                            {
                                using (var cmdDetail = new MySqlCommand("GetInvoiceFullById", connection))
                                {
                                    cmdDetail.CommandType = CommandType.StoredProcedure;
                                    cmdDetail.Parameters.AddWithValue("p_InvoiceID", invoiceId);

                                    using (var adapter = new MySqlDataAdapter(cmdDetail))
                                    {
                                        DataTable dt = new DataTable();
                                        adapter.Fill(dt);

                                        if (dtGlobal.Columns.Count == 0)
                                            dtGlobal = dt.Clone(); // Clonás estructura solo una vez

                                        foreach (DataRow row in dt.Rows)
                                            dtGlobal.ImportRow(row);
                                    }
                                }
                            }

                            DgvFacturas.DataSource = dtGlobal;

                            // Asegurarse de que el DataGridView tiene columnas
                            if (DgvFacturas.Columns.Count > 0)
                            {
                                // Cambiar los encabezados
                                DgvFacturas.Columns["InvoiceID"].HeaderText = "ID Factura";
                                DgvFacturas.Columns["ClientName"].HeaderText = "Cliente";
                                DgvFacturas.Columns["InvoiceDate"].HeaderText = "Fecha";
                                DgvFacturas.Columns["Description"].HeaderText = "Producto";
                                DgvFacturas.Columns["Quantity"].HeaderText = "Cantidad";
                                DgvFacturas.Columns["UnitPrice"].HeaderText = "Precio Unitario";
                                DgvFacturas.Columns["Subtotal"].HeaderText = "Subtotal";
                                DgvFacturas.Columns["InvoiceTotal"].HeaderText = "Total Factura";
                                DgvFacturas.Columns["Status"].HeaderText = "Estado";
                            }
                        


                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar las facturas con detalles: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }




        /*private void BtnPay_Click(object sender, EventArgs e)
        {
            decimal total = cartItems.Sum(item => item.SubTotal);

            if (total > 0)
            {
                int clientId = 1;
                string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        int saleId = 0;
                        int invoiceId = 0;

                        // 1. Insertar venta
                        using (var cmd = new MySqlCommand("InsertSale", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("p_ClientID", clientId);
                            cmd.Parameters.AddWithValue("p_SaleDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("p_Total", total);

                            var saleIdParam = new MySqlParameter("p_SaleID", MySqlDbType.Int32);
                            saleIdParam.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(saleIdParam);

                            cmd.ExecuteNonQuery();
                            saleId = Convert.ToInt32(saleIdParam.Value);
                        }

                        // 2. Detalles de venta
                        foreach (var item in cartItems)
                        {
                            using (var cmd = new MySqlCommand("InsertSaleDetail", connection))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("p_SaleID", saleId);
                                cmd.Parameters.AddWithValue("p_ProductID", item.ProductID);
                                cmd.Parameters.AddWithValue("p_Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("p_Subtotal", item.SubTotal);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 3. Insertar factura
                        using (var cmd = new MySqlCommand("InsertInvoice", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("p_ClientID", clientId);
                            cmd.Parameters.AddWithValue("p_InvoiceDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("p_Total", total);

                            var invoiceIdParam = new MySqlParameter("p_InvoiceID", MySqlDbType.Int32);
                            invoiceIdParam.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(invoiceIdParam);

                            cmd.ExecuteNonQuery();
                            invoiceId = Convert.ToInt32(invoiceIdParam.Value);
                        }

                        // 4. Detalles de factura
                        foreach (var item in cartItems)
                        {
                            using (var cmd = new MySqlCommand("InsertInvoiceDetail", connection))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("p_InvoiceID", invoiceId);
                                cmd.Parameters.AddWithValue("p_Description", item.Description); // Verificá que esto exista
                                cmd.Parameters.AddWithValue("p_Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("p_Price", item.Price);
                                cmd.Parameters.AddWithValue("p_Subtotal", item.SubTotal);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("La compra se ha realizado con éxito.", "Compra exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    cartItems.Clear();
                    DisplayCartItems();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al procesar la compra: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("El carrito está vacío.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        */



    }
}
