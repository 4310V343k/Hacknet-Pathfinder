using System;
using Hacknet.Extensions;
using Pathfinder.Util.XML;

namespace Pathfinder.Extension
{
    public class PathfinderExtensionInfo : ExtensionInfo
    {
        public bool PathfinderExt { get; protected set; } = false;

        public static PathfinderExtensionInfo CloneHacknetInfo(ExtensionInfo info)
        {
            PathfinderExtensionInfo result = new PathfinderExtensionInfo();
            return Util.Utility.ObjectMap(result, info);
        }

        public void ParseInfoFromFolderPath(string folderpath)
        {
            var executor = new EventExecutor(folderpath + "/ExtensionInfo.xml", ignoreCase: true);
            executor.AddExecutor("Pathfinder", (exec, info) =>
            {
                this.PathfinderExt = true;
            });
            executor.Parse();
        }
    }
}