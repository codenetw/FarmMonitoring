using Farm.Moderator.BaseModel;
using Farm.Moderator.ConditionModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Farm.BaseController.CommunicationMessage;

namespace Farm.Moderator
{
    public interface IFarmVisitor<out T>
    {
        T Validate(IBaseConditionModel model, AggregatorState systemState);
        T Visit(BianryOperation binary);
        T Visit(CompareBinaryOperation binary);
        T Visit(PropertyCollectionOperation propertyCollection);
        T Visit(UnaryOperation unary);
        T Visit(CardCheck card);
        T Visit(ProcessCheck processCheck);
        T Visit(EthernetCheck ethernetCheck);
        T Visit(MinerCheck minerCheck);
        T Visit(BaseProperty property);
        T Visit(ContextProperty property);
    }
}
