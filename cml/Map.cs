using CustomMapLoader.Extended;
using Onsharp.Entities;
using Onsharp.Interop;

namespace CustomMapsLoader
{
    /// <summary>
    /// This class represents a map with all its information like the meta data.
    /// </summary>
    public class Map
    {
        /// <summary>
        /// The name of the map.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The author of the map.
        /// </summary>
        public string Author { get; set; } = "";
        
        /// <summary>
        /// The key of the map which identifies the map.
        /// </summary>
        public string Key { get; set; }
        
        /// <summary>
        /// A link to the preview image.
        /// </summary>
        public string Preview { get; set; }
        
        /// <summary>
        /// The spawn of the map.
        /// </summary>
        public MapInformation.MapSpawn Spawn { get; set; }

        /// <summary>
        /// Changes the current map for all players to this one.
        /// </summary>
        public void ChangeTo()
        {
            CML.Default.ChangeMap(this);
        }

        /// <summary>
        /// Changes the map for the given player to this one.
        /// </summary>
        /// <param name="player">The selected player</param>
        public void ChangeFor(Player player)
        {
            player.CallRemote("cml:load:" + Key);
        }

        /// <summary>
        /// Converts the map to a lua table.
        /// </summary>
        public LuaTable ToLua()
        {
            LuaTable mapTable = new LuaTable();
            mapTable.Add("key", Key);
            mapTable.Add("name", Name);
            mapTable.Add("author", Author);
            mapTable.Add("preview", Preview);
            return mapTable;
        }
    }
}