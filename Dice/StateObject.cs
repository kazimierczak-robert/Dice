using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

//https://msdn.microsoft.com/pl-pl/library/fx6588te(v=vs.110).aspx
// State object for reading client data asynchronously
namespace DiceClient
{
    class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
        public byte[] sessionKey = null;
    }
}
