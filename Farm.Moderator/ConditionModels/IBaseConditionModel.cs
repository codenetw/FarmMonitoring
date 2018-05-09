using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farm.Moderator.ConditionModels
{
 

    public interface IBaseConditionModel
    {
        T Visit<T>(IFarmVisitor<T> visitor);
    }

    public interface IPropertyConditionModel : IBaseConditionModel
    { }

    public interface ICollectionConditionModel : IBaseConditionModel
    { }
}
