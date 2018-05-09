using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farm.BaseController.CustomError
{
    public class FarmException : Exception
    {public ErrorCode Code { get; set; }
        public FarmException(Exception e, ErrorCode code)
            :base(e.Message, e)
        {
            Code = code;
        }
    }
}
