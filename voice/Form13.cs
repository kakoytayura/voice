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
    public class VoteResult
    {
        public List<int> SelectedAnswersIndexes { get; set; } = new List<int>();
    }

    public partial class Form13 : Form
    {

        public class Question
        {
            public int QuestionID { get; set; }
            public string Text { get; set; }
            public int QuestionType { get; set; }
            public List<AnswerOption> Answers { get; set; } = new List<AnswerOption>();
        }

        public class AnswerOption
        {
            public int AnswerOptionID { get; set; }
            public string Text { get; set; }
        }

        private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=DbVoice.accdb;";
        private List<Question> questions;
        private List<List<RadioButton>> radioButtonsList = new List<List<RadioButton>>();
        public VoteResult Result { get; private set; } = new VoteResult();

        private TabControl tabControl;
        private Button btnVote;
        private Button btnAnonymousVote;
        private bool hasVoted = false;
        private string userName = "frogg123";

        public Form13(List<voice.Question> surveyQuestions)
        {
            InitializeComponent();

            this.Width = 400;
            this.Height = 650;
            this.Text = "Голосование";

            LoadQuestionsFromDatabase();
            InitializeVotingUI();
        }

        public Form13()
        {
            InitializeComponent();

            this.Width = 400;
            this.Height = 650;
            this.Text = "Голосование";

            LoadQuestionsFromDatabase();
            InitializeVotingUI();
        }

        private void LoadQuestionsFromDatabase()
        {
            questions = new List<Question>();

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();

                string queryQuestions = "SELECT QuestionID, QuestionText, QuestionType FROM Questions WHERE QuestionType = ?";
                OleDbCommand cmdQ = new OleDbCommand(queryQuestions, connection);
                cmdQ.Parameters.Add("?", OleDbType.Integer).Value = 2;

                using (OleDbDataReader readerQ = cmdQ.ExecuteReader())
                {
                    while (readerQ.Read())
                    {
                        var question = new Question
                        {
                            QuestionID = Convert.ToInt32(readerQ.GetValue(0)),
                            Text = readerQ.GetValue(1)?.ToString() ?? string.Empty,
                            QuestionType = Convert.ToInt32(readerQ.GetValue(2)),
                            Answers = new List<AnswerOption>()
                        };

                        string queryAnswers = "SELECT AnswerOptionID, AnswerText FROM AnswerOptions WHERE QuestionID = ? ORDER BY AnswerOptionID";
                        OleDbCommand cmdA = new OleDbCommand(queryAnswers, connection);
                        cmdA.Parameters.Add("?", OleDbType.Integer).Value = question.QuestionID;

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

        private void InitializeVotingUI()
        {
            tabControl = new TabControl()
            {
                Location = new Point(10, 10),
                Size = new Size(360, 550),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            this.Controls.Add(tabControl);

            if (questions == null || questions.Count == 0)
            {
                MessageBox.Show("Вопросы не загружены");
                return;
            }

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

                List<RadioButton> radioButtons = new List<RadioButton>();

                for (int j = 0; j < question.Answers.Count; j++)
                {
                    RadioButton rb = new RadioButton()
                    {
                        Text = question.Answers[j].Text,
                        Location = new Point(20, y),
                        AutoSize = true
                    };
                    tabPage.Controls.Add(rb);
                    radioButtons.Add(rb);
                    y += 25;
                }

                radioButtonsList.Add(radioButtons);
                tabControl.TabPages.Add(tabPage);
            }

            btnVote = new Button()
            {
                Text = "Голосовать",
                Location = new Point(10, 570),
                Width = 100
            };
            btnVote.Click += BtnVote_Click;
            this.Controls.Add(btnVote);

            btnAnonymousVote = new Button()
            {
                Text = "Анонимное голосование",
                Location = new Point(120, 570),
                Width = 150
            };
            btnAnonymousVote.Click += BtnAnonymousVote_Click;
            this.Controls.Add(btnAnonymousVote);
        }

        private void BtnVote_Click(object sender, EventArgs e)
        {
            ProcessVote(userName);
        }

        private void BtnAnonymousVote_Click(object sender, EventArgs e)
        {
            ProcessVote(null); 
        }

        private void ProcessVote(string username)
        {
            if (hasVoted)
            {
                MessageBox.Show("Вы уже проголосовали.");
                return;
            }

            Result.SelectedAnswersIndexes.Clear();

            for (int i = 0; i < radioButtonsList.Count; i++)
            {
                var radios = radioButtonsList[i];
                int selectedIndex = -1;

                for (int j = 0; j < radios.Count; j++)
                {
                    if (radios[j].Checked)
                    {
                        selectedIndex = j;
                        break;
                    }
                }

                if (selectedIndex == -1)
                {
                    MessageBox.Show($"Пожалуйста, выберите ответ на вопрос #{i + 1}");
                    return;
                }

                Result.SelectedAnswersIndexes.Add(selectedIndex);
            }

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    for (int i = 0; i < questions.Count; i++)
                    {
                        int selectedAnswerIndex = Result.SelectedAnswersIndexes[i];

                        string getAnswerOptionIdSql = @"SELECT AnswerOptionID FROM AnswerOptions 
                                                WHERE QuestionID = (SELECT QuestionID FROM Questions WHERE QuestionText = ?) 
                                                ORDER BY AnswerOptionID";
                        int answerOptionId = -1;

                        using (OleDbCommand cmdGetId = new OleDbCommand(getAnswerOptionIdSql, connection))
                        {
                            cmdGetId.Parameters.AddWithValue("?", questions[i].Text);

                            using (OleDbDataReader reader = cmdGetId.ExecuteReader())
                            {
                                int count = 0;
                                while (reader.Read())
                                {
                                    if (count == selectedAnswerIndex)
                                    {
                                        answerOptionId = reader.GetInt32(0);
                                        break;
                                    }
                                    count++;
                                }
                            }
                        }

                        if (answerOptionId != -1)
                        {
                            string insertSql = "INSERT INTO Votes (Username, QuestionID, AnswerOptionID) VALUES (?, ?, ?)";
                            using (OleDbCommand cmd = new OleDbCommand(insertSql, connection))
                            {
                                object userParam = (object)username ?? DBNull.Value;

                                cmd.Parameters.AddWithValue("?", userParam);
                                cmd.Parameters.AddWithValue("?", questions[i].QuestionID); 
                                cmd.Parameters.AddWithValue("?", answerOptionId);          

                                cmd.ExecuteNonQuery();
                            }

                            string updateVoteCountSql = "UPDATE AnswerOptions SET VoteCount = VoteCount + 1 WHERE AnswerOptionID = ?";
                            using (OleDbCommand cmdUpdate = new OleDbCommand(updateVoteCountSql, connection))
                            {
                                cmdUpdate.Parameters.AddWithValue("?", answerOptionId);
                                cmdUpdate.ExecuteNonQuery();
                            }
                        }
                    }
                }

                btnVote.Text = "Голос принят";
                btnVote.Enabled = false;
                btnAnonymousVote.Enabled = false;
                hasVoted = true;

                foreach (var radios in radioButtonsList)
                    foreach (var rb in radios)
                        rb.Enabled = false;

                MessageBox.Show("Спасибо за ваш голос!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении голоса: " + ex.Message);
            }
        }
    }
}




