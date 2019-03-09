using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace Epicoin
{
    /*
     * TODO: Write the event handler (on reception, add tear)
     * 
     */
    class Parent
    {
        public Baby self { get; set; }
        //private List<Baby> babies; Is this really useful? We already have them in the Baby class
        public List<ClientWebSocket> family{get; set;}
        private Socket childCareGiver;

        public Parent()
        {

            //init this.childCareGiver
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 9191); //Check that 9191 is not commonly used
            this.childCareGiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.childCareGiver.Bind(endPoint);
        }
        
    }
    struct Tear
    {
        public readonly string IPAddress;
        public readonly string publicKey;

        public Tear(string receivedTear)
        {
            //TODO: get IPAddress from the UDP packet received
            //TDOO: get the publicKey from the UDP packet received

        }
    }
}
