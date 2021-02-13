
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TournamentBackend;
using Newtonsoft.Json;

namespace BotServer
{
    class Server
    {
        // Main Method 
        public void Main()
        {
            ExecuteServer();
        }

        public void Log(string msg)
        {
            Console.WriteLine("SERVER THREAD: " + msg);
        }

        public void ExecuteServer()
        {
            // Establish the local endpoint  
            // for the socket. Dns.GetHostName 
            // returns the name of the host  
            // running the application. 
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

            // Creation TCP/IP Socket using  
            // Socket Class Costructor 
            Socket listener = new Socket(ipAddr.AddressFamily,
                         SocketType.Stream, ProtocolType.Tcp);

            try
            {

                // Using Bind() method we associate a 
                // network address to the Server Socket 
                // All client that will connect to this  
                // Server Socket must know this network 
                // Address 
                listener.Bind(localEndPoint);

                // Using Listen() method we create  
                // the Client list that will want 
                // to connect to Server 
                listener.Listen(10);

                while (true)
                {

                    Log("Waiting connection ... ");
                    // Suspend while waiting for 
                    // incoming connection Using  
                    // Accept() method the server  
                    // will accept connection of client 
                    Socket clientSocket = listener.Accept();

                    // Data buffer 
                    byte[] bytes = new byte[1024 * 1024];
                    bool messageComplete = false;

                    while (true)
                    {

                        int numByte = clientSocket.Receive(bytes);
                        string jsonData = ASCIIEncoding.ASCII.GetString(bytes);
                        Request req = JsonConvert.DeserializeObject<Request>(jsonData);
                        
                        //Request req = DataManager.Deserialize<Request>(bytes);
                        Log(string.Format("Text Request Type -> {0} ", req.type.ToString()));

                       
                        switch (req.type)
                        {
                            case RequestType.GET_GUILD:
                                {

                                    //Get guild id from arguments
                                    long guild_id = (long) req.args[0];

                                    Guild g = null;
                                    using (var db = new LiteDB.LiteDatabase(Common.dbConnectionString)){
                                        g = Common.dbInterface.GetGuild(db, (ulong) guild_id);
                                    }

                                    //Send g to client

                                    byte[] message = DataManager.Serialize(g);
                                    messageComplete = true;
                                    Console.WriteLine("Message Serialized to {0} bytes. Sending to Client", message.Length);
                                    // Send a message to Client 
                                    // using Send() method 
                                    clientSocket.Send(message);
                                    break;
                                }
                            case RequestType.GET_TOURNAMENT:
                                {
                                    //Get tour id from arguments
                                    long tourn_id = (long) req.args[0];

                                    Tournament t = null;
                                    using (var db = new LiteDB.LiteDatabase(Common.dbConnectionString))
                                    {
                                        t = Common.dbInterface.GetTournament(db, tourn_id);
                                    }

                                    //Send t to client

                                    byte[] message = DataManager.Serialize(t);
                                    messageComplete = true;
                                    // Send a message to Client  
                                    // using Send() method 
                                    clientSocket.Send(message);
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("Unsupported Request {0}", req.type);
                                    break;
                                }
                        }

                        if (messageComplete)
                        {
                            break;
                        }

                    }

                    // Close client Socket using the 
                    // Close() method. After closing, 
                    // we can use the closed Socket  
                    // for a new Client Connection 
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
