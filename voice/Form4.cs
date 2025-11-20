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
    public partial class Form4 : Form
    {

        public Form4(List<Question> questions)
        {
            InitializeComponent();

            this.questions = questions;
            this.Width = 400;
            this.Height = 600;
            this.Text = "Пройти опрос";

            InitializeSurveyUI();
        }

        public class Question
        {
            public string Text { get; set; }
            public int QuestionID { get; set; }
            //public List<string> Answers { get; set; } = new List<string>();
            public List<AnswerOption> Answers { get; set; } = new List<AnswerOption>();
        }
        public class AnswerOption
        {
            public int AnswerOptionID { get; set; }
            public string Text { get; set; }
        }

        public class SurveyResult
        {
            public List<List<int>> SelectedAnswersIndexes { get; set; } = new List<List<int>>();
        }
        private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=DbVoice.accdb;";

        private List<Question> questions;
        private List<List<CheckBox>> checkBoxesList = new List<List<CheckBox>>();
        public SurveyResult Result { get; private set; } = new SurveyResult();

        private TabControl tabControl;
        private Button btnSubmit;

        public Form4(List<voice.Question> surveyQuestions)
        {
            InitializeComponent();

            this.Width = 400;
            this.Height = 600;
            this.Text = "Пройти опрос";

            LoadQuestionsFromDatabase();
            InitializeSurveyUI();
        }

        public Form4()
        {
            InitializeComponent();

            this.Width = 400;
            this.Height = 600;
            this.Text = "Пройти опрос";

            LoadQuestionsFromDatabase();
            InitializeSurveyUI();
        }

        private void LoadQuestionsFromDatabase()
        {
            questions = new List<Question>();

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();

                string queryQuestions = "SELECT QuestionID, QuestionText FROM Questions WHERE QuestionType = ?";
                OleDbCommand cmdQ = new OleDbCommand(queryQuestions, connection);

                cmdQ.Parameters.Add("?", System.Data.OleDb.OleDbType.Integer).Value = 1;

                using (OleDbDataReader readerQ = cmdQ.ExecuteReader())
                {
                    while (readerQ.Read())
                    {
                        var question = new Question
                        {
                            QuestionID = readerQ.GetInt32(0),
                            Text = readerQ.GetString(1),
                            Answers = new List<AnswerOption>()
                        };

                        string queryAnswers = "SELECT AnswerOptionID, AnswerText FROM AnswerOptions WHERE QuestionID = ? ORDER BY AnswerOptionID";
                        OleDbCommand cmdA = new OleDbCommand(queryAnswers, connection);

                        cmdA.Parameters.Add("?", System.Data.OleDb.OleDbType.Integer).Value = question.QuestionID;

                        using (OleDbDataReader readerA = cmdA.ExecuteReader())
                        {
                            while (readerA.Read())
                            {
                                question.Answers.Add(new AnswerOption
                                {
                                    AnswerOptionID = readerA.GetInt32(0),
                                    Text = readerA.GetString(1)
                                });
                            }
                        }

                        questions.Add(question);
                    }
                }
            }
        }

        private void InitializeSurveyUI()
        {
            tabControl = new TabControl()
            {
                Location = new Point(10, 10),
                Size = new Size(360, 500),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            this.Controls.Add(tabControl);

            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];

                TabPage tabPage = new TabPage($"Вопрос {i + 1}");

                int y = 10;

                Label lbl = new Label()
                {
                    Text = question.Text,
                    Location = new Point(10, y),
                    AutoSize = true,
                    Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold)
                };
                tabPage.Controls.Add(lbl);
                y += 30;

                List<CheckBox> checkBoxes = new List<CheckBox>();

                for (int j = 0; j < question.Answers.Count; j++)
                {
                    CheckBox cb = new CheckBox()
                    {
                        Text = question.Answers[j].Text,
                        Location = new Point(20, y),
                        AutoSize = true
                    };
                    tabPage.Controls.Add(cb);
                    checkBoxes.Add(cb);
                    y += 25;
                }

                checkBoxesList.Add(checkBoxes);
                tabControl.TabPages.Add(tabPage);
            }

            btnSubmit = new Button()
            {
                Text = "Отправить",
                Location = new Point(10, 520),
                Width = 100
            };
            btnSubmit.Click += BtnSubmit_Click;
            this.Controls.Add(btnSubmit);
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            Result.SelectedAnswersIndexes.Clear();

            for (int i = 0; i < checkBoxesList.Count; i++)
            {
                List<CheckBox> checkBoxes = checkBoxesList[i];
                List<int> selectedIndexes = new List<int>();

                for (int j = 0; j < checkBoxes.Count; j++)
                {
                    if (checkBoxes[j].Checked)
                    {
                        selectedIndexes.Add(j);
                    }
                }

                if (selectedIndexes.Count == 0)
                {
                    MessageBox.Show($"Пожалуйста, выберите хотя бы один ответ на вопрос #{i + 1}");
                    return;
                }

                Result.SelectedAnswersIndexes.Add(selectedIndexes);
            }

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    for (int i = 0; i < Result.SelectedAnswersIndexes.Count; i++)
                    {
                        foreach (int answerIndex in Result.SelectedAnswersIndexes[i])
                        {
                            string getAnswerOptionIdSql = @"SELECT AnswerOptionID FROM AnswerOptions 
                                                    WHERE QuestionID = (SELECT QuestionID FROM Questions WHERE QuestionText = ?) 
                                                    ORDER BY AnswerOptionID";

                            using (OleDbCommand cmdGetId = new OleDbCommand(getAnswerOptionIdSql, connection))
                            {
                                cmdGetId.Parameters.AddWithValue("?", questions[i].Text);

                                using (OleDbDataReader reader = cmdGetId.ExecuteReader())
                                {
                                    int count = 0;
                                    int answerOptionId = -1;
                                    while (reader.Read())
                                    {
                                        if (count == answerIndex)
                                        {
                                            answerOptionId = reader.GetInt32(0);
                                            break;
                                        }
                                        count++;
                                    }

                                    if (answerOptionId != -1)
                                    {
                                        string updateVoteSql = "UPDATE AnswerOptions SET VoteCount = VoteCount + 1 WHERE AnswerOptionID = ?";
                                        using (OleDbCommand cmdUpdate = new OleDbCommand(updateVoteSql, connection))
                                        {
                                            cmdUpdate.Parameters.AddWithValue("?", answerOptionId);
                                            cmdUpdate.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                MessageBox.Show("Спасибо за прохождение опроса!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении голосов: " + ex.Message);
            }
        }
    }
}
