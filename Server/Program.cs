using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.IO;
using MySqlConnector;
using TournamentBackend;
using System.Threading;


namespace BotServer
{
    public class Program
    {
        public static void discordLog(string msg)
        {
            StreamWriter tx = new StreamWriter("discord.out", true);
            tx.WriteLine(msg);
            tx.Close();
            Console.WriteLine(msg);
        }
        
        static void Main(string[] args)
        {
            //Register Callbacks
            Common.loggerFunc = discordLog;
            Common.Populate(); //Add ranks and other defaults
            
            DiscordDataService _data = new DiscordDataService();
            _data.setTournament(new Tournament());
            
            //Setup Bot
            Bot _bot = new Bot(_data);
            //Start Discord Bot
            _bot.MainAsync();


            //Start Server Listening Thread
            Server _server = new Server();
            Thread t = new Thread(_server.Main);
            t.Start();
            
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("Type something");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "Q":
                        exit = true;
                        break;
                }
            }
            
            t.Abort();
            _bot.Close();
        }
    }

}