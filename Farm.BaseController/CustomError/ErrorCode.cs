using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farm.BaseController
{
    public enum ErrorCode
    {
        ConnectionError,
        SerializationError,
        ParsingError,
        ResourceNotAvailable
    }
}
