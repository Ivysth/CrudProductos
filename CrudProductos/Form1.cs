using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using MySql.Data.MySqlClient;


namespace CrudProductos
{
    public partial class Form1 : Form
    {
        // CAMBIA estos datos por los de tu MySQL.
        private readonly string cadenaConexion =
            "Server=localhost;Database=laboratorio_crud;Uid=root;Pwd=0606FSA;";

        public Form1()
        {
            InitializeComponent();

            // Conectar los botones a sus eventos.
            btnGuardar.Click += btnGuardar_Click;
            btnConsultar.Click += btnConsultar_Click;
            btnEliminar.Click += btnEliminar_Click;
            btnLimpiar.Click += btnLimpiar_Click;

            // btnActualizar ya está conectado en el Designer.
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConsultarProductos();
        }

        private MySqlConnection CrearConexion()
        {
            return new MySqlConnection(cadenaConexion);
        }

        private bool ValidarDatos()
        {
            decimal precio;
            int existencias;

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Escribe el nombre del producto.");
                txtNombre.Focus();
                return false;
            }

            if (lstCategoria.SelectedItem == null)
            {
                MessageBox.Show("Selecciona una categoría.");
                return false;
            }

            if (!decimal.TryParse(
                txtPrecio.Text.Trim(),
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out precio) &&
                !decimal.TryParse(
                txtPrecio.Text.Trim(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out precio))
            {
                MessageBox.Show("Escribe un precio válido.");
                txtPrecio.Focus();
                return false;
            }

            if (precio < 0)
            {
                MessageBox.Show("El precio no puede ser negativo.");
                return false;
            }

            if (!int.TryParse(txtExistencias.Text.Trim(), out existencias)
                || existencias < 0)
            {
                MessageBox.Show(
                    "Las existencias deben ser un número entero igual o mayor que cero.");
                txtExistencias.Focus();
                return false;
            }

            return true;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos())
                return;

            const string sql =
                @"INSERT INTO productos
                  (nombre, categoria, precio, existencias)
                  VALUES (@nombre, @categoria, @precio, @existencias)";

            try
            {
                using (MySqlConnection conexion = CrearConexion())
                using (MySqlCommand comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", txtNombre.Text.Trim());
                    comando.Parameters.AddWithValue(
                        "@categoria", lstCategoria.SelectedItem.ToString());

                    decimal precio;
                    if (!decimal.TryParse(txtPrecio.Text.Trim(),
                        NumberStyles.Number, CultureInfo.CurrentCulture, out precio))
                    {
                        decimal.TryParse(txtPrecio.Text.Trim(),
                            NumberStyles.Number, CultureInfo.InvariantCulture,
                            out precio);
                    }

                    int existencias = int.Parse(txtExistencias.Text.Trim());

                    comando.Parameters.AddWithValue("@precio", precio);
                    comando.Parameters.AddWithValue("@existencias", existencias);

                    conexion.Open();
                    comando.ExecuteNonQuery();
                }

                lblMensaje.Text = "Producto guardado correctamente.";
                LimpiarCampos();
                ConsultarProductos();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error de MySQL al guardar: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar el producto: " + ex.Message);
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            ConsultarProductos();
        }

        private void ConsultarProductos()
        {
            const string sql =
                @"SELECT id, nombre, categoria, precio, existencias
                  FROM productos
                  ORDER BY id DESC";

            try
            {
                lstRegistros.Items.Clear();

                using (MySqlConnection conexion = CrearConexion())
                using (MySqlCommand comando = new MySqlCommand(sql, conexion))
                {
                    conexion.Open();

                    using (MySqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            Producto producto = new Producto
                            {
                                Id = Convert.ToInt32(lector["id"]),
                                Nombre = lector["nombre"].ToString(),
                                Categoria = lector["categoria"].ToString(),
                                Precio = Convert.ToDecimal(lector["precio"]),
                                Existencias = Convert.ToInt32(lector["existencias"])
                            };

                            lstRegistros.Items.Add(producto);
                        }
                    }
                }

                lblMensaje.Text =
                    "Productos encontrados: " + lstRegistros.Items.Count;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "No se pudieron consultar los productos. " +
                    "Revisa la conexión y los nombres de la tabla y columnas.\n\n" +
                    ex.Message);
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            Producto seleccionado = lstRegistros.SelectedItem as Producto;

            if (seleccionado == null)
            {
                MessageBox.Show(
                    "Selecciona un producto de la lista que deseas actualizar.");
                return;
            }

            if (!ValidarDatos())
                return;

            const string sql =
                @"UPDATE productos
                  SET nombre = @nombre,
                      categoria = @categoria,
                      precio = @precio,
                      existencias = @existencias
                  WHERE id = @id";

            try
            {
                decimal precio;
                if (!decimal.TryParse(txtPrecio.Text.Trim(),
                    NumberStyles.Number, CultureInfo.CurrentCulture, out precio))
                {
                    decimal.TryParse(txtPrecio.Text.Trim(),
                        NumberStyles.Number, CultureInfo.InvariantCulture,
                        out precio);
                }

                using (MySqlConnection conexion = CrearConexion())
                using (MySqlCommand comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", txtNombre.Text.Trim());
                    comando.Parameters.AddWithValue(
                        "@categoria", lstCategoria.SelectedItem.ToString());
                    comando.Parameters.AddWithValue("@precio", precio);
                    comando.Parameters.AddWithValue(
                        "@existencias", int.Parse(txtExistencias.Text.Trim()));
                    comando.Parameters.AddWithValue("@id", seleccionado.Id);

                    conexion.Open();
                    int filas = comando.ExecuteNonQuery();

                    lblMensaje.Text = filas > 0
                        ? "Producto actualizado correctamente."
                        : "No se realizaron cambios.";
                }

                LimpiarCampos();
                ConsultarProductos();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error de MySQL al actualizar: " + ex.Message);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            Producto seleccionado = lstRegistros.SelectedItem as Producto;

            if (seleccionado == null)
            {
                MessageBox.Show("Selecciona un producto que deseas eliminar.");
                return;
            }

            if (!chkConfirmarEliminar.Checked)
            {
                MessageBox.Show(
                    "Marca la casilla de confirmación antes de eliminar.");
                return;
            }

            DialogResult respuesta = MessageBox.Show(
                "¿Deseas eliminar el producto " + seleccionado.Nombre + "?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (respuesta != DialogResult.Yes)
                return;

            const string sql = "DELETE FROM productos WHERE id = @id";

            try
            {
                using (MySqlConnection conexion = CrearConexion())
                using (MySqlCommand comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@id", seleccionado.Id);
                    conexion.Open();
                    comando.ExecuteNonQuery();
                }

                lblMensaje.Text = "Producto eliminado correctamente.";
                LimpiarCampos();
                ConsultarProductos();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error de MySQL al eliminar: " + ex.Message);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
            lblMensaje.Text = "Campos limpiados.";
        }

        private void LimpiarCampos()
        {
            txtNombre.Clear();
            txtPrecio.Clear();
            txtExistencias.Clear();
            lstCategoria.ClearSelected();
            lstRegistros.ClearSelected();
            chkConfirmarEliminar.Checked = false;
            txtNombre.Focus();
        }

        private void lstRegistros_SelectedIndexChanged(
            object sender, EventArgs e)
        {
            Producto seleccionado = lstRegistros.SelectedItem as Producto;

            if (seleccionado == null)
                return;

            txtNombre.Text = seleccionado.Nombre;
            txtPrecio.Text = seleccionado.Precio.ToString(
                CultureInfo.CurrentCulture);
            txtExistencias.Text = seleccionado.Existencias.ToString();

            int indice = lstCategoria.FindStringExact(seleccionado.Categoria);
            if (indice >= 0)
                lstCategoria.SelectedIndex = indice;
        }

        // Estos eventos se conservan porque el Designer los tiene conectados.
        private void label1_Click(object sender, EventArgs e) { }
        private void lstCategoria_SelectedIndexChanged(object sender, EventArgs e) { }
        private void lblTitulo_Click(object sender, EventArgs e) { }
        private void label1_Click_1(object sender, EventArgs e) { }
        private void lblNombre_Click(object sender, EventArgs e) { }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }

   
    
}
