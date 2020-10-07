using CustomMapsChanger.Properties;
using Onsharp.Native;

namespace CustomMapsChanger
{
    public class ChangerProvider : PackageProvider
    {
        public ChangerProvider() : base("cmc")
        {
        }

        protected override void Initialize()
        {
            AddClientScript("selector.lua", Resources.selector1);
            AddAsset("selector.html", Resources.selector);
            AddAsset("jquery.js", Resources.jquery);
        }
    }
}