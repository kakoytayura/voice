using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace voice
{
    public partial class Form6 : Form
    {
        private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=DbVoice.accdb;";

        private DataGridView dgvResults;
        private Button btnLoadResults;

        private void Form6_Load(object sender, EventArgs e)
        {
            InitializeComponent();
        }
        public Form6()
        {
            InitializeComponent();
            InitializeResultUI();
            this.Load += new EventHandler(this.Form6_Load);
        }

        private void InitializeResultUI()
        {
            this.Text = "Результаты опросов и голосований";
            this.Width = 700;
            this.Height = 500;

            dgvResults = new DataGridView()
            {
                Location = new Point(10, 10),
                Size = new Size(660, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };
            this.Controls.Add(dgvResults);

            btnLoadResults = new Button()
            {
                Text = "Загрузить результаты",
                Location = new Point(10, 420),
                Width = 150
            };
            btnLoadResults.Click += BtnLoadResults_Click;
            this.Controls.Add(btnLoadResults);
        }

        private void BtnLoadResults_Click(object sender, EventArgs e)
        {
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                    SELECT q.QuestionText, ao.AnswerText, ao.VoteCount
                    FROM Questions q
                    INNER JOIN AnswerOptions ao ON q.QuestionID = ao.QuestionID
                    ORDER BY q.QuestionID, ao.AnswerOptionID";

                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvResults.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки результатов: " + ex.Message);
            }
        }
    }
}
