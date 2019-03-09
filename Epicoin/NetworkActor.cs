using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Epicoin
{
	public abstract class NetworkActor
	{
		public static CancellationTokenSource cts = new CancellationTokenSource();
		protected string OwnIp;

		//usage of fixed port for now
		protected static int Port = 27945; //not in use according to https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers

		public NetworkActor()
		{
			string localIP;
			using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
			{
				socket.Connect("8.8.8.8", 65530);
				IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
				localIP = endPoint.Address.ToString();
			}
		}

		//static HuffmanCode();

		class BinTree<T>
		{
			public T InnerValue { get; set; }
			public BinTree<T> Right { get; set; }
			public BinTree<T> Left { get; set; }

			public BinTree(T val)
			{
				this.InnerValue = val;
				this.Right = null;
				this.Left = null;
			}
		}
	}
}
