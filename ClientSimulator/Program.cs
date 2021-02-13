using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TournamentBackend;
using Newtonsoft.Json;


namespace ClientSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Request req = new Request();
            req.type = RequestType.GET_GUILD;
            req.args.Add(802165536905363456);


            ClientListener cl = new ClientListener();
            Guild g = cl.ExecuteClient(req) as Guild;
            
            
            Console.WriteLine("Hello World!");
            

        }
    }
    
    public class ClientListener
    {
        // ExecuteClient() Method 
        public object ExecuteClient(Request req)
        {
            try
            {
                // Establish the remote endpoint  
                // for the socket. This example  
                // uses port 11111 on the local  
                // computer. 
                string server_ip = "18.198.3.51";
                IPAddress ipAddr = IPAddress.Parse(server_ip);
                //IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                //IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

                // Creation TCP/IP Socket using  
                // Socket Class Costructor 
                Socket sender = new Socket(ipAddr.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    // Connect Socket to the remote  
                    // endpoint using method Connect() 
                    sender.Connect(localEndPoint);

                    // We print EndPoint information  
                    // that we are connected 
                    Console.WriteLine("Socket connected to -> {0} ",
                                  sender.RemoteEndPoint.ToString());


                    //Serialize Request
                    byte[] reqData=null;
                    try
                    {
                        //reqData = DataManager.Serialize(req);
                        string jsondata = JsonConvert.SerializeObject(req);
                        reqData = ASCIIEncoding.ASCII.GetBytes(jsondata);
                    } catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null;
                    }
                    
                    int byteSent = sender.Send(reqData);

                    MemoryStream ms;
                    using (ms = new MemoryStream())
                    {
                        while (true)
                        {
                            byte[] buff = new byte[65535];
                            int byteRecv = sender.Receive(buff);
                            //Console.WriteLine("Received {0} bytes of the message", byteRecv);
                            ms.Write(buff,0,byteRecv);
                            if (byteRecv == 0)
                                break;
                        }    
                    }
                    
                    byte[] messageReceived = ms.ToArray(); 
                    
                    
                    object ob = null;

                    switch (req.type)
                    {
                        case RequestType.GET_TOURNAMENT:
                        {
                            ob = DataManager.Deserialize<Tournament>(messageReceived);
                            break;
                        }
                        case RequestType.GET_GUILD:
                        {
                            ob = DataManager.Deserialize<Guild>(messageReceived);
                            break;
                        }
                    }


                    // Close Socket using  
                    // the method Close() 
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                    return ob;
                }

                // Manage of Socket's Exceptions 
                catch (ArgumentNullException ane)
                {

                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }

                catch (SocketException se)
                {

                    Console.WriteLine("SocketException : {0}", se.ToString());
                }

                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }

            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }

            return null;
        }
    }
    
}