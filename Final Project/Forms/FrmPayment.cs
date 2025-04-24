using BusinessLayer.Implementations;
using FeatureLayer;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Final_Project.Forms
{
    public partial class FrmPayment : Form
    {
        private decimal totalAPagar;
        private readonly ProductService _productService;
        private List<CartItem> cartItems;

        public FrmPayment(decimal total, List<CartItem> items)
        {
            InitializeComponent();
            totalAPagar = total;
            cartItems = items ?? new List<CartItem>(); // Asegura que no sea null
        }

        private void FrmPayment_Load(object sender, EventArgs e)
        {
            // 🔵 Bordes redondeados
            int borderRadius = 25;
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(new Rectangle(0, 0, borderRadius, borderRadius), 180, 90);
            path.AddArc(new Rectangle(this.Width - borderRadius, 0, borderRadius, borderRadius), 270, 90);
            path.AddArc(new Rectangle(this.Width - borderRadius, this.Height - borderRadius, borderRadius, borderRadius), 0, 90);
            path.AddArc(new Rectangle(0, this.Height - borderRadius, borderRadius, borderRadius), 90, 90);
            path.CloseFigure();
            this.Region = new Region(path);

            // 🧾 Mostrar total a pagar
            LblTotalPagar.Text = "Total a pagar: RD$ " + totalAPagar.ToString("N2");
        }

        private void BtnPagar_Click(object sender, EventArgs e)
        {
            // Validación básica de campos
            if (string.IsNullOrWhiteSpace(TxtNombreTitular.Text) ||
                string.IsNullOrWhiteSpace(TxtNumeroTarjeta.Text) ||
                string.IsNullOrWhiteSpace(TxtExpiracion.Text) ||
                string.IsNullOrWhiteSpace(TxtCVV.Text))
            {
                MessageBox.Show("Por favor, complete todos los campos de la tarjeta.", "Campos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validación extra opcional
            if (TxtNumeroTarjeta.Text.Length != 16 || !TxtNumeroTarjeta.Text.All(char.IsDigit))
            {
                MessageBox.Show("El número de tarjeta debe tener 16 dígitos.", "Número inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (TxtCVV.Text.Length != 3 || !TxtCVV.Text.All(char.IsDigit))
            {
                MessageBox.Show("El CVV debe tener 3 dígitos.", "CVV inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Verificar que haya items en el carrito
            if (cartItems == null || cartItems.Count == 0)
            {
                MessageBox.Show("El carrito está vacío.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int clientId = 1; // Deberías obtener el ID del cliente logueado
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
                        cmd.Parameters.AddWithValue("p_Total", totalAPagar);

                        var saleIdParam = new MySqlParameter("p_SaleID", MySqlDbType.Int32);
                        saleIdParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(saleIdParam);

                        cmd.ExecuteNonQuery();
                        saleId = Convert.ToInt32(saleIdParam.Value);
                    }

                    // 2. Insertar detalles de la venta
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
                        cmd.Parameters.AddWithValue("p_Total", totalAPagar);

                        var invoiceIdParam = new MySqlParameter("p_InvoiceID", MySqlDbType.Int32);
                        invoiceIdParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(invoiceIdParam);

                        cmd.ExecuteNonQuery();
                        invoiceId = Convert.ToInt32(invoiceIdParam.Value);
                    }

                    // 4. Insertar detalles de la factura
                    foreach (var item in cartItems)
                    {
                        using (var cmd = new MySqlCommand("InsertInvoiceDetail", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("p_InvoiceID", invoiceId);
                            cmd.Parameters.AddWithValue("p_Description", item.Description);
                            cmd.Parameters.AddWithValue("p_Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("p_Price", item.Price);
                            cmd.Parameters.AddWithValue("p_Subtotal", item.SubTotal);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("¡Pago realizado exitosamente!", "Confirmación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK; // Indica que el pago fue exitoso
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al procesar la compra: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}