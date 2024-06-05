using MaterialSkin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace VeteriLife_Prototipo
{
    public partial class Form1 : MaterialSkin.Controls.MaterialForm
    {
        private string connectionString = "Data Source=PCFLASH\\SQLEXPRESS;Initial Catalog=VETERILIFE;Integrated Security=True;Encrypt=False";
        public Form1()
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey900, Primary.BlueGrey900, Primary.BlueGrey500, Accent.Amber700, TextShade.WHITE);

            CargarComboBoxes();
            CargarMedicamentosEnListView();

            // Suscribirse al evento SelectedIndexChanged
            this.ListViewMedicamentos.SelectedIndexChanged += new System.EventHandler(this.ListViewMedicamentos_SelectedIndexChanged);
        }

        private void CargarComboBoxes()
        {
            CargarComboBox(ComboBoxVia, "ViasAdministracion");
            CargarComboBox(ComboBoxTipo, "TiposMedicamentos");
            CargarComboBox(ComboBoxSituacion, "SituacionesMedicas");
            CargarComboBox(ComboBoxAnimal, "Animales");
            CargarComboBox(ComboBoxMarca, "Marcas");
        }

        private void CargarComboBox(ComboBox comboBox, string tabla)
        {
            comboBox.Items.Clear(); // Limpia el ComboBox antes de cargar nuevos datos
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                string query = $"SELECT nombre FROM {tabla}";

                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    try
                    {
                        cn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            comboBox.Items.Add(reader["nombre"].ToString());
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private int ObtenerIdPorNombre(string nombre, string tabla, string columna)
        {
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                string query = $"SELECT {columna} FROM {tabla} WHERE nombre = @nombre";
                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombre);

                    try
                    {
                        cn.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            return Convert.ToInt32(result);
                        }
                        else
                        {
                            return -1; // Elemento no encontrado
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                        return -1;
                    }
                }
            }
        }





        private void materialButton2_Click(object sender, EventArgs e)
        {
            TextBoxNombre.Text = string.Empty;
        }

        private void CargarMedicamentosEnListView(string filtro = "", string valor = "")
        {
            ListViewMedicamentos.Items.Clear();

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        m.id_medicamento, m.nombre, ma.nombre AS marca, v.nombre AS via, 
                        t.nombre AS tipo, s.nombre AS situacion, a.nombre AS animal, 
                        m.ingredientes, m.descripcion
                    FROM MedicamentosAnimales m
                    JOIN Marcas ma ON m.id_marca = ma.id_marca
                    JOIN ViasAdministracion v ON m.id_via = v.id_via
                    JOIN TiposMedicamentos t ON m.id_tipo = t.id_tipo
                    JOIN SituacionesMedicas s ON m.id_situacion = s.id_situacion
                    JOIN Animales a ON m.id_animal = a.id_animal";

                if (!string.IsNullOrEmpty(filtro) && !string.IsNullOrEmpty(valor))
                {
                    query += $" WHERE {filtro} LIKE @valor";
                }

                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    if (!string.IsNullOrEmpty(filtro) && !string.IsNullOrEmpty(valor))
                    {
                        cmd.Parameters.AddWithValue("@valor", "%" + valor + "%");
                    }

                    try
                    {
                        cn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            ListViewItem item = new ListViewItem(reader["nombre"].ToString());
                            item.Tag = reader["id_medicamento"]; // Guardar el ID del medicamento en el Tag
                            item.SubItems.Add(reader["marca"].ToString());
                            item.SubItems.Add(reader["via"].ToString());
                            item.SubItems.Add(reader["tipo"].ToString());
                            item.SubItems.Add(reader["situacion"].ToString());
                            item.SubItems.Add(reader["animal"].ToString());
                            item.SubItems.Add(reader["ingredientes"].ToString());

                            ListViewMedicamentos.Items.Add(item);
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }



        private void BtRegistrar_Click(object sender, EventArgs e)
        {
            int idMarca = ObtenerIdPorNombre(ComboBoxMarca.SelectedItem.ToString(), "Marcas", "id_marca");
            if (idMarca == -1)
            {
                MessageBox.Show("Marca no encontrada. Por favor, verifique el nombre de la marca.");
                return;
            }

            int idVia = ObtenerIdPorNombre(ComboBoxVia.SelectedItem.ToString(), "ViasAdministracion", "id_via");
            if (idVia == -1)
            {
                MessageBox.Show("Vía de administración no encontrada. Por favor, seleccione una vía válida.");
                return;
            }

            int idTipo = ObtenerIdPorNombre(ComboBoxTipo.SelectedItem.ToString(), "TiposMedicamentos", "id_tipo");
            if (idTipo == -1)
            {
                MessageBox.Show("Tipo de medicamento no encontrado. Por favor, seleccione un tipo válido.");
                return;
            }

            int idSituacion = ObtenerIdPorNombre(ComboBoxSituacion.SelectedItem.ToString(), "SituacionesMedicas", "id_situacion");
            if (idSituacion == -1)
            {
                MessageBox.Show("Situación médica no encontrada. Por favor, seleccione una situación válida.");
                return;
            }

            int idAnimal = ObtenerIdPorNombre(ComboBoxAnimal.SelectedItem.ToString(), "Animales", "id_animal");
            if (idAnimal == -1)
            {
                MessageBox.Show("Animal no encontrado. Por favor, seleccione un animal válido.");
                return;
            }

            string ingredientes = TextBoxIngredientes.Text; // Obtener el texto de los ingredientes
            string nombre = TextBoxNombre.Text; // Obtener el texto de los nombres

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO MedicamentosAnimales (nombre, id_marca, id_via, id_tipo, id_situacion, id_animal, descripcion, ingredientes) " +
                               "VALUES (@nombre, @id_marca, @id_via, @id_tipo, @id_situacion, @id_animal, @descripcion, @ingredientes)";

                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@id_marca", idMarca);
                    cmd.Parameters.AddWithValue("@id_via", idVia);
                    cmd.Parameters.AddWithValue("@id_tipo", idTipo);
                    cmd.Parameters.AddWithValue("@id_situacion", idSituacion);
                    cmd.Parameters.AddWithValue("@id_animal", idAnimal);
                    cmd.Parameters.AddWithValue("@descripcion", MultiLineDescripcion.Text);
                    cmd.Parameters.AddWithValue("@ingredientes", ingredientes);

                    try
                    {
                        cn.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Medicamento registrado exitosamente.");
                        CargarMedicamentosEnListView(); // Refrescar el ListView
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }



        public class ComboboxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        private void materialFloatingActionButton1_Click(object sender, EventArgs e)
        {
            string filtro = "";
            string valor = TextBoxBuscar.Text.Trim();

            if (RadioButtonNombre.Checked)
            {
                filtro = "m.nombre";
            }
            else if (RadioButtonMarca.Checked)
            {
                filtro = "ma.nombre";
            }
            else if (RadioButtonVia.Checked)
            {
                filtro = "v.nombre";
            }
            else if (RadioButtonTipo.Checked)
            {
                filtro = "t.nombre";
            }
            else if (RadioButtonSituacion.Checked)
            {
                filtro = "s.nombre";
            }
            else if (RadioButtonAnimal.Checked)
            {
                filtro = "a.nombre";
            }

            CargarMedicamentosEnListView(filtro, valor);
        }

        private void ListViewMedicamentos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ListViewMedicamentos.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = ListViewMedicamentos.SelectedItems[0];
                int idMedicamento = (int)selectedItem.Tag; // Obtener el ID del medicamento desde el Tag

                MostrarDescripcion(idMedicamento);
            }
        }

        private void MostrarDescripcion(int idMedicamento)
        {
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                string query = "SELECT descripcion FROM MedicamentosAnimales WHERE id_medicamento = @id_medicamento";

                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id_medicamento", idMedicamento);

                    try
                    {
                        cn.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            TextBoxDescripcion.Text = result.ToString();
                        }
                        else
                        {
                            TextBoxDescripcion.Text = "Descripción no encontrada.";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void ButtonRegistrarMarca_Click(object sender, EventArgs e)
        {
            string nuevaMarca = TextBoxNuevaMarca.Text.Trim();

            if (string.IsNullOrEmpty(nuevaMarca))
            {
                MessageBox.Show("Por favor, ingresa un nombre de marca válido.");
                return;
            }

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Marcas (nombre) VALUES (@nombre)";

                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@nombre", nuevaMarca);

                    try
                    {
                        cn.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Marca registrada exitosamente.");
                        CargarComboBox(ComboBoxMarca, "Marcas"); // Recarga el ComboBox de marcas para incluir la nueva marca
                        TextBoxNuevaMarca.Clear(); // Limpia el TextBox después de registrar
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void ButtonRegistrarAnimal_Click(object sender, EventArgs e)
        {
            string nuevoAnimal = TextBoxNuevoAnimal.Text.Trim();

            if (string.IsNullOrEmpty(nuevoAnimal))
            {
                MessageBox.Show("Por favor, ingresa un nombre de animal válido.");
                return;
            }

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Animales (nombre) VALUES (@nombre)";

                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@nombre", nuevoAnimal);

                    try
                    {
                        cn.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Animal registrado exitosamente.");
                        CargarComboBox(ComboBoxAnimal, "Animales"); // Recarga el ComboBox de animales
                        TextBoxNuevoAnimal.Clear(); // Limpia el TextBox después de registrar
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void ButtonRegistrarSituacion_Click(object sender, EventArgs e)
        {
            string nuevaSituacion = TextBoxNuevaSituacion.Text.Trim();

            if (string.IsNullOrEmpty(nuevaSituacion))
            {
                MessageBox.Show("Por favor, ingresa una situación válida.");
                return;
            }

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO SituacionesMedicas (nombre) VALUES (@nombre)";

                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@nombre", nuevaSituacion);

                    try
                    {
                        cn.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Situación registrada exitosamente.");
                        CargarComboBox(ComboBoxSituacion, "SituacionesMedicas"); // Recarga el ComboBox de situaciones
                        TextBoxNuevaSituacion.Clear(); // Limpia el TextBox después de registrar
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void ButtonRegistrarVia_Click(object sender, EventArgs e)
        {
            string nuevaVia = TextBoxNuevaVia.Text.Trim();

            if (string.IsNullOrEmpty(nuevaVia))
            {
                MessageBox.Show("Por favor, ingresa un nombre de vía de administración válido.");
                return;
            }

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO ViasAdministracion (nombre) VALUES (@nombre)";

                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@nombre", nuevaVia);

                    try
                    {
                        cn.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Vía de administración registrada exitosamente.");
                        CargarComboBox(ComboBoxVia, "ViasAdministracion"); // Recarga el ComboBox de vías de administración
                        TextBoxNuevaVia.Clear(); // Limpia el TextBox después de registrar
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }
    }
}
    

