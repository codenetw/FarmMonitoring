using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Farm.BaseController.CommunicationMessage;
using Farm.Moderator.BaseModel;
using Farm.Moderator.ConditionModels;

namespace Farm.Moderator
{
    public class FarmVisitor : IFarmVisitor<Result>
    {
        public AggregatorState _systemState { get; set; }

        public Result Validate(IBaseConditionModel model, AggregatorState systemState)
        {
            _systemState = systemState;
            return Visit(model);
        }

        public Result Visit(IBaseConditionModel model)
        {
            var result = new Result();
            var visit = model.Visit(this);
            result.IsValid &= visit.IsValid;
            CollectAdditionInfo(result, visit);
            return result;
        }

        public Result Visit(BianryOperation binary)
        {
            var leftResult = binary.LeftOperand.Visit(this);
            var rightResult = binary.RightOperand.Visit(this);
            var result = new Result();

            switch (binary.Operation)
            {
                case LocicalBinaryOperator.and:
                    result.IsValid = leftResult.IsValid && rightResult.IsValid;
                    CollectAdditionInfo(result, leftResult, rightResult);
                    break;
                case LocicalBinaryOperator.or:
                    result.IsValid = leftResult.IsValid || rightResult.IsValid;
                    CollectAdditionInfo(result, leftResult, rightResult);
                    break;                
                default:
                    break;
            }
            return result;
        }

        public Result Visit(UnaryOperation unary)
        {
            var operandResult = unary.Operand.Visit(this);
            var result = new Result();

            switch (unary.Operator)
            {
                case LocicalUnaryOperator.True:
                    result.IsValid = operandResult.IsValid;
                    CollectAdditionInfo(result, operandResult);
                    break;
                case LocicalUnaryOperator.Not:
                    result.IsValid = !operandResult.IsValid;
                    CollectAdditionInfo(result, operandResult);
                    break;
                default:
                    break;
            }
            return result;
        }

        public Result Visit(CompareBinaryOperation binary)
        {
            var result = new Result();

            var leftParam = binary.LeftOperand.Visit(this).Value;
            var rightParam = binary.RightOperand.Visit(this).Value;
         
            switch (binary.Operation)
            {
                case CompareBinaryOperator.Equal:
                    result.IsValid &= Check(leftParam, rightParam, result, 0);
                    break;
                case CompareBinaryOperator.NotEqual:
                    result.IsValid &= !Check(leftParam, rightParam, result, 0);
                    break;
                case CompareBinaryOperator.Less:
                    result.IsValid &= Check(leftParam, rightParam, result, - 1);
                    break;
                case CompareBinaryOperator.More:
                    result.IsValid &= Check(leftParam, rightParam, result, 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return result;
        }

        public Result Visit(PropertyCollectionOperation propertyCollection)
        {
            var result = new Result();
            var propCollection = propertyCollection.Collection.Select(x=>x.Visit(this));
            var propValue = propertyCollection.Property.Visit(this).Value;
            if (propValue == null)
            {
                result.IsValid = false;
                return result;
            }

            switch (propertyCollection.Operator)
            {
                case CollectionOperator.AllEqual:
                    var resultAll = propCollection.Select(x => x.Value).All(x => Check(x, propValue, result,0));
                    result.Value = resultAll;
                    result.IsValid = resultAll;
                    break;
                case CollectionOperator.Contains:
                    var resultAny = propCollection
                        .Select(x => x.Value).Any(x => Check(x, propValue, result, 0));
                    result.Value = resultAny;
                    result.IsValid = resultAny;
                    break;
            }

            return result;
        }

        public Result Visit(CardCheck card)
        {
            var result = new Result();
            foreach (var conditionProperty in card.PropertiesCondition)
            {
                result.IsValid &= conditionProperty.Visit(this).IsValid;
            }

            return result;
        }

        public Result Visit(ProcessCheck processCheck)
        {
            var result = new Result();
            foreach (var conditionProperty in processCheck.PropertiesCondition)
            {
                result.IsValid &= conditionProperty.Visit(this).IsValid;
            }

            return result;
        }

        public Result Visit(EthernetCheck ethernetCheck)
        {
            var result = new Result();
            foreach (var conditionProperty in ethernetCheck.PropertiesCondition)
            {
                result.IsValid &= conditionProperty.Visit(this).IsValid;
            }

            return result;
        }

        public Result Visit(MinerCheck minerCheck)
        {
            var result = new Result();
            foreach (var conditionProperty in minerCheck.PropertiesCondition)
            {
                result.IsValid &= conditionProperty.Visit(this).IsValid;
            }

            return result;
        }

        public Result Visit(BaseProperty property)
        {
            return new Result {Value = property.Value};
        }

        public Result Visit(ContextProperty property)
        {
            return new Result { Value = property.DynamicValue(_systemState) };
        }

        private void CollectAdditionInfo(Result In, params Result[] collectionsResults)
        {
            foreach (var collect in collectionsResults) In.AdditionInfo.AddRange(collect.AdditionInfo);
        }
        private bool Check(object a, object b,Result result, int num)
        {
            try
            {
                if (a == null || b == null)
                    return false;

                return CustomComparable.CustomCompareTo((IComparable)a,(IComparable) b) == num;
            }
            catch
            {
                CollectAdditionInfo(result, new Result { AdditionInfo = new List<string>() { "Ошибка сверки типов" } });
            }

            return false;
        }
    }
}
