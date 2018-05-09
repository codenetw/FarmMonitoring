using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Farm.BaseController.CommunicationMessage;

namespace Farm.Moderator.ConditionModels
{
    [Serializable]
    public class CompareBinaryOperation : IBaseConditionModel
    {
        public IPropertyConditionModel LeftOperand { get; set; }
        public CompareBinaryOperator Operation { get; set; }
        public IPropertyConditionModel RightOperand { get; set; }

        public T Visit<T>(IFarmVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
