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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace voice
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string Username = textBox1.Text.Trim();
            string Password = textBox2.Text.Trim();
            

            if (string.IsNullOrEmpty(Username) ||
                string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!UserExists(Username, Password))
                {
                    AddUser(Username, Password);
                    MessageBox.Show("Новый пользователь успешно зарегистрирован.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                Form8 newForm = new Form8();
                newForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при работе с базой данных:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool UserExists(string Username, string Password)
        {
            bool exists = false;
            string query = "SELECT COUNT(*) FROM Users WHERE Username = ? AND Password = ?";

            using (var connection = DbCon.GetConnection())
            {
                connection.Open();
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("?", Username);
                    command.Parameters.AddWithValue("?", Password);

                    int count = (int)command.ExecuteScalar();
                    exists = count > 0;
                }
                connection.Close();
                return exists;
            }
        }

        private void AddUser(string Username, string Password)
        {
            string query = "INSERT INTO Users ([Username], [Password]) VALUES (@Username, @Password)";

            using (var connection = DbCon.GetConnection())
            {
                connection.Open();
                using (var command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Userame", Username);
                    command.Parameters.AddWithValue("@Password", Password);
                    

                    command.ExecuteNonQuery();

                }
                connection.Close();
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
