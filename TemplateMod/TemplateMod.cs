using System;
using System.IO;
using Hacknet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Attribute;
using Pathfinder.Event;
using Pathfinder.ModManager;
using Pathfinder.Util;
using Command = Pathfinder.Command;
using Daemon = Pathfinder.Daemon;
using Executable = Pathfinder.Executable;
using Extension = Pathfinder.Extension;
using Port = Pathfinder.Port;

namespace TemplateMod
{
    public class TemplateMod : IMod
    {
        internal static Port.Type p = new Port.Type("TemplateName", 4);

        public string Identifier => "Template Mod";

        [IgnoreRegister]
        [Event(Priority = 2)]
        public static void PriorityTwo(OSLoadContentEvent e)
        {
            Logger.Info("I should run first");
        }

        [IgnoreRegister]
        [Event(Priority = 1)]
        public static void PriorityOne(OSLoadContentEvent e)
        {
            Logger.Info("I should run second");
        }

        public static void CommandListener(CommandSentEvent e)
        {
            Logger.Info("command {0}", string.Join(" ", e.Arguments));
        }

        public void Load()
        {
            Logger.Verbose("Loading Template Mod");
            EventManager.RegisterListener<OSLoadContentEvent>(PriorityOne);
            EventManager.RegisterListener<OSLoadContentEvent>(PriorityTwo);
            EventManager.RegisterListener<CommandSentEvent>(CommandListener);
            Logger.Info("Loading finished");
        }

        public void LoadContent()
        {
            Executable.Handler.RegisterExecutable("TempExe", new TempExe());
            if (Port.Handler.RegisterPort("tempPort", p) != null)
                Logger.Info("added tempPort to game");
            Logger.Info("full id {0}", Daemon.Handler.RegisterDaemon("tempdae", new TempDaemon()));
            Logger.Info("full id {0}", Extension.Handler.RegisterExtension("tempext", new TempExtInfo()));
        }

        public void Unload()
        {
            Logger.Info("Unloading Template Mod");
        }

        class TempExe : Executable.Base
        {
            public override string Identifier => "TempExe";

            public override bool? Update(Executable.Instance instance, float time)
            {
                Logger.Verbose("TempExe");
                Logger.Info("Template Exe updating");
                return true;
            }

            public override void OnComplete(Executable.Instance instance)
            {
                Logger.Info("Template exe finished");
                base.OnKilled(instance);
            }
        }

        class TempDaemon : Daemon.Base
        {
            public override string InitialServiceName => "TempDae";

            public override void Draw(Daemon.Instance instance, Rectangle bounds, SpriteBatch sb) =>
                sb.Draw(Utils.white, bounds, Color.Green);

            public override void OnNavigatedTo(Daemon.Instance instance) => Logger.Info("was navigated to");

            public override void OnUserAdded(Daemon.Instance instance, string name, string pass, byte type) =>
                Logger.Info("user added {0} {1} {2}", name, pass, type);
        }

        class TempExtInfo : Extension.Info
        {
            public override bool AllowSaves => true;
            public override string Description => "A temporary extension";
            public override string LogoPath => null;
            public override string Name => "Temp Extension";

            public override void OnConstruct(OS os)
            {
                var derp = new Computer("Derpy", "101.101.101.10", new Vector2(10, 10), 0);
                os.netMap.nodes.Add(derp);
                //os.thisComputer.AddLink(derp);
            }

            public override void OnLoad(OS os, Stream loadingStream) {}
        }
    }
}
