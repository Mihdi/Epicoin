using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.Security.Cryptography;


namespace Epicoin
{
    class Baby : NetworkActor
    {
        private static int nbSecurityBytes = 3; //arbitrary number, the bigger the better but the longer to compute. So I put a small number for the tests
        private static string knownParentsFile = "ressources/baby_known_parents.prnt";
        private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
        
        private Parent self;

        private HashSet<Friend> friends;
        private Queue<PotentialParent> shoulders;

        private int[] RSAPrivateKey;
        private int RSAPublicKey;

        public Baby(Parent parent)
        {
            //fields init routines
            this.self = parent;
            parent.self = this;
            this.friends = new HashSet<Friend>();
            this.shoulders = new Queue<PotentialParent>();

            //generate RSAPrivate and Public Key
            RSA.GenerateRSAKeys(out this.RSAPrivateKey, out this.RSAPublicKey);
            
            //making list of friends
            using(StreamReader sr = new StreamReader(this.knownParentsFile)){
                string ip;
                while((ip = sr.ReadLine()) != null){
                    int kbr;
                    if(Int32.TryParse(sr.ReadLine(), out kbr)){
                        this.friends.Add(new Friend(ip, kbr));
                    }
                    else
                    {
                        throw new Exception("init Baby failed: '"+this.knownParentsFile+"' is not formatted properly");
                    }
                }
            }

            //Try to connect to friends
            foreach(Friend f in this.friends){
                Call(f);
            }

            //If has no parents and no friends, cry
        }

        //first-connection related methods
        private void Call(Friend friend) {

            ClientWebSocket newFriend = friend.answerCall();

            if(newFriend != null)
            {
                //Ohana means family. Family means nobody gets left behind or forgotten
                this.self.family.Add(newFriend);
            }
            else
            {
                //if friend can't be reached, put it off the this.friends. Sad.
                this.friends.Remove(friend);
            }
        }

        private void Cry() { 
             throw new NotImplementedException();
        }

        //data downloading methods
        private void Leech(/*data to be leeched*/) { 
             throw new NotImplementedException();
        }

        //extending current friendlist
        private void Befriend(int KBRaddress) { 
             throw new NotImplementedException();
        }

    }
    static class RSA{
            //RSA keys' generation related method
            private static bool isPrime(int n){
                if(n < 2)
                    return false;
                if(n == 2)
                    return true;

                for(int i = 2; i <= Math.Sqrt(n)+1; i++){
                    if(n%i == 0)
                        return false;
                }
                return true;
            }
            private static int gcd(int n, int m){
                if(n == 0 || m == 0)
                    return 0;
                
                if(n == m)
                    return a;
                
                if(n > m){
                    return gcd(n-m,m);
                }
                else
                {
                    return gcd(n, m-n);
                }
            }
            private static bool areCoprime(int n, int m){
                return gcd(n,m) == 1;
            }
            private static int sign(int n){
                if(n > 0)
                    return 1;
                
                if(n == 0)
                    return 0;
                
                return (-1);
            }
            private static int quot(int n, int m){
                if(sign(m) == 1)
                    return n/m;
                
                return n/m-sign(m);
            }

            private static int modulo(int n, int m){
                //assuming m is positive, but that should be enough for RSA
                if(sign(n) == 1)
                    return n%m;

                return (n%m)+((sign(m) == 1)? (m) : (-m));
            }

            private static int[] bezout(int n, int m){
                if(m == 0){
                    return new int[]{1,0,n};
                }
                else
                {
                    int[] foo = bezout(m, (modulo(n,m)));
                    return new int[]{foo[1], (foo[0]-foo[1]*(quot(n,m))), foo[2]};
                }
            }
            
            private static int modMultInv(int a, int n){
                int[] foo = bezout(a,n);
                return modulo(foo[0], n);
            }
            /* 
                AtkinSieve is the quickest known way to compute prime numbers I know of, 
                so it could be nice to implement it as a bonus.
                However, I'm going for Erathostenes right now because it's quicker to implement
            */
            private static int[] AtkinSieve(int limit){
                throw NotImplementedException();
            }
            private static List<int> EratosthenesSieve(int n){
                if(n < 2)
                    throw new Exception("Eratosthenes: invalid input");

                //making a list of all prime candidates
                List<int> output = new List<int>(){2};
                for(int i = 3; i < n; i = i+2){
                    output.Add(i);
                }

                //getting rid of the candidates that aren't actually prime
                for(int i = output[0]; i < output.Count; i++){
                    for(int j = output[i+1]; j < output.Length-1; j++){
                        if(output[j] % i == 0){
                            output.Remove(output[j]);
                        }
                    }
                }

                return output;
            }
            private static int generateRandomNumber(){
                int output = 0;

                Byte[] rndByte = new byte[this.nbSecurityBytes];
                rngCsp.GetBytes(rndByte);
                
                for(int i = 0; i < this.nbSecurityBytes; i++){
                    output += b*Math.Pow(8,i);
                }

                return output;
            }
            private static int generateRandomNumber(int min, int max){
                max--;
                int output = 0;

                Byte[] rndByte = new byte[Math.Log(max,8)];
                rngCsp.GetBytes(rndByte);
                
                for(int i = 0; i < Math.Log(max,8); i++){
                    output += b*Math.Pow(8,i);
                }

                return (output-max+min);
            }
            private static void GenerateRSAKeys(out int privateKey, out int[] publicKey)
            {
                List<int> primes = EratosthenesSieve(generateRandomNumber());

                int p = primes[primes.Count-1];

                primes.RemoveAt(primes.Count-1);

                int q = primes[generateRandomNumber(primes.Count/2, primes.Length)];

                int n = p*q;

                int eulerIndice = (p-1)*(q-1);
                
                int e;
                do{
                    e = generateRandomNumber(0, eulerIndice);
                }while(!(areCoprime(e, eulerIndice)));

                int d = modMultInv(e, eulerIndice);

                privateKey = d;
                publicKey = new int[]{n,e}; 
            }
        }
    struct Friend
    {
        public string IPAddress { get; set; }
        public int KBRAddress { get; set; }

        public Friend(string IP, int KBR) {
            this.IPAddress = IP;
            this.KBRAddress = KBR;
        }

        public async ClientWebSocket answerCall(){

            byte[] buffer = new byte[64]; //this may be useful if we decide to read the returned message
        
            ClientWebSocket output = new ClientWebSocket();

            await output.ConnectAsync(this.IPAddress, NetworkActor.cts.Token);;

            while(output.State == WebSocketState.Open){
                Task result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), NetworkActor.cts.Token);
                if(result.MessageType == WebSocketMessageType.Close) //fun fact about c# websockets: there is no succesful connection state, so you can deduce it from a failed one
                { 
                    await output.CloseAsync(WebSocketCloseStatus.NormalClosure, "", NetworkActor.cts.Token);
                    return null;
                }
                else
                {
                    return output;
                }
            }
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
