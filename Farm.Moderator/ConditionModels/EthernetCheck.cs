using Farm.Moderator.ConditionModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farm.Moderator.BaseModel
{
    [Serializable]
    public class EthernetCheck : IBaseConditionModel
    {
        public List<CompareBinaryOperation> PropertiesCondition { get; set; }
        public T Visit<T>(IFarmVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
