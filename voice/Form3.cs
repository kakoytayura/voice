using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Data.OleDb;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace voice
{
    public partial class Form3 : Form
    {

        private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=DbVoice.accdb;";

        private List<Question> surveyQuestions = new List<Question>();

        private TextBox tbQuestion;
        private TextBox tbAnswer1;
        private TextBox tbAnswer2;
        private TextBox tbAnswer3;
        private Button btnAddQuestion;
        private Button btnShowSurvey;
        private Label lblInfo;

        private int surveyTypeId = 1;

        public Form3()
        {
            InitializeComponent();
            InitializeSurveyForm();
        }

        private void InitializeSurveyForm()
        {
            this.Text = "Создание опроса";
            this.Width = 400;
            this.Height = 320;

            Label lblQ = new Label() { Text = "Вопрос:", Location = new Point(5, 5) };
            tbQuestion = new TextBox() { Location = new Point(10, 30), Width = 360 };

            Label lblA1 = new Label() { Text = "Ответ 1:", Location = new Point(10, 60) };
            tbAnswer1 = new TextBox() { Location = new Point(10, 80), Width = 360 };

            Label lblA2 = new Label() { Text = "Ответ 2:", Location = new Point(10, 110) };
            tbAnswer2 = new TextBox() { Location = new Point(10, 130), Width = 360 };

            Label lblA3 = new Label() { Text = "Ответ 3:", Location = new Point(10, 160) };
            tbAnswer3 = new TextBox() { Location = new Point(10, 180), Width = 360 };

            btnAddQuestion = new Button() { Text = "Добавить опрос", Location = new Point(10, 210), Width = 150 };
            btnAddQuestion.Click += BtnAddQuestion_Click;

            btnShowSurvey = new Button() { Text = "Показать опросы", Location = new Point(200, 210), Width = 150 };
            btnShowSurvey.Click += BtnShowSurvey_Click;

            lblInfo = new Label() { Location = new Point(10, 250), Width = 360, Height = 60 };

            this.Controls.AddRange(new Control[] { lblQ, tbQuestion, lblA1, tbAnswer1, lblA2, tbAnswer2, lblA3, tbAnswer3, btnAddQuestion, btnShowSurvey, lblInfo });
        }

        private void BtnAddQuestion_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbQuestion.Text))
            {
                MessageBox.Show("Введите текст вопроса");
                return;
            }

            if (string.IsNullOrWhiteSpace(tbAnswer1.Text) && string.IsNullOrWhiteSpace(tbAnswer2.Text) && string.IsNullOrWhiteSpace(tbAnswer3.Text))
            {
                MessageBox.Show("Введите хотя бы один вариант ответа");
                return;
            }

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    string insertSurveySql = "INSERT INTO Surveys (Title, Type) VALUES (?, ?)";
                    using (OleDbCommand cmdSurvey = new OleDbCommand(insertSurveySql, connection))
                    {
                        cmdSurvey.Parameters.AddWithValue("?", "Опрос");
                        cmdSurvey.Parameters.AddWithValue("?", surveyTypeId);
                        cmdSurvey.ExecuteNonQuery();
                    }

                    int surveyId;
                    using (OleDbCommand cmdId = new OleDbCommand("SELECT @@IDENTITY", connection))
                    {
                        surveyId = Convert.ToInt32(cmdId.ExecuteScalar());
                    }

                    string insertQuestionSql = "INSERT INTO Questions (SurveyID, QuestionText, QuestionType) VALUES (?, ?, 1)";
                    using (OleDbCommand cmdQuestion = new OleDbCommand(insertQuestionSql, connection))
                    {
                        cmdQuestion.Parameters.AddWithValue("?", surveyId);
                        cmdQuestion.Parameters.AddWithValue("?", tbQuestion.Text.Trim());
                        cmdQuestion.ExecuteNonQuery();
                    }

                    int questionId;
                    using (OleDbCommand cmdId = new OleDbCommand("SELECT @@IDENTITY", connection))
                    {
                        questionId = Convert.ToInt32(cmdId.ExecuteScalar());
                    }

                    void InsertAnswerOption(string answer)
                    {
                        if (!string.IsNullOrWhiteSpace(answer))
                        {
                            string insertAnswerSql = "INSERT INTO AnswerOptions (QuestionID, AnswerText, VoteCount) VALUES (?, ?, 0)";
                            using (OleDbCommand cmdAnswer = new OleDbCommand(insertAnswerSql, connection))
                            {
                                cmdAnswer.Parameters.AddWithValue("?", questionId);
                                cmdAnswer.Parameters.AddWithValue("?", answer.Trim());
                                cmdAnswer.ExecuteNonQuery();
                            }
                        }
                    }

                    InsertAnswerOption(tbAnswer1.Text);
                    InsertAnswerOption(tbAnswer2.Text);
                    InsertAnswerOption(tbAnswer3.Text);

                    var question = new Question { Text = tbQuestion.Text.Trim() };
                    if (!string.IsNullOrWhiteSpace(tbAnswer1.Text)) question.Answers.Add(tbAnswer1.Text.Trim());
                    if (!string.IsNullOrWhiteSpace(tbAnswer2.Text)) question.Answers.Add(tbAnswer2.Text.Trim());
                    if (!string.IsNullOrWhiteSpace(tbAnswer3.Text)) question.Answers.Add(tbAnswer3.Text.Trim());
                    surveyQuestions.Add(question);

                    tbQuestion.Clear();
                    tbAnswer1.Clear();
                    tbAnswer2.Clear();
                    tbAnswer3.Clear();

                    lblInfo.Text = $"Добавлено вопросов: {surveyQuestions.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении данных: " + ex.Message);
            }
        }

        private void BtnShowSurvey_Click(object sender, EventArgs e)
        {
            if (surveyQuestions.Count == 0)
            {
                MessageBox.Show("Сначала добавьте вопросы");
                return;
            }

            Form4 form4 = new Form4(surveyQuestions);
            form4.Show();

            Form10 form10 = new Form10(surveyQuestions);

            Form11 form11 = new Form11(surveyQuestions);

            this.Hide();
        }




        

        private void Form3_Load(object sender, EventArgs e)
        {

        }

        
    }



}

