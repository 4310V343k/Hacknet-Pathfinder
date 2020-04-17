using Hacknet.Extensions;

namespace Pathfinder.Extension
{
    public class PathfinderExtensionInfo : ExtensionInfo
    {
        public static PathfinderExtensionInfo CloneHacknetInfo(ExtensionInfo info)
        {
            PathfinderExtensionInfo result = new PathfinderExtensionInfo();
            return Util.Utility.ObjectMap(result, info);
        }
    }
}