using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farm.Moderator
{
    public class CustomComparable
    {
        //todo удалить этот шлак
        public static int CustomCompareTo(object obj1, object obj2)
        {
            var t1 = obj1.GetType();
            var t2 = obj2.GetType();
            if (t1.FullName == t2.FullName)
                return ((IComparable) obj1).CompareTo(obj2);
            if (IsNumber(obj1) && IsNumber(obj2)) 
                return ((IComparable)float.Parse(obj1.ToString())).CompareTo(float.Parse(obj2.ToString()));
            return ((IComparable)obj1.ToString()).CompareTo(obj2.ToString());
        }
        public static bool IsNumber(object value)
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }
    }
}
