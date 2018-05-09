using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Farm.BaseController.CommunicationMessage;
using Farm.BaseController.CommunicationMessageModel;
using Farm.Moderator;
using Farm.Moderator.BaseModel;
using Farm.Moderator.ConditionModels;
using Farm.Moderator.Helper;
using Microsoft.SqlServer.Server;
using NUnit.Framework;

namespace IntegrationTest.ModeratorTest
{
    [TestFixture]
    public class ModeratorTests
    {

        private AggregatorState FakeState()
        {
            return new AggregatorState
            {
                Cards = new[]
                {
                    new CardParam(1, true, 1111, 50, 0, 0, 0, 0),
                    new CardParam(2, true, 1111, 50, 0, 0, 0, 0),
                    new CardParam(3, true, 1111, 50, 0, 0, 0, 0),
                    new CardParam(4, true, 1111, 0, 0, 0, 0, 0),
                    new CardParam(5, true, 1111, 50, 0, 0, 0, 0)

                },
                MinerApiInfo = new MinerStatistics
                {
                    TotalMainShare = 800
                } 
            };
        }
        private IBaseConditionModel GetModelWhenCardsAndMinerChecked(LocicalBinaryOperator logicalOperator)
        {
            //true or false = true
            return new BianryOperation
            {
                LeftOperand = new CardCheck
                {
                    PropertiesCondition = new List<CompareBinaryOperation>()
                    {
                       new CompareBinaryOperation
                       {
                           LeftOperand =  new ContextProperty
                           {
                               DynamicValue = (ctx) => ctx.Card(1, x => x.Voltage.Current)
                           },
                           Operation = CompareBinaryOperator.Equal,
                           RightOperand =  new BaseProperty
                           {
                               Code = "Voltage",
                               Value = 1111
                           }
                       },
                        new CompareBinaryOperation
                        {
                            LeftOperand =  new BaseProperty
                            {
                                Code = "Temperature",
                                Value = 50
                            },
                            Operation = CompareBinaryOperator.Less,
                            RightOperand =  new BaseProperty
                            {
                                Code = "Temperature",
                                Value = 70
                            }
                        }
                    }
                },
                Operation = logicalOperator,
                RightOperand = new MinerCheck
                {
                    PropertiesCondition = new List<CompareBinaryOperation>
                    {

                        new CompareBinaryOperation
                        {
                            LeftOperand =  new ContextProperty
                            {
                                DynamicValue = (ctx) => ctx.Miner(x => x.TotalMainShare)
                            },
                            Operation = CompareBinaryOperator.More,
                            RightOperand = new BaseProperty
                            {
                                Value = 1000
                            }
                        },
                    }
                }
            };
        }

        [Test]
        public void TestSerializationModerator()
        {
            var seria = BinarySerializer.SerializeToString(GetModelWhenCardsAndMinerChecked(LocicalBinaryOperator.or));
            var deSeria = BinarySerializer.DeserializeFromString<IBaseConditionModel>(seria);
            var moderator = new FarmVisitor();
            var result = moderator.Validate(deSeria, FakeState());
            Assert.AreEqual(result.IsValid, true);
        }

        [Test]
        public void TestValidationModerator()
        {
            var moderator = new FarmVisitor();
            var result = moderator.Validate(GetModelWhenCardsAndMinerChecked(LocicalBinaryOperator.or), FakeState());
            Assert.AreEqual(result.IsValid, true);
        }

        [Test]
        public void TestValidationModeratorAnd()
        {
            var moderator = new FarmVisitor();
            var result = moderator.Visit(GetModelWhenCardsAndMinerChecked(LocicalBinaryOperator.and));
            Assert.AreEqual(result.IsValid, false);
        }

        [Test]
        public void TestValidationCoolectionAllModerator()
        {
            var model = new UnaryOperation
            {
                Operand = new PropertyCollectionOperation
                {
                    Collection = new []
                    {
                        new ContextProperty { DynamicValue = (ctx) => ctx.Card(1, x => x.Voltage.Current)},
                        new ContextProperty { DynamicValue = (ctx) => ctx.Card(2, x => x.Voltage.Current)},
                        new ContextProperty { DynamicValue = (ctx) => ctx.Card(3, x => x.Voltage.Current)},
                        new ContextProperty { DynamicValue = (ctx) => ctx.Card(4, x => x.Voltage.Current)},
                        new ContextProperty { DynamicValue = (ctx) => ctx.Card(5, x => x.Voltage.Current)},
                    },
                    Operator = CollectionOperator.AllEqual,
                    Property = new BaseProperty
                    {
                        Value = 1111
                    }
                }
            };
            var moderator = new FarmVisitor();
            var result = moderator.Validate(model, FakeState());
            Assert.AreEqual(result.IsValid, true);
        }

        [Test]
        public void TestValidationCoolectioninCollectionModerator()
        {
            var model = new UnaryOperation()
            {
                Operand = new PropertyCollectionOperation
                {
                    Collection = new[]
                    {
                        new PropertyCollectionOperation
                        {
                            Collection = new[]
                            {
                                new ContextProperty { DynamicValue = (ctx) => ctx.Card(1, x => x.Voltage.Current)},
                                new ContextProperty { DynamicValue = (ctx) => ctx.Card(2, x => x.Voltage.Current)},
                                new ContextProperty { DynamicValue = (ctx) => ctx.Card(3, x => x.Voltage.Current)},
                                new ContextProperty { DynamicValue = (ctx) => ctx.Card(4, x => x.Voltage.Current)},
                                new ContextProperty { DynamicValue = (ctx) => ctx.Card(5, x => x.Voltage.Current)},
                            },
                            Operator = CollectionOperator.AllEqual,
                            Property = new BaseProperty
                            {
                                Value = 1111
                            }
                        },
                        new PropertyCollectionOperation
                        {
                            Collection = new[]
                            {
                                new ContextProperty { DynamicValue = (ctx) => ctx.Card(1, x => x.TempLimit.Current)},
                                new ContextProperty { DynamicValue = (ctx) => ctx.Card(2, x => x.TempLimit.Current)},
                                new ContextProperty { DynamicValue = (ctx) => ctx.Card(3, x => x.TempLimit.Current)},
                                new ContextProperty { DynamicValue = (ctx) => ctx.Card(4, x => x.TempLimit.Current)},
                                new ContextProperty { DynamicValue = (ctx) => ctx.Card(5, x => x.TempLimit.Current)},
                            },
                            Operator = CollectionOperator.Contains,
                            Property = new BaseProperty
                            {
                                Value = 50
                            }
                        }
                    },
                    Operator = CollectionOperator.AllEqual,
                    Property = new BaseProperty
                    {
                        Value = true
                    }
                }
            };
            var moderator = new FarmVisitor();
            var result = moderator.Validate(model, FakeState());
            Assert.AreEqual(result.IsValid, true);
        }

        [Test]
        public void TestValidationCoolectionAnyModerator()
        {
            var model = new UnaryOperation()
            {
                Operand = new PropertyCollectionOperation
                {
                    Collection = new[]
                    {
                        new ContextProperty { DynamicValue = (ctx) => ctx.Card(1, x => x.TempLimit.Current)},
                        new ContextProperty { DynamicValue = (ctx) => ctx.Card(2, x => x.TempLimit.Current)},
                        new ContextProperty { DynamicValue = (ctx) => ctx.Card(3, x => x.TempLimit.Current)},
                        new ContextProperty { DynamicValue = (ctx) => ctx.Card(4, x => x.TempLimit.Current)},
                        new ContextProperty { DynamicValue = (ctx) => ctx.Card(5, x => x.TempLimit.Current)},
                    },
                    Operator = CollectionOperator.Contains,
                    Property = new BaseProperty
                    {
                        Value = 0
                    }
                }
            };
            var moderator = new FarmVisitor();
            var result = moderator.Validate(model, FakeState());
            Assert.AreEqual(result.IsValid, true);
        }
    }
}
