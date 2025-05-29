using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using QRCoder;

namespace pw11
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private TextBox txtId = new TextBox();
        private TextBox txtName = new TextBox();
        private ComboBox comboType = new ComboBox();
        private DateTimePicker datePicker = new DateTimePicker();
        private TextBox txtNotes = new TextBox();
        private Button btnAdd = new Button();
        private Button btnUpdate = new Button();
        private Button btnDelete = new Button();
        private Button btnClear = new Button();
        private Button btnQR = new Button();
        private DataGridView dataGridView = new DataGridView();

        private string dbPath = "Data Source=samples.db";

        public MainForm()
        {
            this.Text = "Biobaza - Pr�bki biologiczne";
            this.Size = new Size(800, 600);
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            InitUI();
            InitializeDatabase();
            LoadSamples();
        }

        private void InitUI()
        {
            Label lblId = new Label() { Text = "ID:", Left = 10, Top = 10, Width = 100 };
            txtId.SetBounds(120, 10, 200, 20);

            Label lblName = new Label() { Text = "Nazwa:", Left = 10, Top = 40, Width = 100 };
            txtName.SetBounds(120, 40, 200, 20);

            Label lblType = new Label() { Text = "Typ:", Left = 10, Top = 70, Width = 100 };
            comboType.SetBounds(120, 70, 200, 20);
            comboType.Items.AddRange(new string[] { "DNA", "RNA", "Bia�ko", "Inny" });

            Label lblDate = new Label() { Text = "Data pobrania:", Left = 10, Top = 100, Width = 100 };
            datePicker.SetBounds(120, 100, 200, 20);

            Label lblNotes = new Label() { Text = "Uwagi:", Left = 10, Top = 130, Width = 100 };
            txtNotes.SetBounds(120, 130, 200, 60);
            txtNotes.Multiline = true;

            btnAdd.Text = "Dodaj";
            btnAdd.SetBounds(10, 200, 95, 30);
            btnAdd.Click += BtnAdd_Click;

            btnUpdate.Text = "Zapisz zmiany";
            btnUpdate.SetBounds(110, 200, 110, 30);
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete.Text = "Usu�";
            btnDelete.SetBounds(225, 200, 95, 30);
            btnDelete.Click += BtnDelete_Click;

            btnClear.Text = "Wyczy��";
            btnClear.SetBounds(325, 200, 95, 30);
            btnClear.Click += BtnClear_Click;

            btnQR.Text = "Generuj QR";
            btnQR.SetBounds(425, 200, 110, 30);
            btnQR.Click += BtnQR_Click;

            foreach (var btn in new[] { btnAdd, btnUpdate, btnDelete, btnClear, btnQR })
            {
                btn.FlatStyle = FlatStyle.System;
                btn.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            }

            dataGridView.SetBounds(10, 250, 760, 300);
            dataGridView.ReadOnly = true;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.CellClick += DataGridView_CellClick;

            this.Controls.AddRange(new Control[] {
                lblId, txtId, lblName, txtName,
                lblType, comboType, lblDate, datePicker,
                lblNotes, txtNotes,
                btnAdd, btnUpdate, btnDelete, btnClear, btnQR,
                dataGridView
            });
        }

        private void InitializeDatabase()
        {
            using (var conn = new SQLiteConnection(dbPath))
            {
                conn.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS Samples (
                                Id TEXT PRIMARY KEY,
                                Name TEXT NOT NULL,
                                Type TEXT NOT NULL,
                                Date TEXT NOT NULL,
                                Notes TEXT
                            )";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }

        private void LoadSamples()
        {
            using (var conn = new SQLiteConnection(dbPath))
            {
                conn.Open();
                var adapter = new SQLiteDataAdapter("SELECT * FROM Samples", conn);
                var table = new DataTable();
                adapter.Fill(table);
                dataGridView.DataSource = table;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var conn = new SQLiteConnection(dbPath))
            {
                conn.Open();
                string sql = "INSERT INTO Samples (Id, Name, Type, Date, Notes) VALUES (@id, @name, @type, @date, @notes)";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", txtId.Text);
                cmd.Parameters.AddWithValue("@name", txtName.Text);
                cmd.Parameters.AddWithValue("@type", comboType.Text);
                cmd.Parameters.AddWithValue("@date", datePicker.Value.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@notes", txtNotes.Text);

                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Dodano pr�bk�.");
                    LoadSamples();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("B��d: " + ex.Message);
                }
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            using (var conn = new SQLiteConnection(dbPath))
            {
                conn.Open();
                string sql = @"UPDATE Samples 
                               SET Name = @name, Type = @type, Date = @date, Notes = @notes
                               WHERE Id = @id";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", txtId.Text);
                cmd.Parameters.AddWithValue("@name", txtName.Text);
                cmd.Parameters.AddWithValue("@type", comboType.Text);
                cmd.Parameters.AddWithValue("@date", datePicker.Value.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@notes", txtNotes.Text);

                try
                {
                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                        MessageBox.Show("Zaktualizowano pr�bk�.");
                    else
                        MessageBox.Show("Nie znaleziono pr�bki.");

                    LoadSamples();
                    txtId.Enabled = true;
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("B��d: " + ex.Message);
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (txtId.Text == "") return;

            var confirm = MessageBox.Show("Czy na pewno usun�� t� pr�bk�?", "Potwierd�", MessageBoxButtons.YesNo);
            if (confirm == DialogResult.No) return;

            using (var conn = new SQLiteConnection(dbPath))
            {
                conn.Open();
                string sql = "DELETE FROM Samples WHERE Id = @id";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", txtId.Text);

                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Usuni�to pr�bk�.");
                    LoadSamples();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("B��d: " + ex.Message);
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtId.Text = "";
            txtName.Text = "";
            comboType.SelectedIndex = -1;
            datePicker.Value = DateTime.Now;
            txtNotes.Text = "";
            txtId.Enabled = true;
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView.Rows[e.RowIndex];
                txtId.Text = row.Cells["Id"].Value.ToString();
                txtName.Text = row.Cells["Name"].Value.ToString();
                comboType.Text = row.Cells["Type"].Value.ToString();
                datePicker.Value = DateTime.Parse(row.Cells["Date"].Value.ToString());
                txtNotes.Text = row.Cells["Notes"].Value.ToString();
                txtId.Enabled = false;
            }
        }

        private void BtnQR_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtId.Text))
            {
                MessageBox.Show("Najpierw wybierz lub dodaj pr�bk�.");
                return;
            }

            string qrContent = $"ID: {txtId.Text}\nNazwa: {txtName.Text}\nTyp: {comboType.Text}\nData: {datePicker.Value:yyyy-MM-dd}\nUwagi: {txtNotes.Text}";

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q))
            using (QRCode qrCode = new QRCode(qrCodeData))
            using (Bitmap qrImage = qrCode.GetGraphic(20))
            {
                string fileName = $"QR_{txtId.Text}.png";
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                qrImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

                MessageBox.Show($"Zapisano kod QR jako:\n{fileName}");
            }
        }
    }
}