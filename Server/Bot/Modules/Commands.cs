using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using TournamentBackend;
using LiteDB;


namespace BotServer.Modules
{
    public class Commands :ModuleBase<SocketCommandContext>
    {
        public CommandService _commands { get; set; }
        
        [Command("test")]
        public async Task Test()
        {
            var user = Context.User;
            await ReplyAsync(Context.User.Mention + " eisai ilithios");
        }
        
        [Command("hi")]
        public async Task Hi()
        {
            var user = Context.User;
            await ReplyAsync(Context.User.Mention + " Greg's Backend is saying hi!");
        }
        
        private string textPrepend(string s1, string s2, char sep)
        {
            return s2 + sep + s1;
        }

        private void getParentGroupName(ref string s, char splitter, ModuleInfo m)
        {
            if (m.Group != null)
                s = textPrepend(s, m.Group, splitter);
            if (m.Parent != null)
                getParentGroupName(ref s, splitter, m.Parent);
        }

        private string getCommandName(CommandInfo cmd, char splitter)
        {
            string name = cmd.Name;
            string groupName = "";
            getParentGroupName(ref groupName, splitter, cmd.Module);
            if (groupName != "")
                return string.Join(splitter, new string[2] { groupName, name });
            return name;
        }
        
        [Command("help")]
        public async Task Help()
        {
            List<CommandInfo> commands = _commands.Commands.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();

            using (var db = new LiteDatabase(Common.dbConnectionString))
            {
                Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);

                foreach (CommandInfo command in commands)
                {
                    // Get the command Summary attribute information
                    string embedFieldText = command.Summary ?? "No description available\n";

                    embedBuilder.AddField(g.settings.textSettings.Prefix + getCommandName(command, ' '), embedFieldText);
                }
                await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
            }

        }



        //[Group("tourn")]
        public class TournamentModule : ModuleBase<SocketCommandContext>
        {

            [Command("leave")]
            public async Task Unjoin()
            {

                using (var db = new LiteDatabase(Common.dbConnectionString))
                {
                    MainPlayer mp = Common.dbInterface.GetMainPlayer(db, Context.User.Id);

                    if (mp == null)
                    {
                        await ReplyAsync(string.Format("Sorry {0} You are not registered anywhere, to leave...", Context.User.Mention));
                        return;
                    }

                    Tournament t = Common.dbInterface.GetTournament(db, mp.ActiveTournamentID);
                    
                    //Check if Player exists for tournament
                    if (!t.info.RegistrationsEnabled)
                    {
                        await ReplyAsync(string.Format("Sorry {0} Registrations are Closed!", Context.User.Mention));
                        return;
                    }

                    if (t.info.IsStarted)
                    {
                        await ReplyAsync(string.Format("Sorry {0} you can't leave. Tournament has already started!", Context.User.Mention));
                        return;
                    }

                    Player p = t.getPlayerbyDiscordID(Context.User.Id);

                    if (p == null)
                    {
                        await ReplyAsync(string.Format("{0} you have not joined the tournament. Nothing to do here...", Context.User.Mention));
                        return;
                    }

                    Common.dbInterface.RemovePlayerFromTournament(db, p);


                    //Remove role from user
                    ((SocketGuildUser)Context.User).RemoveRoleAsync(Context.Guild.GetRole(t.info.RoleID));
                    await ReplyAsync(string.Format("User {0} left the tournament", Context.User.Mention));

                }

            }

            [Command("join")]
            public async Task Join(string rank)
            {
                var user = Context.User;
                if (!rank.StartsWith("<:"))
                {
                    await ReplyAsync(user.Mention + " use an emote to define your rank");
                    return;
                }
                string rank_emote = rank.Split(':')[1];
                int rank_id = 0;
                switch (rank_emote)
                {
                    case "b1":
                        rank_id = 1;
                        break;
                    case "b2":
                        rank_id = 2;
                        break;
                    case "b3":
                        rank_id = 3;
                        break;
                    case "s1":
                        rank_id = 4;
                        break;
                    case "s2":
                        rank_id = 5;
                        break;
                    case "s3":
                        rank_id = 6;
                        break;
                    case "g1":
                        rank_id = 7;
                        break;
                    case "g2":
                        rank_id = 8;
                        break;
                    case "g3":
                        rank_id = 9;
                        break;
                    case "p1":
                        rank_id = 10;
                        break;
                    case "p2":
                        rank_id = 11;
                        break;
                    case "p3":
                        rank_id = 12;
                        break;
                    case "d1":
                        rank_id = 13;
                        break;
                    case "d2":
                        rank_id = 14;
                        break;
                    case "d3":
                        rank_id = 15;
                        break;
                    case "c1":
                        rank_id = 16;
                        break;
                    case "c2":
                        rank_id = 17;
                        break;
                    case "c3":
                        rank_id = 18;
                        break;
                    case "gc1":
                        rank_id = 19;
                        break;
                    case "gc2":
                        rank_id = 20;
                        break;
                    case "gc3":
                        rank_id = 21;
                        break;
                    case "ssl":
                        rank_id = 22;
                        break;
                    default:
                        rank_id = 0;
                        break;
                }

                if (rank_id == 0)
                {
                    var msg = await ReplyAsync(user.Mention + " Selected rank not found!");
                    await Task.Delay(500);
                    await msg.DeleteAsync();
                    return;
                }


                using (var db = new LiteDatabase(Common.dbConnectionString))
                {
                    //Get Guild Active Tournamente
                    Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                    
                    if (g.ActiveTournamentID == 0)
                    {
                        await ReplyAsync(string.Format("No active tournament in Guild. Wait for the manager...", Context.User.Mention));
                        return;
                    }

                    //Fetch Tournament
                    Tournament t = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);

                    
                    if (!t.info.RegistrationsEnabled)
                    {
                        await ReplyAsync(string.Format("Sorry {0} Registrations are Closed!", Context.User.Mention));
                        return;
                    }


                    //Check if Player exists in Database
                    MainPlayer mp = Common.dbInterface.GetMainPlayer(db, Context.User.Id);
                    
                    if (mp == null)
                    {
                        //Also register player to server

                        mp = new MainPlayer()
                        {
                            DiscordID = Context.User.Id,
                            ActiveTournamentID = g.ActiveTournamentID
                        };

                        Common.dbInterface.RegisterPlayerToDB(db, mp);
                    } 

                    Player p = t.getPlayerbyDiscordID(Context.User.Id);
                    
                    /* TODO BRING THAT CHECK BACK
                    if (p != null)
                    {
                        var msg = await ReplyAsync(user.Mention + " You have already joined!");
                        await Task.Delay(500);
                        await msg.DeleteAsync();
                        return;
                    }
                    */

                    //Create Player
                    p = new Player(Context.User.Username, rank_id);
                    p.mainPlayer = mp;
                    p.MainPlayerID = mp.ID;
                    p.TournamentID = t.info.UID;
                    p.ID = t._players.Count; // Set ID

                    //Register the player to the Tournament
                    Common.dbInterface.registerPlayerToTournament(db, p);

                    //Update main Player
                    mp.ActiveTournamentID = t.info.UID;
                    Common.dbInterface.UpdateMainPlayer(db, mp);

                    //Assign Tournament Role to user
                    var u = user as SocketGuildUser;
                    await u.AddRoleAsync(Context.Guild.GetRole(t.info.RoleID));
                    await ReplyAsync(string.Format("{0} has successfully joined the tournament", user.Mention));

                    await Task.Delay(500);
                    await Context.Message.DeleteAsync();
                }
            }

            public static bool checkIfContextUserAdmin(SocketCommandContext _ctx)
            {
                var u = _ctx.User as SocketGuildUser;

                if (u.GuildPermissions.Administrator)
                    return true;

                var msg = _ctx.Channel.SendMessageAsync("Admin Only Command").GetAwaiter().GetResult();
                System.Threading.Thread.Sleep(2000);
                msg.DeleteAsync().GetAwaiter().GetResult();
                return false;
            }

            public static string getPlayerDiscName(Player p, SocketCommandContext _ctx)
            {
                if (PlayerHasDiscord(p))
                {
                    SocketGuildUser u = _ctx.Guild.GetUser(p.mainPlayer.DiscordID);

                    if (u != null)
                        return u.Mention;
                }
                return p.Name;
            }

            public static bool PlayerHasDiscord(Player p)
            {
                if (p.mainPlayer.DiscordID != 0)
                    return true;
                return false;
            }

            [Command("create")]
            [Summary("Initialize Tournament")]
            public async Task CreateTourney(string type, string team_gen_method)
            {
                if (!checkIfContextUserAdmin(Context))
                    return;


                using (var db = new LiteDatabase(Common.dbConnectionString))
                {
                    Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                    
                    TournamentType _type;
                    switch (type)
                    {
                        case "1s":
                            _type = TournamentType.SOLO;
                            break;
                        case "2s":
                            _type = TournamentType.DOUBLES;
                            break;
                        case "3s":
                            _type = TournamentType.TRIPLES;
                            break;
                        default:
                            _type = TournamentType.DOUBLES; //Default choice
                            break;
                    }


                    TournamentTeamGenMethod _method;
                    switch (team_gen_method)
                    {
                        case "random":
                            _method = TournamentTeamGenMethod.RANDOM;
                            break;
                        case "register":
                            _method = TournamentTeamGenMethod.REGISTER;
                            break;
                        default:
                            _method = TournamentTeamGenMethod.RANDOM; //Default choice
                            break;
                    }

                    

                    //Create Channel Category
                    var channel_cat = await Context.Guild.CreateCategoryChannelAsync("RLFridays");

                    //Create Text Channels
                    var mgmt_channel = await Context.Guild.CreateTextChannelAsync("TOURNAMENT_MANAGEMENT");
                    var reg_channel = await Context.Guild.CreateTextChannelAsync("TOURNAMENT_REGISTRATION");
                    var ann_channel = await Context.Guild.CreateTextChannelAsync("TOURNAMENT_ANNOUNCEMENTS");

                    await mgmt_channel.ModifyAsync(prop => prop.CategoryId = channel_cat.Id);
                    await reg_channel.ModifyAsync(prop => prop.CategoryId = channel_cat.Id);
                    await ann_channel.ModifyAsync(prop => prop.CategoryId = channel_cat.Id);

                    //Set Channel Permissions
                    await mgmt_channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                        OverwritePermissions.DenyAll(mgmt_channel));

                    await ann_channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                        OverwritePermissions.DenyAll(ann_channel).Modify(viewChannel: PermValue.Allow, readMessageHistory: PermValue.Allow));

                    //Create Tournament Role

                    var permissions = new Discord.GuildPermissions();
                    permissions.Modify(false, false, false, false, false, false, true,
                        false, true, true, true, true, false, false, true, false, false, true,
                        true, true, false, false, false, false, false, false, false, false, false, false);

                    //permissions.Add(Discord.GuildPermission.ReadMessages);
                    var role = await Context.Guild.CreateRoleAsync(g.settings.textSettings.tournRoleName, permissions, Color.Blue, false, null);

                    //Create Tournament
                    Tournament t = new Tournament();
                    t.info.TypeID = (int) _type;
                    t.info.GuildID = g.Id;
                    t.info.TeamGenMethodID = (int) _method;
                    t.info.AnnouncementChannelID = ann_channel.Id;
                    t.info.ManagementChannelID = mgmt_channel.Id;
                    t.info.RegistationChannelID = reg_channel.Id;
                    t.info.CategoryChannelID = channel_cat.Id;
                    t.info.RoleID = role.Id;

                    //Register Tournament to Database
                    g.ActiveTournamentID = Common.dbInterface.registerTournamentToDB(db, t.info);
                    Common.dbInterface.UpdateGuild(db, g);
                     
                    //Report tournament id to management channel
                    await mgmt_channel.SendMessageAsync(string.Format("Successfully generated tournament {0}", g.ActiveTournamentID));

                    EmbedFooterBuilder footerBuilder = new EmbedFooterBuilder();
                    footerBuilder.Text = g.settings.textSettings.embed_footer;

                    EmbedBuilder builder = new EmbedBuilder();
                    builder.WithTitle("RLFriday " + DateTime.Now);
                    builder.Description = g.settings.textSettings.desc_TOURNAMENT_START;
                    builder.WithThumbnailUrl(g.settings.textSettings.thumbnail_URL);
                    builder.WithColor(Color.Blue);
                    builder.Footer = footerBuilder;
                    var msg = await ann_channel.SendMessageAsync("", false, builder.Build());
                
                }
            }

            private void deleteTextChannels(ref Tournament t)
            {
                if (t.info.ManagementChannelID > 0)
                {
                    Context.Guild.GetChannel(t.info.ManagementChannelID).DeleteAsync();
                    System.Threading.Thread.Sleep(100);
                }
                    
                if (t.info.AnnouncementChannelID > 0)
                {
                    Context.Guild.GetChannel(t.info.AnnouncementChannelID).DeleteAsync();
                    System.Threading.Thread.Sleep(100);
                }
                    
                if (t.info.ScoreReportChannelID > 0)
                {
                    Context.Guild.GetChannel(t.info.ScoreReportChannelID).DeleteAsync();
                    System.Threading.Thread.Sleep(100);
                }

                    
                if (t.info.RegistationChannelID > 0)
                {
                    Context.Guild.GetChannel(t.info.RegistationChannelID).DeleteAsync();
                    System.Threading.Thread.Sleep(100);
                }
                    

                if (t.info.CategoryChannelID > 0)
                {
                    Context.Guild.GetCategoryChannel(t.info.CategoryChannelID).DeleteAsync();
                    System.Threading.Thread.Sleep(100);
                }
                    
                if (t.info.RoleID > 0)
                {
                    Context.Guild.GetRole(t.info.RoleID).DeleteAsync();
                    System.Threading.Thread.Sleep(100);
                }
                    
                
                t.info.RoleID = 0;
                t.info.ManagementChannelID = 0;
                t.info.AnnouncementChannelID = 0;
                t.info.ScoreReportChannelID = 0;
                t.info.CategoryChannelID = 0;
    
            }

            [Command("end")]
            [Summary("End Tournament")]
            public async Task EndTourney()
            {
                if (!checkIfContextUserAdmin(Context))
                {
                    var msg1 = await ReplyAsync(string.Format("Isa mwrh saloufa thes na kaneis kai end"));
                    System.Threading.Thread.Sleep(5000);
                    await Context.Channel.DeleteMessageAsync(msg1.Id);
                    return;
                }

                using (var db = new LiteDatabase(Common.dbConnectionString))
                {
                    Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                    Tournament t = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);
                    t.info.IsFinished = true; //Force finished status
                    deleteTextChannels(ref t);
                    //deleteVCs();
                    //deletePlayerRoles();    
                    //Reset active tournament Id
                    //Update tournament details
                    Common.dbInterface.UpdateTournamentInfo(db, t.info);

                    //Update new active tournamentID
                    var collection = db.GetCollection<Tournament>("tournaments");
                    List<Tournament> guild_tourneys = collection.Find(x=>x.info.GuildID == g.Id).ToList();

                    if (guild_tourneys.Count > 0)
                    {
                        g.ActiveTournamentID = guild_tourneys.Last().info.UID;
                        Common.dbInterface.UpdateGuild(db, g);
                    }
                }

                await Context.Guild.SystemChannel.SendMessageAsync("Tournament Ended *GGs*");
            }


            [Command("start")]
            [Summary("Start the tournament")]
            public async Task StartTourney()
            {
                var u = Context.User as SocketGuildUser;
                if (!checkIfContextUserAdmin(Context))
                    return;

                using (var db = new LiteDatabase(Common.dbConnectionString))
                {
                    Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                    Tournament t = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);

                    if (!t.info.BracketGenerated)
                    {
                        await Context.Channel.SendMessageAsync(string.Format("Bracked not generated."));
                        return;
                    }

                    //Delete Registration Channel
                    var reg_chl = Context.Guild.GetChannel(t.info.RegistationChannelID);
                    if (reg_chl != null)
                    {
                        await Context.Guild.GetChannel(t.info.RegistationChannelID).DeleteAsync();
                        t.info.RegistationChannelID = 0;
                    }

                    //Create Score Report Channel
                    var score_chl = await Context.Guild.CreateTextChannelAsync("TOURNAMENT_SCORE_REPORT");
                    await score_chl.ModifyAsync(prop => prop.CategoryId = t.info.CategoryChannelID);
                    t.info.ScoreReportChannelID = score_chl.Id;

                    //createVCs(); TODO
                    var ann_chl = Context.Guild.GetChannel(t.info.AnnouncementChannelID) as SocketTextChannel;
                    var role = Context.Guild.GetRole(t.info.RoleID);
                    await ann_chl.SendMessageAsync(string.Format("{0} Tournament Has Officially Started!. Use {1} to report your match results",
                        role.Mention, score_chl.Mention));

                    t.info.IsStarted = true;

                    //Update ChannelManagerInfo to the DB
                    Common.dbInterface.UpdateTournamentInfo(db, t.info);

                    advance(db, Context, t); //Automatically advance tourney
                }
            }


            [Command("advance")]
            [Summary("Checks tournament progress")]
            public async Task Advance()
            {
                using (var db = new LiteDatabase(Common.dbConnectionString))
                {
                    Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                    Tournament t = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);

                    //This command should report to the accouncment channel
                    var _mgmtChannel = Context.Guild.GetChannel(t.info.ManagementChannelID) as ISocketMessageChannel;

                    if (!t.info.BracketGenerated)
                    {
                        await _mgmtChannel.SendMessageAsync(string.Format("Bracked not generated."));
                        return;
                    }

                    if (!t.info.IsStarted)
                    {
                        await _mgmtChannel.SendMessageAsync(string.Format("Tournament Not Started Yet."));
                        return;
                    }

                    advance(db, Context, t); //Automatically advance tourney
                }
            }

            [Command("report")]
            [Summary("Report Score Result")]
            public async Task Report(string status)
            {
                using (var db = new LiteDatabase(Common.dbConnectionString))
                {
                    Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                    Tournament t = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);

                    //This command should report to the scorereport channel
                    var _channel = Context.Guild.GetChannel(t.info.ScoreReportChannelID) as ISocketMessageChannel;

                    if (status != "W" && status != "L")
                    {
                        await _channel.SendMessageAsync(string.Format("{0} Wrong Match Report Status. Report **W** if you won or **L** if you lost", Context.User.Mention));
                        return;
                    }

                    Player p = t.getPlayerbyDiscordID(Context.User.Id);
                    reportResult(db, ref t, p, status);
                }
            }


            private void reportResult(LiteDatabase db, ref Tournament t, Player p, string status)
            {
                //This command should report to the scorereport channel
                var _scoreChannel = Context.Guild.GetChannel(t.info.ScoreReportChannelID) as ISocketMessageChannel;
                var _mgmtChannel = Context.Guild.GetChannel(t.info.ManagementChannelID) as ISocketMessageChannel;

                //Find the Authors active matchup
                bool matchup_found = false;
                foreach (Round round in t.bracket.Rounds)
                {
                    foreach (Matchup match in round.Matchups)
                    {
                        if (match.InProgress)
                        {
                            //Search for player discord ID. First message is sent always to the captain of the first team
                            if (match.Team1.Captain.mainPlayer.DiscordID == p.mainPlayer.DiscordID)
                            {
                                matchup_found = true;

                                if (status == "W")
                                    match.Team1ReportedWinner = match.Team1;
                                else
                                    match.Team1ReportedWinner = match.Team2;

                                //UpdateMatchups
                                Common.dbInterface.ApplyTournamentMatchupChanges(db, ref t);

                                //Notify Admins
                                _mgmtChannel.SendMessageAsync(string.Format("{0} reported {1} for Match {2} of Round {3}.",
                                                             getPlayerDiscName(match.Team1.Captain, Context), status, match.ID, round.ID));

                                if (match.Team2ReportedWinner == null)
                                {
                                    _scoreChannel.SendMessageAsync(string.Format("{0} reported {1} for Match {2} of Round {3}. Waiting {4} to report result",
                                                             getPlayerDiscName(match.Team1.Captain, Context), status, match.ID, round.ID, getPlayerDiscName(match.Team2.Captain, Context)));
                                }

                            }
                            else if (match.Team2.Captain.mainPlayer.DiscordID == p.mainPlayer.DiscordID)
                            {
                                matchup_found = true;

                                if (status == "W")
                                    match.Team2ReportedWinner = match.Team2;
                                else
                                    match.Team2ReportedWinner = match.Team1;

                                //UpdateMatchups
                                Common.dbInterface.ApplyTournamentMatchupChanges(db, ref t);

                                //Notify Admins
                                _mgmtChannel.SendMessageAsync(string.Format("{0} reported {1} for Match {2} of Round {3}.",
                                                             getPlayerDiscName(match.Team2.Captain, Context), status, match.ID, round.ID));

                                if (match.Team1ReportedWinner == null)
                                {
                                    _scoreChannel.SendMessageAsync(string.Format("{0} reported {1} for Match {2} of Round {3}. Waiting {4} to report result",
                                                             getPlayerDiscName(match.Team2.Captain, Context), status, match.ID, round.ID, getPlayerDiscName(match.Team1.Captain, Context)));
                                }
                            }

                            //Try to conclude match
                            if (match.Team2ReportedWinner != null && match.Team2ReportedWinner == match.Team1ReportedWinner && match.Winner == null)
                            {
                                match.Winner = match.Team1ReportedWinner;
                                _scoreChannel.SendMessageAsync(string.Format("Round {0} - Match {1} - {2} Won. Congratulations!",
                                                                 round.ID, match.ID, match.Winner.Name));
                                _mgmtChannel.SendMessageAsync(string.Format("Round {0} - Match {1} - {2} Won. Congratulations!",
                                                                 round.ID, match.ID, match.Winner.Name));
                                advance(db, Context, t); //Automatically advance tourney
                                return;

                            }
                            else if (match.Team1ReportedWinner != null && match.Team2ReportedWinner != null && match.Team2ReportedWinner != match.Team1ReportedWinner)
                            {
                                _scoreChannel.SendMessageAsync(string.Format("Round {0} - Match {1} - Different result reports detected. Tournament Manager has been notified to resolve the issue.",
                                                                 round.ID, match.ID));
                                _mgmtChannel.SendMessageAsync(string.Format("Round {0} - Match {1} : {2} vs {3} Wrong reports. Set the result manually",
                                                                 round.ID, match.ID, match.Team1.Name, match.Team2.Name));
                                advance(db, Context, t);//Automatically advance tourney
                                return;
                            }
                        }
                    }
                }

                if (!matchup_found)
                    _scoreChannel.SendMessageAsync(string.Format("No active match found for {0}", Context.User.Mention));

            }


            private void forceResult(LiteDatabase db, ref Tournament t, Player p, string status)
            {

                //This command should report to the scorereport channel
                var _scoreChannel = Context.Guild.GetChannel(t.info.ScoreReportChannelID) as ISocketMessageChannel;
                var _mgmtChannel = Context.Guild.GetChannel(t.info.ManagementChannelID) as ISocketMessageChannel;

                //Find the Authors active matchup
                bool matchup_found = false;
                foreach (Round round in t.bracket.Rounds)
                {
                    foreach (Matchup match in round.Matchups)
                    {
                        if (match.InProgress)
                        {
                            //Search for player discord ID. First message is sent always to the captain of the first team
                            if (match.Team1.Captain.mainPlayer.DiscordID == p.mainPlayer.DiscordID)
                            {
                                matchup_found = true;

                                if (status == "W")
                                    match.Winner = match.Team1;
                                else
                                    match.Winner = match.Team2;

                                match.Team1ReportedWinner = match.Winner;
                                match.Team2ReportedWinner = match.Winner;

                            }
                            else if (match.Team2.Captain.mainPlayer.DiscordID == p.mainPlayer.DiscordID)
                            {
                                matchup_found = true;

                                if (status == "W")
                                    match.Winner = match.Team2;
                                else
                                    match.Winner = match.Team1;

                                match.Team1ReportedWinner = match.Winner;
                                match.Team2ReportedWinner = match.Winner;
                            }

                            if (matchup_found)
                            {
                                //If matchup found the winner has been set
                                _scoreChannel.SendMessageAsync(string.Format("Round {0} - Match {1} - Forced win for {2}.",
                                                                    round.ID, match.ID, match.Winner.Name));
                                _mgmtChannel.SendMessageAsync(string.Format("Round {0} - Match {1} - Forced win for {2}.",
                                                                    round.ID, match.ID, match.Winner.Name));
                                advance(db, Context, t); //Automatically advance tourney
                                return;
                            }
                        }
                    }
                }

                if (!matchup_found)
                    _scoreChannel.SendMessageAsync(string.Format("No active match found for {0}", Context.User.Mention));

                //UpdateMatchups
                Common.dbInterface.ApplyTournamentMatchupChanges(db, ref t);

            }


            [Command("forcereport")]
            [Summary("Force Report Score Result. (Admin Only)")]
            public async Task ForceReport(string status, IUser user)
            {
                if (!checkIfContextUserAdmin(Context))
                    return;

                if (status != "W" && status != "L")
                {
                    await Context.Channel.SendMessageAsync(string.Format("{0} Wrong Match Report Status. Report **W** if you won or **L** if you lost", Context.User.Mention));
                    return;
                }

                using (var db = new LiteDatabase(Common.dbConnectionString))
                {
                    Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                    Tournament t = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);


                    Player p = t.getPlayerbyDiscordID(user.Id);

                    if (p == null)
                    {
                        await Context.Channel.SendMessageAsync("Input player is not registered to the tournament");
                        return;
                    }


                    forceResult(db, ref t, p, status);
                }
            }

            public static void advance(LiteDatabase db, SocketCommandContext _ctx, Tournament t)
            {
                //This command should report to the accouncment channel
                var _channel = _ctx.Guild.GetChannel(t.info.AnnouncementChannelID) as ISocketMessageChannel;
                var _mgmtChannel = _ctx.Guild.GetChannel(t.info.ManagementChannelID) as ISocketMessageChannel;

                //Iterate in tournament rounds
                for (int i = 0; i < t.bracket.Rounds.Count; i++)
                {
                    Round r = t.bracket.Rounds[i];
                    int active_matchups = 0;
                    foreach (Matchup match in r.Matchups)
                    {
                        if (match.Winner != null)
                        {
                            continue; //Match is concluded continue;
                        }

                        if (match.IsDummy && match.Winner != null)
                        {
                            continue; //Match is dummy but the winner status is already resolved. Nothing to do here
                        }

                        if (match.InProgress)
                        {
                            active_matchups++; //Match has already started and we are waiting for a result
                            continue;
                        }

                        if (!match.IsValid)
                        {
                            active_matchups++; //Match is pending to be populated probably from previous round
                            continue;
                        }
                        else
                        {
                            //Send Announcement
                            bool t1_hasDisc = PlayerHasDiscord(match.Team1.Captain);
                            bool t2_hasDisc = PlayerHasDiscord(match.Team2.Captain);

                            string t1_captain = getPlayerDiscName(match.Team1.Captain, _ctx);
                            string t2_captain = getPlayerDiscName(match.Team2.Captain, _ctx);

                            if (t1_hasDisc && t2_hasDisc)
                            {
                                //Generate RL Lobby
                                Random rand_gen = new Random();

                                match.LobbyName = "rlfriday" + rand_gen.Next(1, 200);
                                match.LobbyPass = rand_gen.Next(1000, 9999).ToString();

                                //Send DM to t1 captain
                                _ctx.Guild.GetUser(match.Team1.Captain.mainPlayer.DiscordID).SendMessageAsync(string.Format("Create a Lobby with name: {0} and pass {1}. " +
                                    "Reply with !ready when you have created the lobby and I will send the credentials to the opposing team. Good luck!",
                                                                                    match.LobbyName, match.LobbyPass));
                                _ctx.Guild.GetUser(match.Team2.Captain.mainPlayer.DiscordID).SendMessageAsync(string.Format("Your opponents are responsible for creating a lobby this match!" +
                                     " When the lobby is ready you will receive the lobby credentials." +
                                    " If anything goes wrong, you can contact the opposing team captain here {0}. Good luck!", t1_captain));

                                _channel.SendMessageAsync(string.Format("Round {0} - Match {1} | {2} vs {3} | Captains {4}, {5} check your dms",
                                                                        i, match.ID, match.Team1.Name, match.Team2.Name, t1_captain, t2_captain));
                            }
                            else
                            {
                                _channel.SendMessageAsync(string.Format("Round {0} - Match {1} | {2} vs {3} | Captains {4}, {5} discord comms not supported",
                                                                        i, match.ID, match.Team1.Name, match.Team2.Name, t1_captain, t2_captain));
                            }

                            match.InProgress = true;
                        }

                    }
                    if (active_matchups > 0) break;
                }

                //Check if tournament has finished
                Matchup final = t.bracket.Rounds.Last().Matchups.Last();
                if (final.Winner != null)
                {
                    var _role = _ctx.Guild.GetRole(t.info.RoleID);
                    _channel.SendMessageAsync(string.Format("{1} DING DING DING WE HAVE A WINNER!!!!! Congrats to {0} for winning the tournament!",
                                                                        final.Winner.Name, _role.Mention));
                }

                //UpdateMatchups
                Common.dbInterface.ApplyTournamentMatchupChanges(db, ref t);

            }


            [Command("players")]
            [Summary("List Tournament Players")]
            public async Task Players()
            {
                using (var db = new LiteDatabase(Common.dbConnectionString))
                {
                    Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                    Tournament t = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);

                    EmbedBuilder builder = new EmbedBuilder();
                    builder.WithTitle("Registered Players");

                    foreach (Player p in t.Players)
                    {
                        string name = p.Name;
                        if (PlayerHasDiscord(p))
                            name = Context.Guild.GetUser(p.mainPlayer.DiscordID).Mention;
                        builder.AddField("Player " + p.ID.ToString(),
                            name);    // true - for inline
                    }

                    builder.WithColor(Color.Red);

                    //This command should be sent to the registration channel
                    var _channel = Context.Guild.GetChannel(t.info.AnnouncementChannelID) as ISocketMessageChannel;
                    await _channel.SendMessageAsync("", false, builder.Build());

                }
            }


            [Command("ready")]
            [Summary("Finalize Lobby Generation")]
            public async Task Ready()
            {
                using (var db = new LiteDatabase(Common.dbConnectionString))
                {
                    MainPlayer mp = Common.dbInterface.GetMainPlayer(db, Context.User.Id);
                    Tournament t = Common.dbInterface.GetTournament(db, mp.ActiveTournamentID);

                    foreach (Round round in t.bracket.Rounds)
                    {
                        foreach (Matchup match in round.Matchups)
                        {
                            if (match.InProgress)
                            {
                                //Search for player discord ID. First message is sent always to the captain of the first team
                                if (match.Team1.Captain.mainPlayer.DiscordID == Context.User.Id)
                                {
                                    //Send DM to t2 captain
                                    await Context.Client.GetUser(match.Team2.Captain.mainPlayer.DiscordID).SendMessageAsync(string.Format("Lobby has been created by {0} with name: {1} and pass {2}. Good Luck!",
                                        Context.User.Mention, match.LobbyName, match.LobbyPass));
                                    //Send DM to t1 captain
                                    await Context.User.SendMessageAsync(string.Format("Good Luck!"));
                                }
                            }
                        }
                    }
                }
            }

            private void createVCs(Tournament tourney)
            {
                foreach (Team t in tourney.Teams)
                {
                    Context.Guild.CreateVoiceChannelAsync("TOURNAMENT TEAM " + t.ID);
                }
            }

            private void deleteVCs(Tournament tourney)
            {
                foreach (Team t in tourney.Teams)
                {
                    foreach (var vc in Context.Guild.VoiceChannels)
                    {
                        if (vc.Name == "TOURNAMENT TEAM " + t.ID || vc.Name == "TOURNAMENT PLAYER POOL")
                        {
                            vc.DeleteAsync();
                        }
                    }
                }
            }

            [Group("registration")]
            public class RegistrationModule : ModuleBase<SocketCommandContext>
            {
                [Command("open")]
                [Summary("Enables Registrations")]
                public async Task Open()
                {
                    if (!checkIfContextUserAdmin(Context))
                        return;

                    using (var db = new LiteDatabase(Common.dbConnectionString))
                    {
                        Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                        
                        if (g.ActiveTournamentID == 0)
                        {
                            await Context.Channel.SendMessageAsync("No active tournament for Guild!");
                            return;
                        }
                        
                        Tournament t = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);
                        t.info.RegistrationsEnabled = true;

                        Common.dbInterface.UpdateTournamentInfo(db, t.info);

                        //Make announcement
                        //This command should be sent to the registration channel
                        EmbedFooterBuilder footerBuilder = new EmbedFooterBuilder();
                        footerBuilder.Text = "EGG Gang - Zimarulis - RL Fridays";
                        EmbedBuilder RegMessage = new EmbedBuilder();
                        RegMessage.WithTitle("Οι εγγραφές άνοιξαν!");
                        RegMessage.Description = "Οι εγγραφές άνοιξαν και θα παραμείνουν ανοιχτές μέχρι την έναρξη του τουρνουά στις 14:30! Θα ενημερωθείς με αντίστοιχο μύνημα όταν οι ομάδες φτιαχτούν.";
                        RegMessage.AddField("Πως δηλώνω συμμετοχή", "Δηλώσε συμμετοχή γράφοντας !join (emote του rank) όπως στην παρακάτω εικόνα", false);    // true - for inline
                        RegMessage.WithImageUrl("https://cdn.discordapp.com/attachments/805516317837885460/805516628505919488/unknown.png");
                        RegMessage.WithThumbnailUrl("https://cdn.discordapp.com/avatars/454232504182898689/ed55a0711ba007be7cfb11b1fa3e2075.png?size=128");
                        //builder.AddField("AOE", "63", true);
                        //builder.WithThumbnailUrl("https://static.wikia.nocookie.net/rocketleague/images/7/7a/Halo_topper_icon.png/revision/latest/scale-to-width-down/256?cb=20200422210226");
                        RegMessage.WithColor(Color.Blue);
                        RegMessage.Footer = footerBuilder;


                        var _channel = Context.Guild.GetChannel(t.info.RegistationChannelID) as ISocketMessageChannel;
                        if (_channel != null)
                        {
                            var msg = await _channel.SendMessageAsync("", false, RegMessage.Build());
                        }
                    }


                }

                [Command("close")]
                [Summary("Close Registrations")]
                public async Task Close()
                {
                    using (var db = new LiteDatabase(Common.dbConnectionString))
                    {
                        Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                        
                        if (g.ActiveTournamentID == 0)
                        {
                            await Context.Channel.SendMessageAsync("No active tournament for Guild!");
                            return;
                        }

                        Tournament t = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);
                        t.info.RegistrationsEnabled = false;
                        Common.dbInterface.UpdateTournamentInfo(db, t.info);

                        //Make announcement
                        //This command should be sent to the registration channel
                        var _channel = Context.Guild.GetChannel(t.info.RegistationChannelID) as ISocketMessageChannel;
                        if (_channel != null)
                        {
                            await _channel.SendMessageAsync("Tournament Registrations are Closed!");
                        }
                    }
                }

                


                
            }







            [Group("teams")]
            public class TeamsModule : ModuleBase<SocketCommandContext>
            {

                [Command("generate")]
                [Summary("Generate Teams")]
                public async Task Generate()
                {
                    using (var db = new LiteDatabase(Common.dbConnectionString))
                    {
                        Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                        Tournament t = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);

                        if (t.info.TeamsGenerated)
                        {
                            await ReplyAsync("Cannot regenerate teams");
                            return;
                        }

                        if (t._players.Count == 0)
                        {
                            await ReplyAsync("No Players Found.");
                            return;
                        }

                        if (t.info.RegistrationsEnabled)
                        {
                            await ReplyAsync("Cannot Generate Teams, registrations are still open!");
                            return;
                        }

                        if (t.info.TeamGenMethod != TournamentTeamGenMethod.RANDOM)
                        {
                            await ReplyAsync("Cannot Generate Teams, tournament is set to team registration mode");
                            return;
                        }

                        t.CreateTeams();
                        Common.dbInterface.registerAllTournamentTeams(db, t);
                        Common.dbInterface.UpdateTournamentInfo(db, t.info);
                        await ReplyAsync("Teams Successfully Generated");
                    }
                    await Post();
                }


                public static Team registerTeam(Tournament _tourney, Player p, string name)
                {
                    if (_tourney.info.Type == TournamentType.SOLO)
                    {
                        Team1s t = new Team1s();
                        t.TournamentID = _tourney.info.UID;
                        t.Name = name;
                        t.addPlayer(p);
                        t.ID = _tourney._teams.Count;
                        return t;
                    }

                    if (_tourney.info.Type == TournamentType.DOUBLES)
                    {
                        Team2s t = new Team2s();
                        t.TournamentID = _tourney.info.UID;
                        t.Name = name;
                        t.addPlayer(p);
                        t.ID = _tourney._teams.Count;
                        return t;
                    }

                    if (_tourney.info.Type == TournamentType.TRIPLES)
                    {
                        Team3s t = new Team3s();
                        t.TournamentID = _tourney.info.UID;
                        t.Name = name;
                        t.addPlayer(p);
                        t.ID = _tourney._teams.Count;
                        return t;
                    }

                    return null;
                }

                [Command("register")]
                [Summary("Register Team")]
                public async Task Register(string name)
                {
                    using (var db = new LiteDatabase(Common.dbConnectionString))
                    {
                        Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                        Tournament t = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);
                        
                        if (!t.info.RegistrationsEnabled)
                        {
                            await ReplyAsync("Registrations are closed. Wait for an announcement!");
                            return;
                        }

                        if (t.info.TeamGenMethod != TournamentTeamGenMethod.REGISTER)
                        {
                            await ReplyAsync("Cannot Register Teams, tournament is set to random team generation mode");
                            return;
                        }

                        //Make sure players exist
                        Player p = t.getPlayerbyDiscordID(Context.User.Id);

                        if (p == null)
                        {
                            await ReplyAsync("Players do not exist, make sure to join the tourney first!");
                            return;
                        }

                        if (p.team != null)
                        {
                            await ReplyAsync("You are already a member of a team. You cannot register a new team");
                            return;
                        }

                        //Create team
                        Team team = registerTeam(t, p, name);

                        if (team != null)
                        {
                            Common.dbInterface.initTeam(db, t, team, p);
                            await ReplyAsync("Team Successfully Generated");
                        }
                    }
                }

                [Command("invite")]
                [Summary("Invite Player to team")]
                public async Task Invite(IUser u)
                {
                    using (var db = new LiteDatabase(Common.dbConnectionString))
                    {
                        Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                        Tournament t = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);

                        if (t.info.TeamGenMethod != TournamentTeamGenMethod.REGISTER)
                        {
                            await ReplyAsync("Tournament does not support team registration");
                            return;
                        }

                        if (!t.info.RegistrationsEnabled)
                        {
                            await ReplyAsync("Registrations are closed. Wait for an announcement!");
                            return;
                        }

                        //Do not allow self invitations
                        /* TODO BRING THAT CHECK BACK
                        if (Context.User.Id == u.Id)
                        {
                            await ReplyAsync("You are not allowed to invite yourself -.-");
                            return;
                        }
                        */

                        //Make sure players exist
                        Player p = t.getPlayerbyDiscordID(Context.User.Id);

                        //Make sure team exists

                        if (p.team == null)
                        {
                            await ReplyAsync("You should register a team first!");
                            return;
                        }

                        //Make sure players exist
                        Player p_inv = t.getPlayerbyDiscordID(u.Id);

                        if (p_inv == null)
                        {
                            await ReplyAsync("Player does not exist, make sure to invite players that have joined the tournament!");
                            return;
                        }

                        TeamInvitation inv = new TeamInvitation()
                        {
                            teamID = p.team.UID,
                            playerID = p.MainPlayerID,
                            tournamentID = t.info.UID
                        };

                        long invitationId = Common.dbInterface.registerPlayerInvitation(db, inv);

                        //Send invitation
                        await u.SendMessageAsync(string.Format("You have been invited by {0} to join team {1}. If you want to accept this invitation reply with ```!teams accept {2}```. If you want to reject this invitation reply with ```!teams reject {2}```",
                            Context.User.Mention, p.team.Name, invitationId));
                    }
                }

                [Command("accept")]
                [Summary("Accept Team Invitation")]
                public async Task accept(int invitationID)
                {
                    using (var db = new LiteDatabase(Common.dbConnectionString))
                    {
                        MainPlayer mp = Common.dbInterface.GetMainPlayer(db, Context.User.Id);
                        
                        if (mp.ActiveTournamentID == 0)
                        {
                            await ReplyAsync("No active tournament found for player.");
                            //TODO Delete invitation here
                            return;
                        }
                        
                        Tournament t = Common.dbInterface.GetTournament(db, mp.ActiveTournamentID);

                        TeamInvitation teamInvitation = Common.dbInterface.GetPlayerInvitation(db, ref t, invitationID);


                        if (teamInvitation == null)
                        {
                            await ReplyAsync("Invitation not found");
                            return;
                        }

                        Player p = t.getPlayerbyDiscordID(Context.User.Id);

                        if (p == null)
                        {
                            await ReplyAsync("You have not joined the tournament yet. Make sure to join first!");
                            return;
                        }
                        
                        if (teamInvitation.accepted)
                        {
                            await ReplyAsync("Invitation expired");
                            return;
                        }

                        teamInvitation.accepted = true;
                        teamInvitation.pending = false;

                        //Register player to team
                        Common.dbInterface.registerTournamentTeamPlayer(db, t, teamInvitation.team, teamInvitation.player);
                        Common.dbInterface.UpdatePlayerInvitation(db, teamInvitation);
                        
                        //Send confirmation
                        await Context.User.SendMessageAsync(string.Format("You have joined team \"{0}\".", teamInvitation.team.Name));
                    }
                }
                
                
                [Command("reject")]
                [Summary("Reject Team Invitation")]
                public async Task reject(int invitationID)
                {
                    using (var db = new LiteDatabase(Common.dbConnectionString))
                    {
                        MainPlayer mp = Common.dbInterface.GetMainPlayer(db, Context.User.Id);

                        if (mp.ActiveTournamentID == 0)
                        {
                            await ReplyAsync("No active tournament found for player.");
                            //TODO Delete invitation here
                            return;
                        }

                        Tournament t = Common.dbInterface.GetTournament(db, mp.ActiveTournamentID);
                        TeamInvitation teamInvitation = Common.dbInterface.GetPlayerInvitation(db, ref t, invitationID);


                        if (teamInvitation == null)
                        {
                            await ReplyAsync("Invitation not found");
                            return;
                        }

                        Player p = t.getPlayerbyDiscordID(Context.User.Id);

                        if (!teamInvitation.pending)
                        {
                            await ReplyAsync("Invitation expired");
                            return;
                        }

                        if (p == null)
                        {
                            await ReplyAsync("You have not joined the tournament yet. Make sure to join first!");
                            return;
                        }

                        teamInvitation.accepted = false;
                        teamInvitation.pending = false;

                        Common.dbInterface.UpdatePlayerInvitation(db, teamInvitation);

                        //Send confirmation
                        await Context.User.SendMessageAsync(string.Format("Invitation {0} Rejected", invitationID));
                    }

                }
                

                [Command("post")]
                [Summary("Announce Teams")]
                public async Task Post()
                {
                    using (var db = new LiteDatabase(Common.dbConnectionString))
                    {
                        Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                        Tournament tourney = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);

                        EmbedFooterBuilder footerBuilder = new EmbedFooterBuilder();
                        footerBuilder.Text = "EGG Gang - Zimarulis - RL Fridays";

                        EmbedBuilder builder = new EmbedBuilder();
                        builder.WithTitle("Registered Teams");
                        builder.WithDescription("Επικοινωνήστε με τους συμπαίκτες σας και βρείτε τους in game.");
                        builder.WithThumbnailUrl(g.settings.textSettings.thumbnail_URL);

                        foreach (Team t in tourney._teams)
                        {
                            List<string> mentions = new List<string>();
                            for (int i = 0; i < t.Players.Count; i++)
                            {
                                Player p = t.Players[i];
                                if (p == null)
                                    mentions.Add("EMPTY_SLOT");
                                else
                                    mentions.Add(getPlayerDiscName(p, Context) + Common.emoteMap(g.settings, p.Rank._rank));
                            }
                            builder.AddField(t.Name, string.Join(" | ", mentions), false);
                        }

                        builder.WithColor(Color.Red);

                        //This command should be sent to the registration channel
                        var _channel = Context.Guild.GetChannel(tourney.info.AnnouncementChannelID) as ISocketMessageChannel;
                        await _channel.SendMessageAsync("", false, builder.Build());
                    }
                }

                [Command("info")]
                [Summary("Fetch Team Info")]
                public async Task Info(int id)
                {
                    using (var db = new LiteDatabase(Common.dbConnectionString))
                    {
                        Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                        Tournament tourney = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);

                        try
                        {
                            Team t = tourney.Teams[id];
                            EmbedBuilder builder = new EmbedBuilder();
                            builder.WithTitle(t.Name);

                            for (int i = 0; i < t.Players.Count; i++)
                            {
                                Player p = t.Players[i];
                                if (i == 0)
                                {
                                    builder.AddField("Player " + i, TournamentModule.getPlayerDiscName(p, Context), true);
                                }
                                else
                                    builder.AddField("Player " + i, TournamentModule.getPlayerDiscName(p, Context), true);
                            }

                            builder.WithColor(Color.Red);

                            //This command should be sent to the registration channel
                            await Context.Channel.SendMessageAsync("", false, builder.Build());

                        }
                        catch (Exception e)
                        {
                            await ReplyAsync(string.Format("Team {0} not found", id));
                        }



                    }
                }






            }


            [Group("bracket")]
            public class BracketModule : ModuleBase<SocketCommandContext>
            {

                [Command("generate")]
                [Summary("Generate Bracket")]
                public async Task Generate()
                {
                    using (var db = new LiteDatabase(Common.dbConnectionString))
                    {
                        Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                        Tournament t = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);

                        if (t._teams.Count == 0)
                        {
                            await ReplyAsync("Please Generate Teams first");
                            return;
                        }

                        if (t.info.BracketGenerated)
                        {
                            await ReplyAsync("Bracket Already Generated");
                            return;
                        }
                        //Create Bracket
                        t.CreateBracket();
                        t.info.BracketGenerated = true;
                        
                        Common.dbInterface.UpdateTournamentInfo(db, t.info); //Tournament Info
                        Common.dbInterface.registerBracket(db, ref t);
                        await ReplyAsync("Bracket Successfully Generated");
                    }
                    await Show();
                }

                [Command("post")]
                [Summary("Post Bracket to Discord")]
                public async Task Show()
                {
                    using (var db = new LiteDatabase(Common.dbConnectionString))
                    {
                        Guild g = Common.dbInterface.GetGuild(db, Context.Guild.Id);
                        Tournament t = Common.dbInterface.GetTournament(db, g.ActiveTournamentID);


                        if (!t.info.BracketGenerated)
                        {
                            await ReplyAsync("Please Generate bracket first");
                            return;
                        }

                        t.bracket.GenerateSVG();
                        var _channel = Context.Guild.GetChannel(t.info.AnnouncementChannelID) as SocketTextChannel;
                        EmbedFooterBuilder footerBuilder = new EmbedFooterBuilder();
                        footerBuilder.Text = g.settings.textSettings.embed_footer;

                        _channel = Context.Guild.GetChannel(t.info.ManagementChannelID) as SocketTextChannel;
                        var picture = await _channel.SendFileAsync(@"bracket.png", "Bracket");

                        string imgurl = picture.Attachments.First().Url;
                        _channel = Context.Guild.GetChannel(t.info.AnnouncementChannelID) as SocketTextChannel;
                        EmbedBuilder BracketMessage = new EmbedBuilder();
                        BracketMessage.WithAuthor("RL Fridays");
                        BracketMessage.WithTitle("Bracket");
                        BracketMessage.Description = "Το bracket δημιουργήθηκε. Σύντομα θα ξεκινήσει ο πρώτος γύρος!";
                        BracketMessage.WithImageUrl(imgurl);
                        BracketMessage.WithThumbnailUrl(g.settings.textSettings.thumbnail_URL);
                        BracketMessage.WithColor(Color.Blue);
                        BracketMessage.Footer = footerBuilder;
                        var msg = await _channel.SendMessageAsync("", false, BracketMessage.Build());
                    }
                }
            }





        }

        

        
    }
}
