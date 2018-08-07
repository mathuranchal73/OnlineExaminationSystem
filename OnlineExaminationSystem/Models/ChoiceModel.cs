using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExaminationSystem.Models
{
    public class ChoiceModel
    {
        public int ChoiceId { get; set; }
        public String IsChecked { get; set; }
    }
    
    public class AnswerModel
    {
        public int TestId { get; set; }
        public int QuestionId { get; set; }
        public Guid Token { get; set; }
        public List<ChoiceModel> UserChoices { get; set; }
        public String Answer { get; set; }
        public String Direction { get; set; }

        public List<int> UserSelectedId
        {
            get
            {
                return UserChoices == null ? new List<int>() : UserChoices.Where(x => x.IsChecked == "on" || "true".Equals(x.IsChecked, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.ChoiceId).ToList();
            }
        }
    }
}