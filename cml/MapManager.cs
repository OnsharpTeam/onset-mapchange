using System;
using System.Collections.Generic;
using System.IO;
using CustomMapLoader.Extended;
using Newtonsoft.Json;
using Onsharp.IO;
using Onsharp.Utils;

namespace CustomMapsLoader
{
    /// <summary>
    /// The map loader creates, manages and loads all maps.
    /// </summary>
    public class MapManager
    {
        /// <summary>
        /// All loaded maps in this manager instance.
        /// </summary>
        public List<Map> Maps { get; }
        
        /// <summary>
        /// The current running map on the server.
        /// </summary>
        public Map CurrentMap { get; private set; }
        
        private readonly string _mapsFolder;
        private readonly string _packagesFolder;
        private readonly ILogger _logger;

        internal MapManager(ILogger logger, string mapsFolder, string packagesFolder)
        {
            Maps = new List<Map>();
            _logger = logger;
            _packagesFolder = packagesFolder;
            _mapsFolder = mapsFolder;
            Directory.CreateDirectory(_mapsFolder);
            if (CML.Instance.Config.AutoCreateOnStartup)
            {
                foreach (string path in Directory.GetFiles(_mapsFolder))
                {
                    if (Path.GetExtension(path).ToLower() == ".omap" || Path.GetExtension(path).ToLower() == ".pak")
                    {
                        CreateMap(Path.GetFileNameWithoutExtension(path));
                    }
                }
            }
            
            foreach (string path in Directory.GetDirectories(_packagesFolder))
            {
                LoadMap(path, !CML.Instance.Config.IsDebug);
            }
        }

        public void ChangeMap(Map map)
        {
            if(CurrentMap != null)
                CML.Instance.Runtime.StopPackage(map.Key);
            CurrentMap = map;
            CML.Instance.Runtime.StartPackage(map.Key);
        }
        
        public Map CreateMap(string key, bool register = true)
        {
            try
            {
                string folder = Path.Combine(_packagesFolder, key);
                if (Directory.Exists(folder))
                {
                    _logger.Fatal("A folder named by the map key {KEY} is already existing!", key);
                    return null;
                }

                Directory.CreateDirectory(folder);
                MapInformation info;
                
                string omapFile = Path.Combine(_mapsFolder, key + ".omap");
                if (File.Exists(omapFile))
                {
                    info = MapExtension.Extract(omapFile, Path.Combine(folder, key + ".pak"));
                    File.Delete(omapFile);
                }
                else
                {
                    string pakFile = Path.Combine(_mapsFolder, key + ".pak");
                    if (!File.Exists(pakFile))
                    {
                        _logger.Fatal("No PAK file found for the map {KEY}!", key);
                        return null;
                    }
            
                    string metaFile = Path.Combine(_mapsFolder, key + ".json");
                    if (!File.Exists(metaFile))
                    {
                        _logger.Fatal("No JSON file found for the map {KEY}!", key);
                        return null;
                    }

                    info = JsonConvert.DeserializeObject<MapInformation>(File.ReadAllText(metaFile));
                    File.Move(pakFile, Path.Combine(folder, key + ".pak"), true);
                    File.Delete(metaFile);
                }
                
                Map map = info.ToMap(key);
                MapExtension.StoreMapMeta(folder, key, map);
                File.WriteAllText(Path.Combine(folder, "client.lua"), GenerateScript(map, info));
                File.WriteAllText(Path.Combine(folder, "package.json"), GeneratePackage(map.Author, key));
                if (register) Maps.Add(map);
                return map;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while create a map with key {KEY}!", key);
                return null;
            }
        }

        public bool IsMap(string key)
        {
            return GetMap(key) != null;
        }

        public Map GetMap(string key)
        {
            return Maps.SelectFirst(map => string.Equals(map.Key, key, StringComparison.CurrentCultureIgnoreCase));
        }

        private string GeneratePackage(string author, string key)
        {
            return JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                {"author", author}, {"version", "cml-gen-" + CML.Instance.Meta.Version},
                {"files", new List<string> {key + ".pak"}},
                {"client_scripts", new List<string> {"client.lua"}},
                {"server_scripts", new List<string>()}
            });
        }
        
        private string GenerateScript(Map map, MapInformation info)
        {
            return
                @"
                local function LoadThisMap()
                    if GetWorld():GetMapName() ~= ""%map-name%"" then
                        LoadPak(""%pak-name%"", ""%pak-root%"", ""%pak-content%"")
                        ConnectToServer(GetServerIP(), GetServerPort(), """", ""%pak-map%"")
                    end
                    CallRemoteEvent(""cml:load:finish"")
                end
                
                AddRemoteEvent(""cml:load:%map-key%"", function ()
                    LoadThisMap()
                end)

                AddEvent(""OnPackageStart"", function () 
                    LoadThisMap()
                end)
                ".Replace("%map-name%", map.Name).Replace("%pak-name%", info.Pak.Name)
                    .Replace("%pak-root%", info.Pak.RootPath)
                    .Replace("%pak-content%", info.Pak.ContentPath)
                    .Replace("%pak-map%", info.Pak.Map)
                    .Replace("%map-key%", map.Key);
        }
        
        private void LoadMap(string path, bool silently)
        {
            try
            {
                Map map = MapExtension.LoadMapMeta<Map>(path, new DirectoryInfo(path).Name);
                if (map == default)
                {
                    if(!silently)
                        _logger.Fatal("The map folder contains no meta and is therefore no map!");
                    return;
                }
                
                Map selectMap = GetMap(map.Key);
                if (selectMap != null)
                {
                    _logger.Warn("A map with the key {KEY} is already loaded!", map.Key);
                    return;
                }

                Maps.Add(map);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "An error occurred while loading map on path {PATH}!", path);
            }
        }
    }
}