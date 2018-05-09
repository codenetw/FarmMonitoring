using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farm.Moderator.ConditionModels
{
    [Serializable]
    public class UnaryOperation : IBaseConditionModel
    {
        public UnaryOperation()
        {
            Operator = LocicalUnaryOperator.True;
        }
        public LocicalUnaryOperator Operator { get; set; }

        public IBaseConditionModel Operand { get; set; }

        public T Visit<T>(IFarmVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
