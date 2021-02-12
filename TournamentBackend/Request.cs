using System;
using System.Collections.Generic;
using System.Text;

namespace TournamentBackend
{
    public enum RequestType
    {
        GET_TOURNAMENT,
        GET_BRACKET,
        GET_PLAYER,
        GET_GUILD
    }

    [Serializable]
    public class Request
    {
        public RequestType type;
        public List<object> args = new List<object>();
    }
}
