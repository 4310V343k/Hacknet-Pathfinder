﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Hacknet;
using Hacknet.Mission;
using HarmonyLib;
using Pathfinder.Event;
using Pathfinder.Event.Loading.Content;

namespace Pathfinder.Action
{
    [HarmonyPatch]
    public static class ConditionManager
    {
        private static readonly Dictionary<string, Type> CustomConditions = new Dictionary<string, Type>();

        static ConditionManager()
        {
            EventManager<GetAdditionalConditionsEvent>.AddHandler(OnGetAdditionalConditions);
            EventManager.onPluginUnload += OnPluginUnload;
        }

        private static void OnGetAdditionalConditions(GetAdditionalConditionsEvent args)
        {
            args.AdditonalConditions.AddRange(CustomConditions.Keys.Select(x => new GetAdditionalConditionsEvent.ConditionInfo { XmlName = x, Callback = ConditionLoadCallback}));
        }
        private static SerializableCondition ConditionLoadCallback(XmlReader reader)
        {
            var conditionType = CustomConditions[reader.Name];
            PathfinderCondition condition = (PathfinderCondition)Activator.CreateInstance(conditionType);
            condition.LoadFromXml(reader);
            return condition;
        }
        
        private static void OnPluginUnload(Assembly pluginAsm)
        {
            var allTypes = pluginAsm.GetTypes();
            foreach (var name in CustomConditions.Where(x => allTypes.Contains(x.Value)).Select(x => x.Key).ToList())
                CustomConditions.Remove(name);
        }
        
        public static void RegisterCondition<T>(string xmlName) where T : PathfinderCondition => RegisterCondition(typeof(T), xmlName);
        public static void RegisterCondition(Type conditionType, string xmlName)
        {
            if (!typeof(PathfinderCondition).IsAssignableFrom(conditionType))
                throw new ArgumentException("Condition type must inherit from Pathfinder.Action.PathfinderCondition!", nameof(conditionType));
            CustomConditions.Add(xmlName, conditionType);
        }

        public static void UnregisterCondition<T>() where T : PathfinderCondition => UnregisterCondition(typeof(T));
        public static void UnregisterCondition(Type conditionType)
        {
            var xmlName = CustomConditions.FirstOrDefault(x => x.Value == conditionType).Key;
            if (xmlName != null)
                CustomConditions.Remove(xmlName);
        }
        public static void UnregisterCondition(string xmlName)
        {
            CustomConditions.Remove(xmlName);
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SerializableCondition), nameof(SerializableCondition.GetSaveString))]
        internal static bool GetSaveStringOverridePrefix(SerializableCondition __instance, ref string __result)
        {
            if (__instance is PathfinderCondition pfCondition)
            {
                __result = pfCondition.GetSaveStringOverridable();
                return false;
            }

            return true;
        }
    }
}
