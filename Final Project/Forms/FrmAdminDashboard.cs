using System;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Drawing;
using BusinessLayer.Implementations;
using DataLayer.Implementations;
using FeatureLayer;
using System.IO;
using System.Data;
using FeatureLayer.Entities;
using System.Collections.Generic;

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





        private void BtnSearchE_Click(object sender, EventArgs e)
        {
            int clientId;

            if (!int.TryParse(TxtSearchE.Text, out clientId))
            {
                MessageBox.Show("Por favor ingrese un ID válido.");
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SearchClientByID", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_ClientID", clientId); 

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Asignar los datos a los Labels
                            LblClientID.Text = reader["ClientID"].ToString();
                            LblClientName.Text = reader["ClientName"].ToString();
                            LblClientEmail.Text = reader["Email"].ToString();
                            LblClientPhone.Text = reader["Phone"].ToString();
                            LblClientStatus.Text = reader["Status"].ToString();
                            LblUserID.Text = reader["UserID"].ToString();


                            MessageBox.Show("Cliente encontrado.");
                        }
                        else
                        {
                            MessageBox.Show("Cliente no encontrado.");

                            // Limpiar labels si no se encuentra el cliente
                            LblClientID.Text = "";
                            LblClientName.Text = "";
                            LblClientEmail.Text = "";
                            LblClientPhone.Text = "";
                            LblClientStatus.Text = "";
                            LblUserID.Text = "";


                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al buscar cliente: " + ex.Message);
                }
            }
        }



        private void BtnDeleteE_Click(object sender, EventArgs e)
        {
            // Validar que el CheckBox esté marcado
            if (!CheckboxE.Checked)
            {
                MessageBox.Show("Por favor confirme la eliminación marcando la casilla.");
                return;
            }

            int clientId;

            if (!int.TryParse(TxtSearchE.Text, out clientId))
            {
                MessageBox.Show("Por favor ingrese un ID válido para eliminar.");
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("DeactivateClient", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_ClientID", clientId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string message = reader["Message"].ToString();
                            MessageBox.Show(message);

                            // Actualizar el estado en la interfaz
                            LblClientStatus.Text = "inactive";
                        }
                        else
                        {
                            MessageBox.Show("No se recibió respuesta del servidor.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al desactivar cliente: " + ex.Message);
                }
            }
        }

        private void BtnSearchAll_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand("SearchAllInactiveClients", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    DgvInactives.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar los clientes inactivos: " + ex.Message);
                }
            }
        }

        private void BtnSearchA_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Llamamos al procedimiento almacenado que retorna todos los clientes inactivos
                    MySqlCommand cmd = new MySqlCommand("SearchInactiveClientIDs", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) // Mostrar solo el primer cliente inactivo encontrado
                        {
                            // Asignar los datos a los Labels
                            LblClientIDA.Text = reader["ClientID"].ToString();
                            LblClientNameA.Text = reader["ClientName"].ToString();
                            LblClientEmailA.Text = reader["Email"].ToString();
                            LblClientPhoneA.Text = reader["Phone"].ToString();
                            LblClientStatusA.Text = reader["Status"].ToString();
                            LblUserIDA.Text = reader["UserID"].ToString();

                            MessageBox.Show("Cliente inactivo encontrado.");
                        }
                        else
                        {
                            LimpiarLabelsCliente();
                            MessageBox.Show("No hay clientes inactivos.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al buscar clientes inactivos: " + ex.Message);
                }
            }
        }

        private void LimpiarLabelsCliente()
        {
            LblClientIDA.Text = "";
            LblClientNameA.Text = "";
            LblClientEmailA.Text = "";
            LblClientPhoneA.Text = "";
            LblClientStatusA.Text = "";
            LblUserIDA.Text = "";
        }



        private void BtnActivateClient_Click(object sender, EventArgs e)
        {
            int clientId;

            if (!int.TryParse(TxtClientID.Text, out clientId))  // Suponiendo que tienes un cuadro de texto para el ClientID
            {
                MessageBox.Show("Por favor ingrese un ID válido.");
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("ActivateClient", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_ClientID", clientId);  // Pasa el ClientID al procedimiento

                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())  // Si el procedimiento devuelve el mensaje
                    {
                        MessageBox.Show(reader["Message"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al activar cliente: " + ex.Message);
                }
            }
        }

        private void BtnSearchByName_Click(object sender, EventArgs e)
        {
            string clientName = TxtSearchClietName.Text;  // Asumiendo que el nombre del cliente se ingresa en un TextBox

            if (string.IsNullOrWhiteSpace(clientName))
            {
                MessageBox.Show("Por favor ingrese un nombre de cliente.");
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Ejecutar el procedimiento almacenado
                    MySqlCommand cmd = new MySqlCommand("SearchClientByName", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_ClientName", clientName);

                    // Crear un DataTable para almacenar los resultados
                    DataTable dt = new DataTable();

                    // Usar un DataAdapter para llenar el DataTable
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(dt);

                    // Asignar el DataTable al DataGridView
                    DgvSearchByName.DataSource = dt;

                    // Verificar si hay resultados
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No se encontraron clientes con ese nombre.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al buscar clientes: " + ex.Message);
                }
            }
        }

        private void BtnSearchIDM_Click(object sender, EventArgs e)
        {
            int clientId;

            // Verificar si el ID del cliente es válido
            if (!int.TryParse(TxtClientIDM.Text, out clientId))
            {
                MessageBox.Show("Por favor ingrese un ID de cliente válido.");
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Llamar al procedimiento almacenado GetClientAndRoleByID
                    MySqlCommand cmd = new MySqlCommand("GetClientAndRoleByID", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_ClientID", clientId);

                    // Crear un lector para obtener los datos
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Mostrar los datos en los TextBox
                            TxtClientNameM.Text = reader["ClientName"].ToString();
                            TxtClientEmailM.Text = reader["Email"].ToString();
                            TxtClientPhoneM.Text = reader["Phone"].ToString();
                            CmbRoleM.SelectedItem = reader["Role"].ToString();  // Asumiendo que tienes un ComboBox con los roles

                            MessageBox.Show("Cliente encontrado.");
                        }
                        else
                        {
                            MessageBox.Show("Cliente no encontrado.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al buscar cliente: " + ex.Message);
                }
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            // Verificar si el ID del cliente es válido
            int clientId;
            if (!int.TryParse(TxtClientIDM.Text, out clientId))
            {
                MessageBox.Show("Por favor ingrese un ID de cliente válido.");
                return;
            }

            // Verificar si el ComboBox tiene un valor seleccionado
            if (CmbRoleM.SelectedItem == null)
            {
                MessageBox.Show("Por favor seleccione un rol para el cliente.");
                return;
            }

            string clientName = TxtClientNameM.Text;
            string clientEmail = TxtClientEmailM.Text;
            string clientPhone = TxtClientPhoneM.Text;
            string userRole = CmbRoleM.SelectedItem.ToString(); // Obtener el rol seleccionado en el ComboBox

            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Llamar al procedimiento almacenado ModifyClient
                    MySqlCommand cmd = new MySqlCommand("ModifyClient", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Añadir los parámetros al procedimiento
                    cmd.Parameters.AddWithValue("p_ClientID", clientId);
                    cmd.Parameters.AddWithValue("p_ClientName", clientName);
                    cmd.Parameters.AddWithValue("p_ClientEmail", clientEmail);
                    cmd.Parameters.AddWithValue("p_ClientPhone", clientPhone);
                    cmd.Parameters.AddWithValue("p_UserRole", userRole);

                    // Ejecutar el procedimiento
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Los datos del cliente y su rol se han modificado correctamente.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al modificar el cliente: " + ex.Message);
                }
            }
        }


    }
}
