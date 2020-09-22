using System.IO;
using Onsharp.Commands;
using Onsharp.Entities;
using Onsharp.Events;
using Onsharp.Interop;
using Onsharp.Plugins;

namespace CustomMapsLoader
{
    [PluginMeta("cml", "CML", "1.0.0", "OnsharpTeam")]
    public class CML : Plugin
    {
        /// <summary>
        /// The default map loader instance created and managed by CML itself.
        /// </summary>
        public static MapManager Default { get; private set; }
        
        internal static CML Instance { get; private set; }
        
        public override void OnStart()
        {
            Instance = this;
            string serverDirectory = Data.Directory.Parent?.Parent?.Parent?.FullName ?? "";
            Default = new MapManager(Logger, Path.Combine(serverDirectory, "maps"), Path.Combine(serverDirectory, "packages"));
        }

        public override void OnStop()
        {
            
        }

        [RemoteEvent("cml:load:finish")]
        public void OnCmlLoadFinish(Player player)
        {
            if (Default.CurrentMap?.Spawn == null) return;
            player.SetDimension(Default.CurrentMap.Spawn.Dimension);
            player.SetPosition(Default.CurrentMap.Spawn.X, Default.CurrentMap.Spawn.Y, Default.CurrentMap.Spawn.Z);
            player.Heading = Default.CurrentMap.Spawn.Yaw;
        }

        [ServerEvent(EventType.PlayerJoin)]
        public void OnPlayerJoin(Player player)
        {
            Default.CurrentMap?.ChangeFor(player);
        }
        
        [ConsoleCommand("changemap", "Changes the map to the given <key>")]
        public void OnChangeMapCommand([Describe("The key of the map")] string key)
        {
            Map map = Default.GetMap(key) ?? Default.CreateMap(key);
            if (map == null)
            {
                Logger.Fatal("No map with the given key found!");
                return;
            }
            
            map.ChangeTo();
        }

        [LuaExport("getmaps")]
        public LuaTable LuaGetMaps()
        {
            LuaTable table = new LuaTable();
            int i = 1;
            foreach (Map map in Default.Maps)
            {
                table.Add(i, map.ToLua());
                i++;
            }

            return table;
        }

        [LuaExport("getmap")]
        public LuaTable LuaGetMap(string key)
        {
            if (!Default.IsMap(key)) return null;
            return Default.GetMap(key).ToLua();
        }

        [LuaExport("getcurrentmap")]
        public LuaTable LuaGetCurrentMap()
        {
            return Default.CurrentMap?.ToLua();
        }
    }
}