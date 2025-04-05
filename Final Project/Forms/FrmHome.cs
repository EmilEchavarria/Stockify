using BusinessLayer.Implementations;
using DataLayer.Implementations;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FeatureLayer;
using System.Data.Common;

namespace Final_Project.Forms
{
    public partial class FrmHome : Form
    {
        private readonly ProductService _productService;

        public FrmHome()
        {
            InitializeComponent();

            // Crear la conexión y pasarla al repositorio
            var connection = new MySqlConnection("Server=localhost;Port=3306;Database=Stockify;User Id=root;Password=Escogido5002.@HTML;");
            var productRepository = new ProductRepository(connection);
            _productService = new ProductService(productRepository);
        }

        private void BtnPSearch_Click(object sender, EventArgs e)
        {
            string searchName = TxtPName.Text.Trim(); // Asegúrate de tener un TextBox llamado TxtSearch
            var products = _productService.SearchProductsByName(searchName);

            // Mostrar los resultados en el DataGridView
            DgvProducts.DataSource = products;
        }
    }
}
