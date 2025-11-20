using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace voice
{
    public class Question
    {
        public string Text { get; set; }
        public List<string> Answers { get; set; } = new List<string>();
    }

    public class SurveyResult
    {
        public List<int> SelectedAnswersIndexes { get; set; } = new List<int>();
    }
}
