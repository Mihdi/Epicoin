using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;


namespace Epicoin
{
	class Baby : NetworkActor
	{
		public static int nbSecurityBytes = 3; //arbitrary number, the bigger the better but the longer to compute. So I put a small number for the tests
		private static string knownParentsFile = "ressources/baby_known_parents.prnt";
		public static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

		private static int MaxNumberOfShoulders = 5; //small number to start with
		private static int ShoulderMaxWaitTime = 5; //in seconds
		private readonly string cryingRequest;

		private static int IPFormat = 4; //IPv6 is better but the odds of finding people using IPv4 are greaters

		private readonly Parent self;

		private readonly HashSet<Friend> friends;
		private readonly Queue<PotentialParent> shoulders;

		private readonly int RSAPrivateKey;
		private readonly int[] RSAPublicKey;

		public Baby(Parent parent)
		{
			//fields init routines
			this.self = parent;
			parent.Self = this;
			this.friends = new HashSet<Friend>();
			this.shoulders = new Queue<PotentialParent>();

			//generate RSAPrivate and Public Key
			RSA.GenerateRSAKeys(out this.RSAPrivateKey, out this.RSAPublicKey);

			//making list of friends
			using (StreamReader sr = new StreamReader(Baby.knownParentsFile))
			{
				string ip;
				while ((ip = sr.ReadLine()) != null)
				{
					if (Int32.TryParse(sr.ReadLine(), out int kbr))
					{
						this.friends.Add(new Friend(ip, kbr));
					}
					else
					{
						throw new Exception("init Baby failed: '" + Baby.knownParentsFile + "' is not formatted properly");
					}
				}
			}

			//Try to connect to friends
			foreach (Friend f in this.friends)
			{
				Call(f);
			}

			//Generate the crying request
			this.cryingRequest = null;

			//If has no parents and no friends, cry
			if(friends.Count == 0)
			{
				Cry();
			}
		}

		//first-connection related methods
		private async void Call(Friend friend)
		{

			Task<ClientWebSocket> tryingToMakeAFriend = friend.AnswerCall();
			ClientWebSocket friendlySocket = await tryingToMakeAFriend;
			if (friendlySocket != null)
			{
				//Ohana means family. Family means nobody gets left behind or forgotten
				this.self.Family.Add(friend, friendlySocket);
			}
			else
			{
				//if friend can't be reached, put it off the this.friends. Sad.
				this.friends.Remove(friend);
			}
		}
		private static string GenerateRandomIp()
		{
			string output = "";

			Byte[] rndByte = new byte[Baby.IPFormat];
			Baby.rngCsp.GetBytes(rndByte);

			
			foreach(byte b in rndByte)
			{
				output += b.ToString();
			}

			return output;
		}

		private void Cry()
		{
			//“We need never be ashamed of our tears.” - Dickens
			if(this.shoulders.Count == Baby.MaxNumberOfShoulders)
			{
				PotentialParent firstShoulder = this.shoulders.Peek();

				if (((int)(DateTime.UtcNow - firstShoulder.CryingDate).TotalSeconds) > Baby.ShoulderMaxWaitTime){
					this.shoulders.Dequeue();
				}

				Cry();
			}
			else
			{
				string hostIp;
				PotentialParent daddy;

				do
				{

					hostIp = Baby.GenerateRandomIp();
					daddy = new PotentialParent(hostIp, NetworkActor.Port);

				} while (this.shoulders.Contains(daddy));

				daddy.SendTo(this.cryingRequest);
			}
			
		}
		
		//data downloading methods
		private void Leech(/*data to be leeched*/)
		{
			throw new NotImplementedException();
		}

		//extending current friendlist
		private void Befriend(int KBRaddress)
		{
			throw new NotImplementedException();
		}

	}
	static class RSA
	{
		//RSA keys' generation related method
		private static bool IsPrime(int n)
		{
			if (n < 2)
				return false;
			if (n == 2)
				return true;

			for (int i = 2; i <= Math.Sqrt(n) + 1; i++)
			{
				if (n % i == 0)
					return false;
			}
			return true;
		}

		private static int GCD(int n, int m)
		{
			if (n == 0 || m == 0)
				return 0;

			if (n == m)
				return n;

			if (n > m)
			{
				return GCD(n - m, m);
			}
			else
			{
				return GCD(n, m - n);
			}
		}

		private static bool AreCoprime(int n, int m)
		{
			return GCD(n, m) == 1;
		}

		private static int Sign(int n)
		{
			if (n > 0)
				return 1;

			if (n == 0)
				return 0;

			return (-1);
		}

		private static int Quot(int n, int m)
		{
			if (Sign(m) == 1)
				return n / m;

			return n / m - Sign(m);
		}

		private static int Modulo(int n, int m)
		{
			//assuming m is positive, but that should be enough for RSA
			if (Sign(n) == 1)
				return n % m;

			return (n % m) + ((Sign(m) == 1) ? (m) : (-m));
		}

		private static int[] Bezout(int n, int m)
		{
			if (m == 0)
			{
				return new int[] { 1, 0, n };
			}
			else
			{
				int[] foo = Bezout(m, (Modulo(n, m)));
				return new int[] { foo[1], (foo[0] - foo[1] * (Quot(n, m))), foo[2] };
			}
		}

		private static int ModMultInv(int a, int n)
		{
			int[] foo = Bezout(a, n);
			return Modulo(foo[0], n);
		}

		/* 
			AtkinSieve is the quickest known way to compute prime numbers I know of, 
			so it could be nice to implement it as a bonus.
			However, I'm going for Erathostenes right now because it's quicker to implement
		*/
		private static int[] AtkinSieve(int limit)
		{
			throw new NotImplementedException();
		}

		private static List<int> EratosthenesSieve(int n)
		{
			if (n < 2)
				throw new Exception("Eratosthenes: invalid input");

			//making a list of all prime candidates
			List<int> output = new List<int>() { 2 };
			for (int i = 3; i < n; i = i + 2)
			{
				output.Add(i);
			}

			//getting rid of the candidates that aren't actually prime
			for (int i = output[0]; i < output.Count; i++)
			{
				for (int j = output[i + 1]; j < output.Count - 1; j++)
				{
					if (output[j] % i == 0)
					{
						output.Remove(output[j]);
					}
				}
			}

			return output;
		}

		private static int GenerateRandomNumber()
		{
			int output = 0;

			Byte[] rndByte = new byte[Baby.nbSecurityBytes];
			Baby.rngCsp.GetBytes(rndByte);

			for (int i = 0; i < rndByte.Length; i++)
			{
				output += (int)(rndByte[i] * Math.Pow(8, i));
			}

			return output;
		}

		private static int GenerateRandomNumber(int min, int max)
		{
			max--;
			int output = 0;

			Byte[] rndByte = new byte[(int)(Math.Log(max, 8))];
			Baby.rngCsp.GetBytes(rndByte);

			for (int i = 0; i < Math.Log(max, 8); i++)
			{
				output += (int)(rndByte[i] * Math.Pow(8, i));
			}

			return (output - max + min);
		}

		public static void GenerateRSAKeys(out int privateKey, out int[] publicKey)
		{
			List<int> primes = EratosthenesSieve(GenerateRandomNumber());

			int p = primes[primes.Count - 1];

			primes.RemoveAt(primes.Count - 1);

			int q = primes[GenerateRandomNumber(primes.Count / 2, primes.Count)];

			int n = p * q;

			int eulerIndice = (p - 1) * (q - 1);

			int e;
			do
			{
				e = GenerateRandomNumber(0, eulerIndice);
			} while (!(AreCoprime(e, eulerIndice)));

			int d = ModMultInv(e, eulerIndice);

			privateKey = d;
			publicKey = new int[] { n, e };
		}
	}
	class Friend
	{
		public string IPAddress { get; set; }
		public int KBRAddress { get; set; }

		public Friend(string IP, int KBR)
		{
			this.IPAddress = IP;
			this.KBRAddress = KBR;
		}


		public async Task<ClientWebSocket> AnswerCall()
		{

			byte[] buffer = new byte[64]; //this may be useful if we decide to read the returned message in a further version

			ClientWebSocket output = new ClientWebSocket();

			await output.ConnectAsync(new Uri("http://" + this.IPAddress + "/"), NetworkActor.cts.Token);

			while (output.State == WebSocketState.Open)
			{
				WebSocketReceiveResult result = await output.ReceiveAsync(new ArraySegment<byte>(buffer), NetworkActor.cts.Token);
				if (result.MessageType == WebSocketMessageType.Close) //fun fact about c# websockets: there is no succesful connection state, so you can deduce it from a failed one
				{
					await output.CloseAsync(WebSocketCloseStatus.NormalClosure, "", NetworkActor.cts.Token);
					return null;
				}
				else
				{
					return output;
				}
			}
			throw new Exception("AnswerCall: This exception should've never been triggered. Something went seriously wrong.");
		}
	}

	struct PotentialParent
	{
		public IPEndPoint EndPoint { get; }
		public DateTime CryingDate { get; }
		public Socket Server { get; }

		public PotentialParent(string hostName, int port)
		{
			this.EndPoint = new IPEndPoint(IPAddress.Parse(hostName), port); //check if valid code
			this.CryingDate = DateTime.UtcNow;
			this.Server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		}

		public int SendTo(string msg)
		{
			byte[] data = Encoding.ASCII.GetBytes(msg);
			return this.Server.SendTo(data, data.Length, SocketFlags.None, this.EndPoint);
		}
	}
}
