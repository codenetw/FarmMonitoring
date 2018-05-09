using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farm.Moderator.ConditionModels
{
    [Serializable]
    public class BaseProperty : IPropertyConditionModel
    {
        public string Code { get; set; }
        public object Value { get; set; }

        public T Visit<T>(IFarmVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
