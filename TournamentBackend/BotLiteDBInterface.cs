using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;
using System.Linq;
using Newtonsoft.Json;

namespace TournamentBackend
{
    public class BotLiteDBInterface
    {
        public Guild GetGuild(LiteDatabase db, ulong guild_id)
        {
            var collection = db.GetCollection<Guild>("guilds");
            Guild g = collection.FindOne(x => x.DiscordID == guild_id);

            //Get Tournaments of guild
            g.Tournaments = GetGuildTournaments(db, g.Id);

            if (g != null)
                return g;
            return null;
        }

        public List<TournamentInfo> GetGuildTournaments(LiteDatabase db, long guild_uid)
        {
            var collection = db.GetCollection<TournamentInfo>("tournaments");
            List<TournamentInfo> data = collection.Find(x => x.GuildID == guild_uid).ToList();
            return data;
        }

        public void UpdateGuild(LiteDatabase db, Guild g)
        {
            var collection = db.GetCollection<Guild>("guilds");
            collection.Update(g);
        }

        public MainPlayer GetMainPlayer(LiteDatabase db, ulong user_disc_id)
        {
            var collection = db.GetCollection<MainPlayer>("players");
            MainPlayer o = collection.FindOne(x => x.DiscordID == user_disc_id);

            if (o != null)
                return o;
            return null;
        }

        public void UpdateMainPlayer(LiteDatabase db, MainPlayer mp)
        {
            var collection = db.GetCollection<MainPlayer>("players");
            collection.Update(mp);
        }

        //Tournament Stuff

        public void getTournamentPLayers(LiteDatabase db, ref Tournament tourney)
        {
            var collection = db.GetCollection<Player>("tournament_players");

            long tourn_id = tourney.info.UID;
            tourney._players = collection.Find(x => x.TournamentID == tourn_id).ToList();

            //Get main Players
            var main_players = db.GetCollection<MainPlayer>("players");
            foreach (Player p in tourney._players)
            {
                p.mainPlayer = main_players.FindOne(x => x.ID == p.MainPlayerID);
            }
        }

        public void getTournamentTeams(LiteDatabase db, ref Tournament tourney)
        {
            var collection = db.GetCollection<Team>("tournament_teams");

            long tourn_id = tourney.info.UID;
            List<Team> teams = collection.Find(x => x.TournamentID == tourn_id).ToList();

            
            foreach (Team t in teams)
            {
                Team team = null;
                switch (tourney.info.Type)
                {
                    case TournamentType.SOLO:
                        {
                            team = new Team1s();
                            break;
                        }
                    case TournamentType.DOUBLES:
                        {
                            team = new Team2s();
                            break;
                        }
                    case TournamentType.TRIPLES:
                        {
                            team = new Team3s();
                            break;
                        }
                }

                team.Name = t.Name;
                team.CaptainID = t.CaptainID;
                team.Captain = tourney.getPlayerByUID(t.CaptainID);
                team.ID = tourney._teams.Count;
                team.UID = t.UID;
                tourney._teams.Add(team);
            }
        }


        public void getTournamentPlayerAssignments(LiteDatabase db, ref Tournament tourney)
        {
            var player_collection = db.GetCollection<TeamPlayerAssoc>("tournament_team_players");
            long tourn_id = tourney.info.UID;
            List<TeamPlayerAssoc> associations = player_collection.Find(x => x.TournamentID == tourn_id).ToList();

            foreach (TeamPlayerAssoc assoc in associations)
            {
                Player p = tourney.getPlayerByUID(assoc.PlayerID);
                Team t = tourney.getTeamByUID(assoc.TeamID);

                switch (tourney.info.Type)
                {
                    case TournamentType.SOLO:
                        {
                            ((Team1s)t).addPlayer(p);
                            break;
                        }
                    case TournamentType.DOUBLES:
                        {
                            ((Team2s)t).addPlayer(p);
                            break;
                        }
                    case TournamentType.TRIPLES:
                        {
                            ((Team3s)t).addPlayer(p);
                            break;
                        }
                }
            }
        }

        public Tournament GetTournament(LiteDatabase db, long tourn_id)
        {
            var collection = db.GetCollection<TournamentInfo>("tournaments");
            Tournament t = new Tournament();
            t.info = collection.FindOne(x => x.UID == tourn_id);
            
            //Get Players
            getTournamentPLayers(db, ref t);

            //Get Teams
            getTournamentTeams(db, ref t);

            //Get team-player assignments
            getTournamentPlayerAssignments(db, ref t);

            //Get Player invitations
            //getPlayerInvitations(conn, ref t, tourn_id);

            //Get bracket
            if (t.info.BracketGenerated)
                getTournamentBracket(db, ref t);

            if (t != null)
                return t;
            return null;
        }

        public void UpdateTournamentInfo(LiteDatabase db, TournamentInfo t)
        {
            var collection = db.GetCollection<TournamentInfo>("tournaments");
            collection.Update(t);
        }


        public void RemovePlayerFromTournament(LiteDatabase db, Player p)
        {
            var collection = db.GetCollection<Player>("tournament_players");
            collection.DeleteMany(x=>x.UID == p.UID);
        }

        public void RegisterPlayerToDB(LiteDatabase db, MainPlayer mp)
        {
            var collection = db.GetCollection<MainPlayer>("players");
            collection.Insert(mp);
            collection.EnsureIndex(x => x.ID);
            collection.EnsureIndex(x => x.DiscordID);
        }

        public void registerPlayerToTournament(LiteDatabase db, Player p)
        {
            var collection = db.GetCollection<Player>("tournament_players");
            collection.Insert(p);
            collection.EnsureIndex(p => p.TournamentID);
            collection.EnsureIndex(p => p.UID);
        }

        public long registerTournamentToDB(LiteDatabase db, TournamentInfo t)
        {
            var collection = db.GetCollection<TournamentInfo>("tournaments");
            long index = 0;
            try
            {
                index = collection.Insert(t);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
             
            collection.EnsureIndex(t => t.UID);
            
            return index;
        }

        public void registerTeamToTournament(LiteDatabase db, Team t)
        {
            var team_collection = db.GetCollection<Team>("tournament_teams");
            team_collection.Insert(t); //Register Team
            team_collection.EnsureIndex(x => x.UID);
            team_collection.EnsureIndex(x => x.TournamentID);
        }

        public void registerTournamentTeamPlayer(LiteDatabase db, Tournament tourn, Team team, Player p)
        {
            var player_collection = db.GetCollection<TeamPlayerAssoc>("tournament_team_players");
            TeamPlayerAssoc assoc = new TeamPlayerAssoc()
            {
                PlayerID = p.UID,
                TeamID = team.UID,
                TournamentID = tourn.info.UID
            };
            
            player_collection.Insert(assoc);
            player_collection.EnsureIndex(x => x.UID);
        }


        public void registerAllTournamentTeams(LiteDatabase db, Tournament t)
        {
            //Iterate in teams
            foreach (Team team in t._teams)
            {
                registerTeamToTournament(db, team);
                foreach (Player p in team.Players)
                {
                    registerTournamentTeamPlayer(db, t, team, p);
                }
            }
        }

        public void initTeam(LiteDatabase db, Tournament tourn, Team t, Player p)
        {
            registerTeamToTournament(db, t);
            registerTournamentTeamPlayer(db, tourn, t, p);
        }

        public long registerPlayerInvitation(LiteDatabase db, TeamInvitation inv)
        {
            var collections = db.GetCollection<TeamInvitation>("team_invitations");
            long id = collections.Insert(inv);
            collections.EnsureIndex(x => x.UID);
            return id;
        }

        public TeamInvitation GetPlayerInvitation(LiteDatabase db, ref Tournament t, long inv_id)
        {
            var collections = db.GetCollection<TeamInvitation>("team_invitations");
            
            TeamInvitation inv = collections.FindOne(x => x.UID == inv_id);
            
            if (inv != null)
            {
                inv.team = t.getTeamByUID(inv.teamID);
                inv.player = t.getPlayerByMainPlayerUID(inv.playerID);
            }

            return inv;
        }

        public void UpdatePlayerInvitation(LiteDatabase db, TeamInvitation inv)
        {
            var collections = db.GetCollection<TeamInvitation>("team_invitations");
            collections.Update(inv);
        }


        //Matchups
        public void UpdateMatchup(LiteDatabase db, Matchup m)
        {
            var collections = db.GetCollection<Matchup>("matchups");
            collections.Update(m);
        }

        public void ApplyTournamentMatchupChanges(LiteDatabase db, ref Tournament t)
        {
            for (int i = t.bracket.Rounds.Count - 1; i >= 0; i--)
            {
                Round r = t.bracket.Rounds[i];
                for (int j = 0; j < r.Matchups.Count; j++)
                {
                    Matchup m = r.Matchups[j];
                    UpdateMatchup(db, m);
                }
            }
        }

        //Bracket

        public void registerBracket(LiteDatabase db, ref Tournament t)
        {
            var collection = db.GetCollection<Matchup>("matchups");

            //t.bracket.MatchupTree.UID = collection.Insert(t.bracket.MatchupTree);
            for (int i = t.bracket.Rounds.Count - 1; i >= 0; i--)
            {
                Round r = t.bracket.Rounds[i];
                for (int j = 0; j < r.Matchups.Count; j++)
                {
                    Matchup m = r.Matchups[j];
                    collection.Insert(m);
                }
            }
            collection.EnsureIndex(x => x.UID);
        }

        public void getTournamentBracket(LiteDatabase db, ref Tournament tourney)
        {
            tourney.CreateBracket();
            
            var collection = db.GetCollection<Matchup>("matchups");

            long tourn_id = tourney.info.UID;
            List<Matchup> ms = collection.Find(Query.All("_id", Query.Descending), 0, 100).Where(x=>x.TournamentID == tourn_id).ToList();
            
            foreach (Matchup m in ms)
            {
                Matchup mat = tourney.bracket.Matchups[m.ID];
                //Override info
                mat.UID = m.UID;
                mat.TournamentID = m.TournamentID;
                mat.RoundID = m.RoundID;
                mat.LobbyName = m.LobbyName;
                mat.LobbyPass = m.LobbyPass;
                if (m.Team1ID > 0)
                    mat.Team1 = tourney.getTeamByUID(m.Team1ID);
                mat.Team1 = tourney.getTeamByUID(m.Team1ID);
                if (m.Team2ID > 0)
                    mat.Team2 = tourney.getTeamByUID(m.Team2ID);
                mat.Team2 = tourney.getTeamByUID(m.Team2ID);
                if (m.Team1ReportedWinnerID > 0)
                    mat.Team1ReportedWinner = tourney.getTeamByUID(m.Team1ReportedWinnerID);
                if (m.Team2ReportedWinnerID > 0)
                    mat.Team2ReportedWinner = tourney.getTeamByUID(m.Team2ReportedWinnerID);
                if (m.WinnerID > 0)
                    mat.Winner = tourney.getTeamByUID(m.WinnerID);
                if (m.NextMatchupID > 0)
                    mat.Next = tourney.getmatchupByUID(m.NextMatchupID);
                mat.InProgress = m.InProgress;
            }
        }


    }

    

}
