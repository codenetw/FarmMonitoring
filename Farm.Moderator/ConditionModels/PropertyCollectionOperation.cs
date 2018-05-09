using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farm.Moderator.ConditionModels
{
    [Serializable]
    public class PropertyCollectionOperation : IPropertyConditionModel
    {
        public IEnumerable<IPropertyConditionModel> Collection { get; set; }
        public CollectionOperator Operator { get; set; }
        public IPropertyConditionModel Property { get; set; }
        public T Visit<T>(IFarmVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
