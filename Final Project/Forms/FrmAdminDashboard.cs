using System;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

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

    }
}