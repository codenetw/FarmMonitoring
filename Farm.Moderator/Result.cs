using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farm.Moderator
{
    public class Result
    {
        public Result()
        {
            AdditionInfo = new List<string>(0);
            IsValid = true;
        }
        public bool IsValid { get; set; }
        public object Value { get; set; }
        public List<string> AdditionInfo { get; set; }
    }
}
