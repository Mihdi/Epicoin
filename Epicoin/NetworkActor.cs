using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Epicoin
{
    public abstract class NetworkActor
    {
        protected static CancellationTokenSource cts = new CancellationTokenSource();
        protected static string OwnIp = Dns.GetHostByName(hostName).AddressList[0].ToString();  
        //usage of fixed port for now
        protected static int Port = 27945; //not in use according to https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers
        

        //static HuffmanCode();
        
        class BinTree<T>
        {
            public T InnerValue { get; set; }
            public BinTree<T> Right{ get; set; }
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
