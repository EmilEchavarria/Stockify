using System;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Drawing;
using BusinessLayer.Implementations;
using DataLayer.Implementations;
using FeatureLayer;
using System.IO;

namespace Final_Project.Forms
{
    public partial class FrmAdminDashboard : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        private Timer refreshTimer;

        public FrmAdminDashboard()
        {
            InitializeComponent();

            // Configurar el timer para actualizaciones automáticas
            refreshTimer = new Timer
            {
                Interval = 10000 // 10 segundos
            };
            refreshTimer.Tick += RefreshTimer_Tick;
        }

        private void FrmAdminDashboard_Load(object sender, EventArgs e)
        {
            UpdateTotalClientsCount();
            refreshTimer.Start();
        }

        private void UpdateTotalClientsCount()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Clients"; // Consulta modificada para Clients
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        int totalClients = Convert.ToInt32(command.ExecuteScalar());

                        // Actualizar el Label
                        if (LblTotalUsers.InvokeRequired)
                        {
                            LblTotalUsers.Invoke((MethodInvoker)delegate {
                                LblTotalUsers.Text = $"{totalClients}";
                            });
                        }
                        else
                        {
                            LblTotalUsers.Text = $"{totalClients}";
                        }
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                MessageBox.Show($"Error de MySQL: {sqlEx.Message}\nCódigo: {sqlEx.Number}",
                              "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LblTotalUsers.Text = "Error al cargar clientes";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                LblTotalUsers.Text = "Error en sistema";
            }
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            UpdateTotalClientsCount();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
        }

        private void BtnAddImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Seleccionar imagen";
                ofd.Filter = "Archivos de imagen (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Verificar si el archivo existe y es una imagen válida
                        if (File.Exists(ofd.FileName))
                        {
                            // Mostrar imagen en el PictureBox
                            PbPImage.Image = Image.FromFile(ofd.FileName);
                            PbPImage.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                        else
                        {
                            MessageBox.Show("El archivo seleccionado no existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Capturar cualquier error al cargar la imagen
                        MessageBox.Show($"Error al cargar la imagen: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnSaveP_Click(object sender, EventArgs e)
        {
            // Verificar si todos los campos obligatorios están completos
            if (string.IsNullOrEmpty(TxtPName.Text) || string.IsNullOrEmpty(TxtPDescription.Text) || CbSPSatus.SelectedIndex == -1 || PbPImage.Image == null || string.IsNullOrEmpty(TxtPPrice.Text) || string.IsNullOrEmpty(TxtPStock.Text))
            {
                MessageBox.Show("Por favor complete todos los campos.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Convertir la imagen del PictureBox a un arreglo de bytes
                byte[] imageBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    PbPImage.Image.Save(ms, PbPImage.Image.RawFormat);
                    imageBytes = ms.ToArray();
                }

                // Llamar al método AddProduct de la capa de negocio (ProductService)
                var productService = new ProductService(new ProductRepository(new MySqlConnection(connectionString)));

                // Aquí se pasan los parámetros directamente en lugar de crear un objeto Product
                productService.AddProduct(
                    TxtPName.Text,              // Nombre del producto
                    TxtPDescription.Text,       // Descripción del producto
                    decimal.Parse(TxtPPrice.Text), // Precio (asumimos que tienes un TextBox para el precio)
                    int.Parse(TxtPStock.Text),  // Stock (asumimos que tienes un TextBox para el stock)
                    CbSPSatus.SelectedItem.ToString(), // Estado (extraído del ComboBox)
                    imageBytes                    // Imagen en formato byte[]
                );

                MessageBox.Show("Producto guardado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Limpiar los controles después de guardar
                TxtPName.Clear();
                TxtPDescription.Clear();
                TxtPPrice.Clear();
                TxtPStock.Clear();
                CbSPSatus.SelectedIndex = -1;
                PbPImage.Image = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}