using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExaminationSystem.Models
{
    public class QuestionModel
    {
        public int TotalQuestionInSet { get; set; }
        public int QuestionNumber { get; set; }
        public int TestId { get; set; }
        public String TestName { get; set; }

        public String Question { get; set; }
        public String QuestionType { get; set; }

        public int Point { get; set; }
        public List<QXOModel> options { get; set; }

    }
        public class QXOModel  //Questions X Options Model
        {
            public int ChoiceId { get; set; }
            public String Label { get; set; }
            public String Answer { get; set; }
        }

    
}