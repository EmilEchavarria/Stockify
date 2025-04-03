using System;
using System.Windows.Forms;
using FeatureLayer.Entities;
using BusinessLayer.Implementations;  // Importa la capa de negocios

namespace Final_Project.Forms
{
    public partial class FrmRegister : Form
    {
        private readonly ClientService _clientService;

        // Constructor que inyecta la capa de negocio (ClientService)
        public FrmRegister(ClientService clientService)
        {
            InitializeComponent();
            _clientService = clientService;  // Asigna la instancia del servicio
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                // Crear una instancia del usuario a partir de los datos del formulario
                User user = new User
                {
                    Username = TxtUsername.Text,
                    Password = TxtPassword.Text,
                    ClientName = TxtClientName.Text,
                    Email = TxtEmail.Text,
                    Phone = TxtPhone.Text
                };

                // Llamar al servicio para registrar al cliente
                _clientService.RegisterClient(user);

                // Mostrar un mensaje de éxito
                MessageBox.Show("Cliente registrado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FrmHome homeForm = new FrmHome();
                homeForm.Show();
                this.Hide();
            }
            catch (ArgumentException ex)
            {
                // Si ocurre algún error, mostrar el mensaje de error
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Capturar otros posibles errores
                MessageBox.Show("Ocurrió un error inesperado: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
