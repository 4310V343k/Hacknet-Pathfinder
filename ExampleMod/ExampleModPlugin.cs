﻿using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Hacknet;
using Microsoft.Xna.Framework;
using System.Xml;
using Hacknet.Mission;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Action;
using Pathfinder.Util;
using Pathfinder.Daemon;
using Pathfinder.Mission;

namespace ExampleMod2
{
    [BepInPlugin("com.Windows10CE.Example", "Example", "1.0.0")]
    public class ExampleModPlugin2 : BepInEx.Hacknet.HacknetPlugin
    {
        public override bool Load()
        {
            base.HarmonyInstance.PatchAll(typeof(PatchClass2));

            Pathfinder.Executable.ExecutableManager.RegisterExecutable<TestExe>("#PF_TEST_EXE#");
            Pathfinder.Port.PortManager.RegisterPort("Example port", 50);
            Pathfinder.Daemon.DaemonManager.RegisterDaemon<TestDaemon>();
            Pathfinder.Command.CommandManager.RegisterCommand("pathfinder", TestCommand);
            Pathfinder.Mission.GoalManager.RegisterGoal<TestGoal>("resetIP");
            Pathfinder.Action.ConditionManager.RegisterCondition<TestCondition>("OnDelete");
            Pathfinder.Action.ActionManager.RegisterAction<TestAction>("RandomFlag");

            return true;
        }

        public static void TestCommand(OS os, string[] args)
        {
            os.write("pathfinder is here!");
            os.write("Arguments passed in: " + string.Join(" ", args));
        }
    }

    public class TestExe : Pathfinder.Executable.BaseExecutable
    {
        public override string GetIdentifier() => "Some";

        public TestExe(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args) { this.ramCost = 761; }

        public override void LoadContent()
        {
            base.LoadContent();
            Programs.getComputer(os, targetIP).hostileActionTaken();
            os.write(string.Join(" ", Args));
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawTarget();
            drawOutline();
            Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X, Bounds.Center.Y), "blue text", new Color(255, 0, 0));
        }

        private float total = 0f;
        public override void Update(float t)
        {
            base.Update(t);
            
            total += t;
            if (total > 2.5f)
            {
                isExiting = true;
                Programs.getComputer(os, targetIP).openPort(50, os.thisComputer.ip);
            }
        }
    }

    public class TestDaemon : BaseDaemon
    {
        public TestDaemon(Computer computer, string serviceName, OS opSystem) : base(computer, serviceName, opSystem) { }

        public override string Identifier => "Test Daemon! :)";

        [XMLStorage]
        public string DisplayString;

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);

            var center = os.display.bounds.Center;
            Hacknet.Gui.TextItem.doLabel(new Vector2(center.X, center.Y), DisplayString, Color.Aquamarine);
        }
    }

    public class TestGoal : InitializableGoal
    {
        [XMLStorage]
        public string NodeID;

        public string OriginalIP;

        public override void Initialize()
        {
            OriginalIP = Programs.getComputer(OS.currentInstance, NodeID).ip;
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            return Programs.getComputer(OS.currentInstance, NodeID).ip != OriginalIP;
        }
    }

    public class TestCondition : PathfinderCondition
    {
        [XMLStorage]
        public string Computer;
        [XMLStorage] 
        public string Directory;
        [XMLStorage]
        public string File;

        private Computer Comp;
        
        public override bool Check(object os_obj)
        {
            var os = (OS)os_obj;

            if (Computer == null || Directory == null)
                throw new FormatException("TestCondition: Need a node ID and directory!");
            
            if (Comp == null)
                Comp = Programs.getComputer(os, Computer);

            var folder = Comp.getFolderFromPath(Directory);

            if (File == null)
                return folder.files.Count == 0;

            return folder.files.All(x => x.name != File);
        }
    }

    public class TestAction : DelayablePathfinderAction
    {
        [XMLStorage]
        public string Min;
        [XMLStorage]
        public string Max;

        private int min = 0;
        private int max = 9;
        
        private static readonly Random Rand = new Random();
        
        public override void Trigger(OS os)
        {
            os.Flags.Flags.RemoveAll(x => x.StartsWith("randomInt"));
            os.Flags.Flags.Add("randomInt" + Rand.Next(min, max));
        }

        public override void LoadFromXml(XmlReader reader)
        {
            base.LoadFromXml(reader);

            if (Min != null)
                min = int.Parse(Min);
            if (Max != null)
                max = int.Parse(Max);
        }
    }

    [HarmonyPatch]
    public static class PatchClass2
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Draw))]
        public static void MainMenuTextPatch()
        {
            GuiData.startDraw();
            Hacknet.Gui.Button.doButton(3473249, 5, 5, 30, 600, "bruh", Color.BlueViolet);
            GuiData.endDraw();
        }
    }
}
