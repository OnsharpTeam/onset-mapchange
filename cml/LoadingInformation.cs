﻿using CustomMapLoader.Extended;

namespace CustomMapsLoader
{
    internal static class LoadingInformation
    {
        internal static Map ToMap(this MapInformation info, string key)
        {
            string[] parts = info.Pak.Map.Split('/');
            return new Map
            {
                Author = info.Author, Preview = info.Preview,
                Spawn = info.Spawn, Name = parts[^1], Key = key
            };
        }
    }
}