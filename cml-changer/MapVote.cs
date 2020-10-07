using System.Collections.Generic;
using CustomMapsLoader;
using Onsharp.Entities;
using Onsharp.Threading;
using Onsharp.Utils;

namespace CustomMapsChanger
{
    internal class MapVote
    {
        private readonly List<Map> _maps;
        private readonly Dictionary<Map, int> _votes;
        private readonly Dictionary<long, Map> _playersVote;
        private bool _isOpen = true;

        internal MapVote(List<Map> maps)
        {
            _maps = maps;
            _votes = new Dictionary<Map, int>();
            foreach(Map map in maps)
                _votes.Add(map, 0);
            _playersVote = new Dictionary<long, Map>();
        }

        internal bool Vote(Player player, string mapName)
        {
            if (!_isOpen) return false;
            Map map = GetMap(mapName);
            if (map == null) return false;
            long id = player.SteamID;
            if (_playersVote.ContainsKey(id))
            {
                _votes[_playersVote[id]]++;
                _playersVote.Remove(id);
            }

            _votes[map]++;
            _playersVote.Add(id, map);
            return true;
        }

        internal void StartTimeout()
        {
            Timer.Delay(CMC.Instance.Config.VoteSeconds * 1000, () =>
            {
                _isOpen = true;
                Map winner = null;
                int winVotes = int.MinValue;
                foreach (Map map in _votes.Keys)
                {
                    int votes = _votes[map];
                    if (votes > winVotes)
                    {
                        winVotes = votes;
                        winner = map;
                    }
                }

                if (winner != null)
                {
                    CMC.Instance.Server.Players.SafeForEach(player =>
                    {
                        player.CallRemote("cmc:server:finish", winner.Name);
                    });
                    winner.ChangeTo();
                }
            });
        }

        private Map GetMap(string key)
        {
            return _maps.SelectFirst(map => map.Name == key);
        }
    }
}