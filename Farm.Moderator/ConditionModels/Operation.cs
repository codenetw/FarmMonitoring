using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farm.Moderator.ConditionModels
{
    [Serializable]
    public enum LocicalBinaryOperator
    {
        or,
        and
    }

    [Serializable]
    public enum LocicalUnaryOperator
    {
        True,
        Not
    }
    
    [Serializable]
    public enum CompareBinaryOperator
    {
        Equal,
        NotEqual,
        Less,
        More
    }

    [Serializable]
    public enum CollectionOperator
    {
        AllEqual,
        Contains
    }
}
