using System;
using System.Collections.Generic;
using CustomMapsLoader;
using Newtonsoft.Json;
using Onsharp.Commands;
using Onsharp.Entities;
using Onsharp.Events;
using Onsharp.Plugins;
using Onsharp.Utils;

namespace CustomMapsChanger
{
    [PluginMeta("cml-changer", "CMC", "1.0.0", "OnsharpTeam", "cml", PackageProvider = typeof(ChangerProvider))]
    public class CMC : Plugin
    {
        private static readonly Random Random = new Random();
        
        internal static CMC Instance { get; private set; }
        
        internal MapVote CurrentVote { get; set; }
        
        internal SelectionConfig Config { get; private set; }

        public override void OnStart()
        {
            Instance = this;
            Config = Data.Config<SelectionConfig>();
            Logger.SetDebug(Config.IsDebug);
        }

        public override void OnStop()
        {
        }

        [ConsoleCommand("mapvote", "Shows the Map Vote")]
        public void OnMapVoteCommand()
        {
            StartMapVote();
        }

        [RemoteEvent("cmc:client:vote")]
        public void OnPlayerMapVote(Player player, string map)
        {
            if(CurrentVote == null) return;
            if(!CurrentVote.Vote(player, map)) return;
            Server.Players.SafeForEach(target =>
            {
                target.CallRemote("cmc:server:vote", player.SteamID, map);
            });
        }

        public void StartMapVote(List<Map> maps = null)
        {
            Logger.Debug("Starting map vote! Check if current vote is active...");
            if(CurrentVote != null) return;
            Logger.Debug("No vote active! Check if maps are selected...");
            if (maps == null)
            {
                Logger.Debug("No maps are provided! Generating list...");
                maps = new List<Map>();
                for (int i = 0; i < Config.MaxMapsForVote; i++)
                {
                    int timeout = 10;
                    while (true)
                    {
                        Map map = GetRandomMap();
                        if (!maps.Contains(map))
                        {
                            maps.Add(map);
                            Logger.Debug("Map selected: {I}", map.Name);
                            break;
                        }

                        timeout--;
                        if(timeout <= 0)
                            break;
                    }
                }
            }
            
            Logger.Debug("Showing map vote...");
            CurrentVote = new MapVote(maps);
            List<object> choices = new List<object>();
            foreach(Map map in  maps)
                choices.Add(new
                {
                    t = map.Name, p = map.Preview
                });
            Server.Players.SafeForEach(player =>
            {
                player.CallRemote("cmc:server:show", Config.VoteSeconds, JsonConvert.SerializeObject(choices));
            });
            CurrentVote.StartTimeout();
            Logger.Debug("Map vote started!");
        }

        private Map GetRandomMap()
        {
            return CML.Default.Maps[Random.Next(CML.Default.Maps.Count)];
        }
    }
}