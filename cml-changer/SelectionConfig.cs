using Onsharp.IO;

namespace CustomMapsChanger
{
    [Config("config")]
    public class SelectionConfig
    {
        public bool IsDebug { get; set; } = false;
        
        public int VoteSeconds { get; set; } = 20;
        
        public int MaxMapsForVote { get; set; } = 3;
    }
}