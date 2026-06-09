using DynamicData;
using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridReportForm
{
    public class SocketService
    {
        private SourceList<IWebSocketConnection> _sockets { get; set; } = new SourceList<IWebSocketConnection>();

        public IObservable<IChangeSet<IWebSocketConnection>> Connect() => _sockets.Connect();
    }
}
