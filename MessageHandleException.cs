using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridReportForm
{
    public class MessageHandleException : Exception
    {
        public MessageHandleException(string message) : base(message) { }

        public MessageHandleException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
