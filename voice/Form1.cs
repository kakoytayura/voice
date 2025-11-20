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

namespace voice
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 newForm = new Form2();
            newForm.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string Username = textBox1.Text.Trim();
            string Password = textBox2.Text.Trim();

            if (string.IsNullOrEmpty(Username))
            {
                MessageBox.Show("Введите имя.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Введите пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (UserExists(Username, Password))
            {
                this.Hide();
                if (Username == "Qwerty" || Password == "123456q")
                {
                    Form5 adminForm = new Form5();
                    adminForm.Show();
                }
                else if (Username == "frogg123" || Password == "quaaa243")
                {
                    Form9 userForm = new Form9();
                    userForm.Show();
                }
                else if (Username == "garryon" || Password == "grgrgroff")
                {
                    Form7 userForm = new Form7();
                    userForm.Show();
                }
                else
                {
                    Form8 userForm = new Form8();
                    userForm.Show();
                }
            }
            else
            {
                MessageBox.Show("Пользователь с таким именем и паролем не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        List<Question> surveyQuestions = new List<Question>();
        private bool UserExists(string Username, string Password)
        {
            bool exists = false;
            string query = "SELECT COUNT(*) FROM Users WHERE Username = ? AND Password = ?";

            try
            {
                using (var connection = DbCon.GetConnection())
                {
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("?", Username);
                        command.Parameters.AddWithValue("?", Password);

                        connection.Open();
                        int count = (int)command.ExecuteScalar();
                        exists = count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении к базе данных:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return exists;
        }

        

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }

}
