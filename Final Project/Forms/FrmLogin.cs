using System;
using System.Windows.Forms;
using DataLayer.Connection;
using DataLayer.Implementations;
using BusinessLayer.Implementations;
using Final_Project.Forms;

namespace Final_Project
{
    public partial class FrmLogin : Form
    {
        private readonly ClientService _clientService;

        public FrmLogin()
        {
            InitializeComponent();

            try
            {
                // 🔹 Inicializa la conexión y las capas de negocio
                DbConnection dbConnection = new DbConnection();
                ClientRepository clientRepository = new ClientRepository(dbConnection);
                _clientService = new ClientService(clientRepository);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inicializando servicios: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            if (_clientService == null)
            {
                MessageBox.Show("No se pudo inicializar el servicio de clientes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 🔹 Pasa la instancia de ClientService al formulario de registro
            FrmRegister registerForm = new FrmRegister(_clientService);
            registerForm.Show();
            this.Hide();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (_clientService == null)
            {
                MessageBox.Show("No se pudo inicializar el servicio de clientes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string username = TxtUsername.Text;
            string password = TxtPassword.Text;

            try
            {
                string userRole = _clientService.LoginUser(username, password);

                if (userRole == "admin")
                {
                    FrmAdminDashboard adminDashboard = new FrmAdminDashboard();
                    adminDashboard.Show();
                    this.Hide(); 
                }
                else if (userRole == "user")
                {
                    FrmHome mainForm = new FrmHome();
                    mainForm.Show();
                    this.Hide(); 
                }
                else
                {
                    MessageBox.Show("Credenciales inválidas o el usuario está inactivo.", "Error de Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message, "Error de Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
