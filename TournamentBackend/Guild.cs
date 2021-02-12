using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;
using Newtonsoft.Json;

namespace TournamentBackend
{
    [Serializable]
    public class Guild
    {   
        [BsonId]
        public long Id { get; set; }

        [BsonField]
        public ulong DiscordID { get; set; }

        [BsonField]
        public string Name { get; set; }

        [BsonField]
        public long ActiveTournamentID { get; set; }

        [BsonIgnore]
        public List<TournamentInfo> Tournaments { get; set; }
        
        [BsonField]
        public string SettingsJson {

            get
            {
                if (settings == null)
                    return "";
                else
                    return JsonConvert.SerializeObject(settings);
            }
            set
            {
                settings = JsonConvert.DeserializeObject<Settings>(value);
            }
            
        }
        
        public Settings settings = null;
    }
}
