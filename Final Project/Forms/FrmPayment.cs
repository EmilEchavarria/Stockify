using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Final_Project.Forms
{
    public partial class FrmPayment : Form
    {
        private decimal totalAPagar;

        public FrmPayment(decimal total)
        {
            InitializeComponent();
            totalAPagar = total;
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

            // Confirmación de pago
            MessageBox.Show("¡Pago realizado exitosamente!", "Confirmación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
