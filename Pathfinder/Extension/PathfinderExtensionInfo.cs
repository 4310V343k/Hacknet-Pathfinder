using System;
using System.IO;
using Hacknet;
using Hacknet.Extensions;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.ModManager;
using Pathfinder.Util;
using Pathfinder.Util.XML;

namespace Pathfinder.Extension
{
    public class PathfinderExtensionInfo : ExtensionInfo
    {
        private static readonly ModTaggedDict<string, ReadExecution> ExtensionInfoExecutors = new ModTaggedDict<string, ReadExecution>();

        public static void AddExecutor(string name, ReadExecution loader, bool overrideable = false)
        {
            if (overrideable) ExtensionInfoExecutors.AddOverrideable(name, loader);
            else ExtensionInfoExecutors.Add(name, loader);
        }

        public static bool RemoveExecutor(string name)
            => ExtensionInfoExecutors.Remove(name);

        public bool PathfinderExt { get; protected set; } = false;
        public string[] DependentsPaths { get; protected set; } = new string[0];

        public PathfinderExtensionInfo() { }
        public PathfinderExtensionInfo(string folderPath)
        {
            ParseExtensionInfo(folderPath);
        }

        public void ParseExtensionInfo(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                throw new FileNotFoundException($"Folder '{folderPath}' not found.");
            folderPath += Path.DirectorySeparatorChar;
            var extPath = folderPath + "ExtensionInfo.xml";
            if (!File.Exists(extPath))
                throw new FileNotFoundException($"Extension Info '{extPath}' not found.");
            var executor = new EventExecutor(extPath, ignoreCase: true);
            FolderPath = folderPath;
            Language = "en-us";

            executor.AddExecutor("Language", (exec, info) => Language = info.Value ?? Language, true);
            executor.AddExecutor("Name", (exec, info) => Name = info.Value, true);
            executor.AddExecutor("AllowSaves", (exec, info) => AllowSave = info.Value.ToBool(true), true);
            executor.AddExecutor("StartingVisibleNodes", (exec, info) =>
            {
                if (info.Value.IndexOfAny(new[] { ' ', '\t', '\r', '/' }) != -1)
                    Logger.Warn("Nonstandard StartingVisibleNodes seperator found");
                StartingVisibleNodes = info.Value.Split(new[] { ',', ' ', '\t', '\r', '\n', '/' }, StringSplitOptions.RemoveEmptyEntries);
            }, true);
            executor.AddExecutor("StartingMission", (exec, info) =>
                StartingMissionPath = info.Value.ToLower() == "none" ? null : info.Value, true);
            executor.AddExecutor("StartingActions", (exec, info) =>
                StartingActionsPath = info.Value.ToLower() == "none" ? null : info.Value, true);
            executor.AddExecutor("Description", (exec, info) =>
                Description = Utils.CleanFilterStringToRenderable(info.Value), true);
            executor.AddExecutor("Faction", (exec, info) => FactionDescriptorPaths.Add(info.Value), true);
            executor.AddExecutor("StartsWithTutorial", (exec, info) => StartsWithTutorial = info.Value.ToBool(), true);
            executor.AddExecutor("HasIntroStartup", (exec, info) => HasIntroStartup = info.Value.ToBool(), true);
            executor.AddExecutor("StartingTheme", (exec, info) => Theme = info.Value.ToLower(), true);
            executor.AddExecutor("IntroStartupSong", (exec, info) => IntroStartupSong = info.Value, true);
            executor.AddExecutor("IntroStartupSongDelay", (exec, info) => IntroStartupSongDelay = info.Value.ToFloat(), true);
            executor.AddExecutor("SequencerSpinUpTime", (exec, info) => SequencerSpinUpTime = info.Value.ToFloat(), true);
            executor.AddExecutor("ActionsToRunOnSequencerStart", (exec, info) => ActionsToRunOnSequencerStart = info.Value, true);
            executor.AddExecutor("SequencerFlagRequiredForStart", (exec, info) => SequencerFlagRequiredForStart = info.Value, true);
            executor.AddExecutor("SequencerTargetID", (exec, info) => SequencerTargetID = info.Value, true);
            executor.AddExecutor("WorkshopDescription", (exec, info) => WorkshopDescription = info.Value, true);
            executor.AddExecutor("WorkshopVisibility", (exec, info) => WorkshopVisibility = (byte)info.Value.ToInt(), true);
            executor.AddExecutor("WorkshopTags", (exec, info) => WorkshopTags = info.Value, true);
            executor.AddExecutor("WorkshopPreviewImagePath", (exec, info) => WorkshopPreviewImagePath = info.Value, true);
            executor.AddExecutor("WorkshopLanguage", (exec, info) => WorkshopLanguage = info.Value, true);
            executor.AddExecutor("WorkshopPublishID", (exec, info) => WorkshopPublishID = info.Value, true);
            executor.AddExecutor("Logo", (exec, info) => TryLoadLogoImage(info.Value, true), true);
            executor.AddExecutor("Pathfinder", (exec, info) => PathfinderExt = true, true);
            executor.AddExecutor("Pathfinder.Dependents", (exec, info) => DependentsPaths = info.Value.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries), true);
            foreach (var pair in ExtensionInfoExecutors)
                executor.AddExecutor(pair.Key, pair.Value, true);
            executor.Parse();
            if (LogoImage == null)
                if (!TryLoadLogoImage(folderPath + "Logo"))
                    throw new FileNotFoundException($"{nameof(LogoImage)} not found for {folderPath.RemoveExtended(-1)}");
        }

        public bool TryLoadLogoImage(string path, bool hasExt = false)
        {
            LogoImage = null;
            if (!hasExt)
                foreach (var ext in new[] { ".png", ".jpg", ".bmp", ".gif", ".tif" })
                    if (File.Exists(path + ext))
                    {
                        path += ext;
                        break;
                    }
            using (var fs = File.OpenRead(path))
                LogoImage = Texture2D.FromStream(Game1.getSingleton().GraphicsDevice, fs);
            return LogoImage != null;
        }
    }
}