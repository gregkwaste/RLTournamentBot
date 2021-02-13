using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using TournamentBackend;
using Newtonsoft.Json;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Drawing;
using System.Windows.Data;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ClientListener cl;
        private long GuildID = 802165536905363456;

        //Private props
        private Guild _g = null;
        private Tournament _t = null;
        private long ActiveTournamentID = 0;
            
        private void SetRankImages()
        {
            foreach (Rank rank in Common.RankEnumMap.Values)
            {
                //Set Image
                switch (rank._rank)
                {
                    case RL_RANK.BRONZE_I:
                        rank.ImagePath = "pack://application:,,,/Resources/Bronze1_rank_icon.png";
                        break;
                    case RL_RANK.BRONZE_II:
                        rank.ImagePath = "pack://application:,,,/Resources/Bronze2_rank_icon.png";
                        break;
                    case RL_RANK.BRONZE_III:
                        rank.ImagePath = "pack://application:,,,/Resources/Bronze3_rank_icon.png";
                        break;
                    case RL_RANK.SILVER_I:
                        rank.ImagePath = "pack://application:,,,/Resources/Silver1_rank_icon.png";
                        break;
                    case RL_RANK.SILVER_II:
                        rank.ImagePath = "pack://application:,,,/Resources/Silver2_rank_icon.png";
                        break;
                    case RL_RANK.SILVER_III:
                        rank.ImagePath = "pack://application:,,,/Resources/Silver3_rank_icon.png";
                        break;
                    case RL_RANK.GOLD_I:
                        rank.ImagePath = "pack://application:,,,/Resources/Gold1_rank_icon.png";
                        break;
                    case RL_RANK.GOLD_II:
                        rank.ImagePath = "pack://application:,,,/Resources/Gold2_rank_icon.png";
                        break;
                    case RL_RANK.GOLD_III:
                        rank.ImagePath = "pack://application:,,,/Resources/Gold3_rank_icon.png";
                        break;
                    case RL_RANK.PLATINUM_I:
                        rank.ImagePath = "pack://application:,,,/Resources/Platinum1_rank_icon.png";
                        break;
                    case RL_RANK.PLATINUM_II:
                        rank.ImagePath = "pack://application:,,,/Resources/Platinum2_rank_icon.png";
                        break;
                    case RL_RANK.PLATINUM_III:
                        rank.ImagePath = "pack://application:,,,/Resources/Platinum3_rank_icon.png";
                        break;
                    case RL_RANK.DIAMOND_I:
                        rank.ImagePath = "pack://application:,,,/Resources/Diamond1_rank_icon.png";
                        break;
                    case RL_RANK.DIAMOND_II:
                        rank.ImagePath = "pack://application:,,,/Resources/Diamond2_rank_icon.png";
                        break;
                    case RL_RANK.DIAMOND_III:
                        rank.ImagePath = "pack://application:,,,/Resources/Diamond3_rank_icon.png";
                        break;
                    case RL_RANK.CHAMPION_I:
                        rank.ImagePath = "pack://application:,,,/Resources/Champion1_rank_icon.png";
                        break;
                    case RL_RANK.CHAMPION_II:
                        rank.ImagePath = "pack://application:,,,/Resources/Champion2_rank_icon.png";
                        break;
                    case RL_RANK.CHAMPION_III:
                        rank.ImagePath = "pack://application:,,,/Resources/Champion3_rank_icon.png";
                        break;
                    case RL_RANK.GRAND_CHAMPION_I:
                        rank.ImagePath = "pack://application:,,,/Resources/Grand_champion1_rank_icon.png";
                        break;
                    case RL_RANK.GRAND_CHAMPION_II:
                        rank.ImagePath = "pack://application:,,,/Resources/Grand_champion2_rank_icon.png";
                        break;
                    case RL_RANK.GRAND_CHAMPION_III:
                        rank.ImagePath = "pack://application:,,,/Resources/Grand_champion3_rank_icon.png";
                        break;
                    case RL_RANK.SUPERSONIC_LEGEND:
                        rank.ImagePath = "pack://application:,,,/Resources/Supersonic_Legend_rank_icon.png";
                        break;
                    default:
                        rank.ImagePath = "";
                        break;
                }
}

        }

        public MainWindow()
        {
            InitializeComponent();
            Common.Populate(); //Add ranks and other defaults
            SetRankImages();

            cl = new ClientListener();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            

        }


        private void RequestTournament(long tourn_uid)
        {
            //Send Request to server to fetch tournament data
            Request req = new Request();
            req.type = RequestType.GET_TOURNAMENT;
            req.args.Add(tourn_uid);

            _t = cl.ExecuteClient(req) as Tournament;

            TournamentPresenter.Content = _t;
        }

        private void OpenTournament(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Console.WriteLine("Opening Tournament...");

            ListViewItem lv = sender as ListViewItem;

            TournamentInfo tInfo = lv.Content as TournamentInfo;
            ActiveTournamentID = tInfo.UID;
            RequestTournament(tInfo.UID);
        }

        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListViewItem lv = sender as ListViewItem;
            Player p = lv.Content as Player;

            Console.WriteLine("View Player {0}", p.Name);
        }

        private void ExportBracket(object sender, RoutedEventArgs e)
        {
            Button _b = sender as Button;

            if (_t.bracket != null)
            {
                //Update Image control
                Grid g = (_b.Parent as DockPanel).Parent as Grid;

                System.Windows.Controls.Image img = g.FindName("BracketImage") as System.Windows.Controls.Image;
                //img.Source = null;
                
                _t.ExportBracket();

                using (Bitmap localImage = new Bitmap("bracket.png"))
                {
                    MemoryStream stream = new MemoryStream();
                    localImage.Save(stream, ImageFormat.Png);
                    stream.Position = 0;
                    
                    BitmapImage result = new BitmapImage();
                    result.BeginInit();
                    // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                    // Force the bitmap to load right now so we can dispose the stream.
                    result.CacheOption = BitmapCacheOption.OnLoad;
                    result.StreamSource = stream;
                    result.EndInit();
                    result.Freeze();
                    img.Source = result;
                }
            }
        }

        private void ConnectToDB(object sender, RoutedEventArgs e)
        {
            Request req = new Request();
            req.type = RequestType.GET_GUILD;
            req.args.Add(GuildID);

            _g = cl.ExecuteClient(req) as Guild;

            if (_g != null)
            {
                GuildPresenter.Content = _g;
                Console.WriteLine("Guild Name: ", _g.Name);
                Console.WriteLine("Guild ActiveTournament ID: ", _g.ActiveTournamentID);
            }
        }

        private void RefreshTournament(object sender, RoutedEventArgs e)
        {
            if (ActiveTournamentID != 0)
                RequestTournament(ActiveTournamentID);
        }

        private void SaveGuildSettings(object sender, RoutedEventArgs e)
        {
            Button _b = sender as Button;
            
            Grid g = _b.Parent as Grid;

            TextBox tbox = g.FindName("GuildSettingsBox") as TextBox;
            //Try to manually deserialize
            try
            {
                JsonConvert.DeserializeObject<Settings>(tbox.Text);
                BindingExpression be = tbox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            } catch
            {
                MessageBox.Show("Malformed Json");
                return;
            }
            
        }
    }

    public class MatchupToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                bool m = value != null ? true : false;
                if (m)
                    return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGreen);
            }
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
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
                //string server_ip = "18.198.3.51";
                //IPAddress ipAddr = IPAddress.Parse(server_ip);
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = ipHost.AddressList[0];
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
  
