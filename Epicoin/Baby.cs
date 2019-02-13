using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Epicoin
{
    class Baby
    {
        private Parent self;

        private List<Friend> friends;
        private List<PotentialParent> shoulders;

        private int RSAPrivateKey;
        private int RSAPublicKey;

        public Baby(Parent parent)
        {
            this.self = parent;
            parent.self = this;
            //Try to connect to parent
            //Try to connect to friends
            //If has no parents and no friends, cry
        }

        //first-connection related methods
        private void Call(Friend friend) { }
        private void Cry() { }

        //data downloading methods
        private void Leech(/*data to be leeched*/) { }

        //extending current friendlist
        private void Befriend(int KBRaddress) { }

    }
    struct Friend
    {
        public string IPAddress { get; set; }
        public int KBRAddress { get; set; }

        public Friend(string IP, int KBR) {
            this.IPAddress = IP;
            this.KBRAddress = KBR;
        }
    }

    struct PotentialParent
    {
        public IPEndPoint endPoint { get; }
        public DateTime cryingDate { get; }
        public Socket server { get; }

        public PotentialParent(string hostName, int port)
        {
            this.endPoint = new IPEndPoint(IPAddress.Parse(hostName), port); //check if valid code
            this.cryingDate = DateTime.UtcNow;
            this.server =new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public int SendTo(string msg)
        {
            byte[] data = Encoding.ASCII.GetBytes(msg);
            return this.server.SendTo(data, data.Length, SocketFlags.None, this.endPoint);
        }
    }
}
