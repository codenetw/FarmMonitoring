using System;
using Farm.BaseController.CommunicationMessage;

namespace Farm.Moderator.ConditionModels
{
    [Serializable]
    public class ContextProperty : IPropertyConditionModel
    {
        public Func<AggregatorState, object> DynamicValue { get; set; }
        
        public T Visit<T>(IFarmVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}