using System;
using System.IO;
using MySqlConnector;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace TournamentBackend
{
    public class BotSQLInterface
    {
        private string connection_string = "server=localhost;userid=greg;database=bot_server;password=210390";
        
        public BotSQLInterface()
        {
            
        }

        public MySqlConnection CreateConnection()
        {
            return new MySqlConnection(connection_string);
        }
        
        public void Connect(MySqlConnection conn)
        {
            conn.Open();
        }
        
        
        public void Disconnect(MySqlConnection conn)
        {
            //Initialize Connection
            conn.Close();
        }

        private MySqlCommand GenerateCommand(MySqlConnection conn, string q, int timeout=300)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = q;
            cmd.CommandTimeout = timeout;
            return cmd;
        }
        
        public MySqlDataReader ExecuteQuery(MySqlConnection conn, string q)
        {
            try
            {
                var cmd = GenerateCommand(conn, q);
                return cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        
        public void registerGuild(MySqlConnection conn, SocketGuild guild)
        {
            //Init default settings
            string jsonstring = File.ReadAllText("settings.json");
            string q = string.Format("INSERT INTO guilds (name, discord_id, settings) VALUES ('{0}', '{1}', '{2}')",
                guild.Name.Replace("\'","\'\'"), guild.Id, jsonstring.Replace("\'","\'\'"));
            MySqlDataReader rdr = ExecuteQuery(conn, q);
            rdr.Close();
        }

        public Settings getGuildSettings(MySqlConnection conn, ulong guild_id)
        {
            string q = string.Format("SELECT settings from guilds WHERE discord_id='{0}'", guild_id);

            MySqlDataReader rdr = ExecuteQuery(conn, q);

            if (!rdr.HasRows)
                return null;
            
            Settings settings = new Settings();
            string text = "";
            while (rdr.Read())
                text = rdr.GetString(rdr.GetOrdinal("settings"));
            rdr.Close();
            //Load Settings File
            
            return JsonConvert.DeserializeObject<Settings>(text);
        }
        
        public ulong registerTournament(MySqlConnection conn, int type_id, int method_id, ulong guild_id, ulong ann_id, 
            ulong reg_id, ulong mgmt_id, ulong score_id, ulong role_id, ulong cat_id)
        {
            string q = string.Format("INSERT INTO tournaments (type, date_created, guild_id, announcement_channel_id, registration_channel_id, management_channel_id, scorereport_channel_id, role_id, category_id, team_generation_method_id) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')",
                type_id, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), guild_id, ann_id, reg_id, mgmt_id, score_id, role_id, cat_id, method_id);
            
            var cmd = GenerateCommand(conn, q);
            cmd.ExecuteScalar();
            return (ulong) cmd.LastInsertedId;
        }

        public void ApplyTournamentInfoChanges(MySqlConnection conn, Tournament t)
        {
            string q = string.Format("UPDATE tournaments SET status_started = '{0}', status_in_progress = '{1}', status_finished = '{2}', registration_open = '{3}', bracket_generated = '{4}', round_num = '{5}' WHERE ID = '{6}'",
                t.info.IsStarted ? 1 : 0, 0, t.info.IsFinished ?1 : 0, t.info.RegistrationsEnabled ? 1 : 0, t.info.BracketGenerated ? 1 : 0, t.info.RoundNumber, t.info.UID);
            MySqlDataReader rdr = ExecuteQuery(conn, q);
            rdr?.Close();
        }

        public void ApplySingleMatchupUpdate(MySqlConnection conn, Matchup m, ref Tournament t)
        {
            //Update Matchup to DB
            string q = "UPDATE matchups SET status_in_progress = @status_in_progress, team1_id=@team1_id, team2_id=@team2_id, winner_id=@winner_id, team1_reported_winner_id=@team1_reported_winner_id, team2_reported_winner_id=@team2_reported_winner_id, lobby_name=@lobby_name, lobby_pass=@lobby_pass WHERE tournament_id = @tournament_id AND ID = @uid";
            var cmd = GenerateCommand(conn, q);
            cmd.Parameters.AddWithValue("@tournament_id", t.info.UID);
            cmd.Parameters.AddWithValue("@uid", m.UID);
            cmd.Parameters.AddWithValue("@status_in_progress", m.InProgress);
            cmd.Parameters.AddWithValue("@lobby_name", m.LobbyName);
            cmd.Parameters.AddWithValue("@lobby_pass", m.LobbyPass);
            
            //Team1
            if (m.Team1 != null)
            {
                if (m.Team1.IsDummy)
                    cmd.Parameters.AddWithValue("@team1_id", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@team1_id", m.Team1.UID);
            } else 
                cmd.Parameters.AddWithValue("@team1_id", DBNull.Value);
            
            //Team2
            if (m.Team2 != null)
            {
                if (m.Team2.IsDummy)
                    cmd.Parameters.AddWithValue("@team2_id", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@team2_id", m.Team2.UID);
            } else 
                cmd.Parameters.AddWithValue("@team2_id", DBNull.Value);
            
            //Team1 reported winner
            if (m.Team1ReportedWinner != null)
            {
                if (m.Team1ReportedWinner.IsDummy)
                    cmd.Parameters.AddWithValue("@team1_reported_winner_id", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@team1_reported_winner_id", m.Team1ReportedWinner.UID);
            } else 
                cmd.Parameters.AddWithValue("@team1_reported_winner_id", DBNull.Value);
            
            //Team2 reported winner
            if (m.Team2ReportedWinner != null)
            {
                if (m.Team2ReportedWinner.IsDummy)
                    cmd.Parameters.AddWithValue("@team2_reported_winner_id", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@team2_reported_winner_id", m.Team2ReportedWinner.UID);
            } else 
                cmd.Parameters.AddWithValue("@team2_reported_winner_id", DBNull.Value);
            
            //Winner
            if (m.Winner != null)
            {
                if (m.Team2.IsDummy)
                    cmd.Parameters.AddWithValue("@winner_id", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@winner_id", m.Winner.UID);
            } else 
                cmd.Parameters.AddWithValue("@winner_id", DBNull.Value);
            
            //Next Match
            if (m.Next!=null)
                cmd.Parameters.AddWithValue("@next_matchup_id", m.Next.UID);
            else
                cmd.Parameters.AddWithValue("@next_matchup_id", DBNull.Value);
            
            //Lobby Info
            
            MySqlDataReader rdr = cmd.ExecuteReader();
            rdr?.Close();
        }
        
        public void ApplyTournamentMatchupChanges(MySqlConnection conn, ref Tournament t)
        {
            for (int i = t.bracket.Rounds.Count - 1; i >= 0; i--)
            {
                Round r = t.bracket.Rounds[i];
                for (int j = 0;j < r.Matchups.Count; j++)
                {
                    Matchup m = r.Matchups[j];
                    ApplySingleMatchupUpdate(conn, m, ref t);
                }
            }
        }
        
        public void ApplySingleInvitationUpdate(MySqlConnection conn, TeamInvitation inv)
        {
            //Update Matchup to DB
            string q = "UPDATE team_invitations SET pending = @pending, accepted=@accepted WHERE ID = @uid";
            var cmd = GenerateCommand(conn, q);
            cmd.Parameters.AddWithValue("@uid", inv.UID);
            cmd.Parameters.AddWithValue("@pending", inv.pending);
            cmd.Parameters.AddWithValue("@accepted", inv.accepted);
            
            MySqlDataReader rdr = cmd.ExecuteReader();
            rdr?.Close();
        }

        public TournamentChannelManager GetChannelManager(MySqlConnection conn, ulong tourn_id)
        {
            string q = string.Format("SELECT * from tournaments WHERE ID='{0}'", tourn_id);
            
            MySqlDataReader rdr = ExecuteQuery(conn, q);

            if (!rdr.HasRows)
                return null;
            
            TournamentChannelManager mgr = new TournamentChannelManager();
            while (rdr.Read())
            {
                mgr.RoleID = rdr.GetUInt64(rdr.GetOrdinal("role_id")); 
                mgr.AnnouncementChannelID = rdr.GetUInt64(rdr.GetOrdinal("announcement_channel_id"));
                mgr.CategoryChannelID = rdr.GetUInt64(rdr.GetOrdinal("category_id"));
                mgr.RegistationChannelID = rdr.GetUInt64(rdr.GetOrdinal("registration_channel_id"));
                mgr.ManagementChannelID = rdr.GetUInt64(rdr.GetOrdinal("management_channel_id"));
                mgr.ScoreReportChannelID = rdr.GetUInt64(rdr.GetOrdinal("scorereport_channel_id"));
            }
            rdr.Close();
            return mgr;
        }

        public void ApplyChannelManagerChanges(MySqlConnection conn, TournamentChannelManager mgr, ulong guild_id, ulong tourn_id)
        {
            string q = string.Format("UPDATE tournaments SET announcement_channel_id = '{0}', registration_channel_id = '{1}', management_channel_id = '{2}', scorereport_channel_id = '{3}', role_id = '{4}' WHERE guild_id = '{5}' AND ID = '{6}'",
                mgr.AnnouncementChannelID, mgr.RegistationChannelID, mgr.ManagementChannelID, mgr.ScoreReportChannelID, mgr.RoleID, guild_id, tourn_id);
            MySqlDataReader rdr = ExecuteQuery(conn, q);
            rdr?.Close();
        }

        public bool guildExists(MySqlConnection conn, ulong id)
        {
            string q = string.Format("SELECT * FROM guilds WHERE discord_id='{0}'",
                id);

            var cmd = GenerateCommand(conn, q);

            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                return true;
            return false;
        }
        
        //Tournament
        public Tournament getTournament(MySqlConnection conn, long tourn_id)
        {
            Tournament t = new Tournament();

            string q = string.Format("SELECT * from tournaments WHERE ID='{0}'", tourn_id);

            MySqlDataReader rdr = ExecuteQuery(conn, q);

            if (!rdr.HasRows)
                return null;

            while (rdr.Read())
            {
                t.info.UID = rdr.GetInt64(rdr.GetOrdinal("ID"));
                t.info.TypeID = rdr.GetInt32(rdr.GetOrdinal("type"));
                t.info.IsStarted = rdr.GetBoolean(rdr.GetOrdinal("status_started"));
                t.info.IsFinished = rdr.GetBoolean(rdr.GetOrdinal("status_finished"));
                t.info.RegistrationsEnabled = rdr.GetBoolean(rdr.GetOrdinal("registration_open"));
                t.info.TeamGenMethodID = rdr.GetInt32(rdr.GetOrdinal("team_generation_method_id"));
                t.info.BracketGenerated = rdr.GetBoolean(rdr.GetOrdinal("bracket_generated"));
            }
            rdr.Close();
            
            //Get Players
            getTournamentPLayers(conn, ref t, tourn_id);
            
            //Get Teams
            getTournamentTeams(conn, ref t, tourn_id);
            
            //Get team-player assignments
            getTournamentPLayerAssignments(conn, ref t, tourn_id);
            
            //Get Player invitations
            getPlayerInvitations(conn, ref t, tourn_id);
            
            //Get bracket
            getTournamentBracket(conn, ref t, tourn_id);
            
            rdr.Close();
            return t;
        }

        public void setguildActiveTournament(MySqlConnection conn, ulong guild_disc_id, ulong tourn_id)
        {
            string q = string.Format("UPDATE guilds SET active_tournament_id ='{0}' WHERE discord_id='{1}'",
                tourn_id, guild_disc_id);
            MySqlDataReader rdr = ExecuteQuery(conn, q);
            rdr?.Close();
        }
        
        public ulong getGuildActiveTournnamentID(MySqlConnection conn, ulong guild_disc_id)
        {
            string q = string.Format("SELECT active_tournament_id FROM guilds WHERE discord_id='{0}'",
                guild_disc_id);
            
            MySqlDataReader rdr = ExecuteQuery(conn, q);

            ulong tourn_id = 0;
            while (rdr.Read())
                tourn_id = rdr.GetUInt64(rdr.GetOrdinal("active_tournament_id"));
            rdr.Close();
            return tourn_id;
        }
        
        public void resetguildActiveTournament(MySqlConnection conn, ulong guild_disc_id)
        {
            string q = string.Format("UPDATE guilds SET active_tournament_id = NULL WHERE discord_id='{0}'", guild_disc_id);
            MySqlDataReader rdr = ExecuteQuery(conn, q);
            rdr?.Close();
        }
        
        
        //Player Stuff
        public ulong getPlayerActiveTournnamentID(MySqlConnection conn, ulong user_disc_id)
        {
            string q = string.Format("SELECT active_tournament_id FROM players WHERE discord_id='{0}'",
                user_disc_id);
            
            MySqlDataReader rdr = ExecuteQuery(conn, q);
            ulong tourn_id = 0;
            while (rdr.Read())
                tourn_id = rdr.GetUInt64(rdr.GetOrdinal("active_tournament_id"));
            rdr.Close();
            return tourn_id;
        }
        
        
        
        public bool checkPlayerExistsForTournament(MySqlConnection conn, ulong user_disc_id, int tourn_id)
        {
            string q = string.Format("SELECT * FROM tournament_players WHERE tournament_id = '{0}' AND discord_id = '{1}'", tourn_id, user_disc_id);
            
            var cmd = GenerateCommand(conn, q);

            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                return true;
            return false;
        }
        
        public ulong registerPlayerToTournament(MySqlConnection conn, Player p, ulong tourn_id)
        {
            return registerPlayerToTournament(conn, p.mainPlayer.DiscordID, p.Name, (int) p.Rank._rank, tourn_id);
        }
        
        public ulong registerPlayerToTournament(MySqlConnection conn, ulong user_disc_id, string user_name, int rank_id, ulong tourn_id)
        {
            //First check if player is registered to the DB
            string q = string.Format("SELECT * FROM players WHERE discord_id = '{0}'",
                user_disc_id); 
            
            var cmd1 = GenerateCommand(conn, q);
            MySqlDataReader rdr1 = cmd1.ExecuteReader();

            ulong main_player_uid = 0;
            if (rdr1.HasRows)
            {
                while (rdr1.Read())
                    main_player_uid = rdr1.GetUInt64(rdr1.GetOrdinal("ID")); 
                rdr1.Close();
                
                //Set active tournament ID for player
                q = string.Format("UPDATE players SET active_tournament_id = '{0}' WHERE ID = '{1}'", tourn_id, main_player_uid);
                
                var cmd = GenerateCommand(conn, q);
                MySqlDataReader rdr = cmd.ExecuteReader();
                rdr?.Close();
            }
            else
            {
                //Register Player to DB
                q = string.Format("INSERT INTO players (discord_id, active_tournament_id) VALUES ('{0}', '{1}')",
                    user_disc_id, tourn_id);
                
                rdr1.Close();
                var cmd = GenerateCommand(conn, q);
                MySqlDataReader rdr = cmd.ExecuteReader();
                main_player_uid = (ulong) cmd.LastInsertedId;
                rdr?.Close();
            }
            
            q = string.Format("INSERT INTO tournament_players (main_player_id, name, rank_id, tournament_id) VALUES ('{0}', '{1}', '{2}', '{3}')",
                main_player_uid, user_name, rank_id, tourn_id);

            cmd1 = GenerateCommand(conn, q);
            cmd1.ExecuteReader();
            return (ulong) cmd1.LastInsertedId;
        }
        
        public void registerPlayerToTeam(MySqlConnection conn, Player p, Team t, ulong tourn_id)
        {
            //Register first player
            string q = string.Format("INSERT INTO tournament_team_players (player_id, team_id, tournament_id) VALUES ('{0}', '{1}', '{2}')",
                p.UID, t.UID, tourn_id);
            MySqlDataReader rdr = ExecuteQuery(conn, q);
            rdr?.Close();  
        }
        
        public ulong registerPlayerInvitation(MySqlConnection conn, ulong team_id, ulong player_id, ulong tourn_id)
        {
            
            //Register Player to DB
            string q = string.Format("INSERT INTO team_invitations (player_id, tournament_id, team_id) VALUES ('{0}', '{1}', '{2}')",
                player_id, tourn_id, team_id);
            
            var cmd = GenerateCommand(conn, q);
            cmd.ExecuteReader();
            return (ulong) cmd.LastInsertedId;
        }

        public void RemovePlayerFromTournament(MySqlConnection conn, Player p, ulong tourn_id)
        {
            //Remove team associations
            string q = string.Format("DELETE from tournament_team_players WHERE player_id = '{0}'", p.UID);
            MySqlDataReader rdr = ExecuteQuery(conn, q);
            rdr?.Close();

            if (p.team != null)
            {
                if (p.team.Captain == p)
                {
                    //Remove Team as well
                    RemoveTeamFromTournament(conn, p.team);
                }    
            }
            
            //Remove Player from tournament
            q = string.Format("DELETE from tournament_players WHERE ID = '{0}'", p.UID);
            rdr = ExecuteQuery(conn, q);
            rdr?.Close();
        } 
        
        public void RemoveTeamFromTournament(MySqlConnection conn, Team t)
        {
            //Remove team associations
            string q = string.Format("DELETE from tournament_teams WHERE ID = '{0}'", t.UID);
            MySqlDataReader rdr = ExecuteQuery(conn, q);
            rdr?.Close();
        }
        
        public void getTournamentPLayers(MySqlConnection conn, ref Tournament tourney, long tourn_id)
        {
            string q = string.Format("SELECT tournament_players.ID, players.discord_id, tournament_players.name, tournament_players.rank_id  FROM tournament_players INNER JOIN players ON tournament_players.main_player_id = players.ID WHERE tournament_id = '{0}'", tourn_id);
            
            MySqlDataReader rdr = ExecuteQuery(conn, q);


            while (rdr.Read())
            {
                long _uid =rdr.GetInt64(rdr.GetOrdinal("ID"));
                ulong _discid = rdr.GetUInt64(rdr.GetOrdinal("discord_id"));
                string _name = rdr.GetString(rdr.GetOrdinal("name"));
                int _rank = rdr.GetInt32(rdr.GetOrdinal("rank_id"));
                
                Player p = new Player(_name, _rank);
                p.ID = tourney._players.Count;
                p.UID = _uid;
                tourney._players.Add(p);
            }
            rdr.Close();
        }

        public void getPlayerInvitations(MySqlConnection conn, ref Tournament tourney, long tourn_id)
        {

            foreach (Player p in tourney._players)
            {
                //Fetch pending invitation for player
                string q = string.Format("SELECT * from team_invitations WHERE tournament_id = '{0}' AND player_id='{1}' AND pending=1", tourn_id, p.UID);
                MySqlDataReader rdr = ExecuteQuery(conn, q);
                
                while (rdr.Read())
                {
                    ulong _uid =rdr.GetUInt64(rdr.GetOrdinal("ID"));
                    long _teamid = rdr.GetInt64(rdr.GetOrdinal("team_id"));

                    TeamInvitation inv = new TeamInvitation();
                    inv.team = tourney.getTeamByUID(_teamid);
                    p.Invitations.Add(inv);
                }
                rdr.Close();
            }
        }
        
        public TeamInvitation getPlayerInvitation(MySqlConnection conn, Tournament t, ulong inv_id)
        {
            //Fetch invitation for player
            string q = string.Format("SELECT * from team_invitations WHERE ID='{0}'", inv_id);
            MySqlDataReader rdr = ExecuteQuery(conn, q);

            TeamInvitation inv = new TeamInvitation();
            while (rdr.Read())
            {
                long _uid = rdr.GetInt64(rdr.GetOrdinal("ID"));
                long _playerid = rdr.GetInt64(rdr.GetOrdinal("player_id"));
                bool _pending = rdr.GetBoolean(rdr.GetOrdinal("pending"));
                bool _accepted = rdr.GetBoolean(rdr.GetOrdinal("accepted"));
                long _teamid = rdr.GetInt64(rdr.GetOrdinal("team_id"));

                inv.UID = _uid;
                inv.team = t.getTeamByUID(_teamid);
                inv.player = t.getPlayerByUID(_playerid);
                inv.pending = _pending;
                inv.accepted = _accepted;
            }

            rdr.Close();

            return inv;
        }
        
        

        
        public void getTournamentTeams(MySqlConnection conn, ref Tournament tourney, long tourn_id)
        {
            string q = string.Format("SELECT * FROM tournament_teams WHERE tournament_id = '{0}'", tourn_id);
            
            MySqlDataReader rdr = ExecuteQuery(conn, q);

            while (rdr.Read())
            {
                long _uid =rdr.GetInt64(rdr.GetOrdinal("ID"));
                string _name = rdr.GetString(rdr.GetOrdinal("team_name"));
                long captain_id = rdr.GetInt64(rdr.GetOrdinal("captain"));

                Team team;
                switch (tourney.info.Type)
                {
                    case TournamentType.SOLO:
                    {
                        team = new Team1s();
                        team.Name = _name;
                        team.Captain = tourney.getPlayerByUID(captain_id);
                        team.ID = tourney._teams.Count;
                        team.UID = _uid;
                        tourney._teams.Add(team);
                        break;
                    }
                    case TournamentType.DOUBLES:
                    {
                        team = new Team2s();
                        team.Name = _name;
                        team.Captain = tourney.getPlayerByUID(captain_id);
                        team.ID = tourney._teams.Count;
                        team.UID = _uid;
                        tourney._teams.Add(team);
                        break;
                    }    
                    case TournamentType.TRIPLES:
                    {
                        team = new Team3s();
                        team.Name = _name;
                        team.Captain = tourney.getPlayerByID(captain_id);
                        team.ID = tourney._teams.Count;
                        team.UID = _uid;
                        tourney._teams.Add(team);
                        break;
                    }   
                }
            }
            rdr.Close();
        }
        
        public void getTournamentPLayerAssignments(MySqlConnection conn, ref Tournament tourney, long tourn_id)
        {
            string q = string.Format("SELECT * FROM tournament_team_players WHERE tournament_id = '{0}'", tourn_id);
            
            MySqlDataReader rdr = ExecuteQuery(conn, q);

            while (rdr.Read())
            {
                long _teamuid = rdr.GetInt64(rdr.GetOrdinal("team_id"));
                long _playeruid = rdr.GetInt64(rdr.GetOrdinal("player_id"));

                Player p = tourney.getPlayerByUID(_playeruid);
                Team t = tourney.getTeamByUID(_teamuid);

                switch (tourney.info.Type)
                {
                    case TournamentType.SOLO:
                    {
                        ((Team1s) t).addPlayer(p);
                        break;
                    }
                    case TournamentType.DOUBLES:
                    {
                        ((Team2s) t).addPlayer(p);
                        break;
                    }
                    case TournamentType.TRIPLES:
                    {
                        ((Team3s) t).addPlayer(p);
                        break;
                    }
                }
            }
            rdr.Close();
        }
        
        public void getTournamentBracket(MySqlConnection conn, ref Tournament tourney, long tourn_id)
        {
            string q = string.Format("SELECT * FROM matchups WHERE tournament_id = '{0}' ORDER BY ID DESC", tourn_id);
            
            MySqlDataReader rdr = ExecuteQuery(conn, q);

            //Populate Rounds
            tourney.CreateBracket();
            
            while (rdr.Read())
            {
                long _uid =rdr.GetInt64(rdr.GetOrdinal("ID"));
                int ordinal = rdr.GetOrdinal("team1_id");
                
                ordinal = rdr.GetOrdinal("team1_id");
                long _team1_id = rdr.IsDBNull(ordinal) ? 0 : rdr.GetInt64(ordinal);
                ordinal = rdr.GetOrdinal("team2_id");
                long _team2_id = rdr.IsDBNull(ordinal) ? 0 : rdr.GetInt64(ordinal);
                bool status_in_progress =rdr.GetBoolean(rdr.GetOrdinal("status_in_progress"));
                ordinal = rdr.GetOrdinal("next_matchup_id");
                long _next_matchup_id = rdr.IsDBNull(ordinal) ? 0 : rdr.GetInt64(ordinal);
                ordinal = rdr.GetOrdinal("winner_id");
                ulong _winner_id = rdr.IsDBNull(ordinal) ? 0 : rdr.GetUInt64(ordinal);
                int _round_Id = rdr.GetInt32(rdr.GetOrdinal("round_id"));
                ordinal = rdr.GetOrdinal("team1_reported_winner_id");
                ulong _team1_reported_winner_id = rdr.IsDBNull(ordinal) ? 0 : rdr.GetUInt64(ordinal);
                ordinal = rdr.GetOrdinal("team2_reported_winner_id");
                ulong _team2_reported_winner_id = rdr.IsDBNull(ordinal) ? 0 : rdr.GetUInt64(ordinal);
                string lobby_name = rdr.GetString(rdr.GetOrdinal("lobby_name"));
                string lobby_pass = rdr.GetString(rdr.GetOrdinal("lobby_pass"));

                Matchup m = new Matchup();
                m.UID = _uid;
                m.RoundID = _round_Id;
                m.LobbyName = lobby_name;
                m.LobbyPass = lobby_pass;
                
                if (_team1_id > 0)
                    m.Team1 = tourney.getTeamByUID(_team1_id);
                else
                    m.Team1 = null;
                if (_team2_id > 0)
                    m.Team2 = tourney.getTeamByUID(_team2_id);
                else
                    m.Team2 = null;
                
                //Reported Winners
                if (_team1_reported_winner_id > 0)
                    m.Team1ReportedWinner = tourney.getTeamByUID(_team1_id);
                else
                    m.Team1ReportedWinner = null;
                if (_team2_reported_winner_id > 0)
                    m.Team2ReportedWinner = tourney.getTeamByUID(_team2_id);
                else
                    m.Team2ReportedWinner = null;

                if (_winner_id > 0)
                    m.Winner = tourney.getTeamByUID(_team2_id);
                else
                    m.Winner = null;

                if (_next_matchup_id > 0)
                    m.Next = tourney.getmatchupByUID(_next_matchup_id);

                m.InProgress = status_in_progress; //Winner setting can mess up stuff
                tourney.bracket.Rounds[_round_Id].Matchups.Add(m);
            }
            rdr.Close();
        }
        
        //Tournament stuff
        public bool getTournamentRegistrationStatus(MySqlConnection conn, int tourn_id)
        {
            string q = string.Format("SELECT registration_open FROM tournaments WHERE ID='{1}'", tourn_id);
            
            MySqlDataReader rdr = ExecuteQuery(conn, q);

            bool status = false;
            while (rdr.Read())
                status = rdr.GetBoolean(rdr.GetOrdinal("registration_open"));
            rdr.Close();
            return status;
        }
        
        public void setTournamentRegistrationStatus(MySqlConnection conn, ulong guild_disc_id, ulong tourn_id, bool status)
        {
            string q = string.Format("UPDATE tournaments SET registration_open='{0}' WHERE guild_id='{1}' AND ID='{2}'",
                status? 1 : 0, guild_disc_id, tourn_id);
            MySqlDataReader rdr =ExecuteQuery(conn, q);
            rdr?.Close();
        }
        
        public void setTournamentBracketGeneratedStatus(MySqlConnection conn, ulong tourn_id, bool status)
        {
            string q = string.Format("UPDATE tournaments SET bracket_generated='{0}' AND ID='{1}'",
                status? 1 : 0, tourn_id);
            MySqlDataReader rdr = ExecuteQuery(conn, q);
            rdr?.Close();
        }
        
        public void setTournamentRoundNumber(MySqlConnection conn, ulong tourn_id, int num)
        {
            string q = string.Format("UPDATE tournaments SET round_num='{0}' AND ID='{1}'",
                num, tourn_id);
            MySqlDataReader rdr = ExecuteQuery(conn, q);
            rdr?.Close();
        }

    
        public void initTeam(MySqlConnection conn, Team t, Player p, ulong tourn_id)
        {
            //Register Team
            string q = string.Format("INSERT INTO tournament_teams (tournament_id, team_name, captain) VALUES ('{0}', '{1}', '{2}')",
                tourn_id, t.Name, t.Captain.UID);
            
            var cmd = GenerateCommand(conn, q);
            MySqlDataReader rdr = cmd.ExecuteReader();
            ulong team_uid =(ulong) cmd.LastInsertedId;
            rdr?.Close();
            
            //Register first player
            q = string.Format("INSERT INTO tournament_team_players (player_id, team_id, tournament_id) VALUES ('{0}', '{1}', '{2}')",
                p.UID, team_uid, tourn_id);
            rdr = ExecuteQuery(conn, q);
            rdr?.Close();  
        }
        
        public void registerTeam(MySqlConnection conn, Team t, ulong tourn_id)
        {
            //Register Team
            string q = string.Format("INSERT INTO tournament_teams (tournament_id, team_name, captain) VALUES ('{0}', '{1}', '{2}')",
                tourn_id, t.Name, t.Captain.UID);
            MySqlDataReader rdr = ExecuteQuery(conn, q);
            rdr?.Close();
        }

        public void registerAllTournamentTeams(MySqlConnection conn, Tournament t, ulong tourn_id)
        {
            //Iterate in teams
            foreach (Team team in t._teams)
            {
                //Register Team
                string q = string.Format("INSERT INTO tournament_teams (tournament_id, team_name, captain) VALUES ('{0}', '{1}', '{2}')",
                    tourn_id, team.Name, team.Captain.UID);
                
                var cmd = GenerateCommand(conn, q);
                MySqlDataReader rdr = cmd.ExecuteReader();
                ulong team_uid =(ulong) cmd.LastInsertedId;
                rdr?.Close();
                
                foreach (Player p in team.Players)
                {
                    //Register Players to Team
                    q = string.Format("INSERT INTO tournament_team_players (player_id, team_id, tournament_id) VALUES ('{0}', '{1}', '{2}')",
                        p.UID, team_uid, tourn_id);
                    rdr = ExecuteQuery(conn, q);
                    rdr?.Close();
                }
            }
        }
        
        
        //Bracket
        public void registerBracket(MySqlConnection conn, Tournament t, ulong tourn_id)
        {
            setTournamentBracketGeneratedStatus(conn, tourn_id, true);
            setTournamentRoundNumber(conn, tourn_id, t.bracket.Rounds.Count);
            
            for (int i = t.bracket.Rounds.Count - 1; i >= 0; i--)
            {
                Round r = t.bracket.Rounds[i];
                for (int j = 0;j < r.Matchups.Count; j++)
                {
                    Matchup m = r.Matchups[j];
                    //Register Matchup to DB
                    string q = "INSERT INTO matchups (tournament_id, team1_id, team2_id, next_matchup_id, round_id, team1_reported_winner_id, team2_reported_winner_id) VALUES (@tournament_id, @team1_id, @team2_id, @next_matchup_id, @round_id, @team1_reported_winner_id, @team2_reported_winner_id)";
                    var cmd = GenerateCommand(conn, q);
                    cmd.Parameters.AddWithValue("@tournament_id", tourn_id);
                    cmd.Parameters.AddWithValue("@round_id", i);

                    //Team1
                    if (m.Team1 != null)
                    {
                        if (m.Team1.IsDummy)
                            cmd.Parameters.AddWithValue("@team1_id", DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@team1_id", m.Team1.UID);
                    } else 
                        cmd.Parameters.AddWithValue("@team1_id", DBNull.Value);
                    
                    //Team2
                    if (m.Team2 != null)
                    {
                        if (m.Team2.IsDummy)
                            cmd.Parameters.AddWithValue("@team2_id", DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@team2_id", m.Team2.UID);
                    } else 
                        cmd.Parameters.AddWithValue("@team2_id", DBNull.Value);
                    
                    
                    //Team1 reported winner
                    if (m.Team1ReportedWinner != null)
                    {
                        if (m.Team1ReportedWinner.IsDummy)
                            cmd.Parameters.AddWithValue("@team1_reported_winner_id", DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@team1_reported_winner_id", m.Team1ReportedWinner.UID);
                    } else 
                        cmd.Parameters.AddWithValue("@team1_reported_winner_id", DBNull.Value);
                    
                    //Team2 reported winner
                    if (m.Team2ReportedWinner != null)
                    {
                        if (m.Team2ReportedWinner.IsDummy)
                            cmd.Parameters.AddWithValue("@team2_reported_winner_id", DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@team2_reported_winner_id", m.Team2ReportedWinner.UID);
                    } else 
                        cmd.Parameters.AddWithValue("@team2_reported_winner_id", DBNull.Value);
                    
                    //Next Match
                    if (m.Next!=null)
                        cmd.Parameters.AddWithValue("@next_matchup_id", m.Next.UID);
                    else
                        cmd.Parameters.AddWithValue("@next_matchup_id", DBNull.Value);
                    
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    long matchup_uid = cmd.LastInsertedId;
                    m.UID = matchup_uid;
                    rdr?.Close();
                }
            }
            
        }
    }
}