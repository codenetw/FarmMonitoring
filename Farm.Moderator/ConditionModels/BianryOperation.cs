using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farm.Moderator.ConditionModels
{
    [Serializable]
    public class BianryOperation : IBaseConditionModel
    {
        public IBaseConditionModel LeftOperand { get; set; }
        public LocicalBinaryOperator Operation { get; set; }
        public IBaseConditionModel RightOperand { get; set; }
        public T Visit<T>(IFarmVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
