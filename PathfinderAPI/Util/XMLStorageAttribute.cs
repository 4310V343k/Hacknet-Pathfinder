﻿using System;
using System.Reflection;
using System.Text;
using System.Xml;
using BepInEx.Logging;
using HarmonyLib;
using Pathfinder.Util.XML;

namespace Pathfinder.Util
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class XMLStorageAttribute : Attribute
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public;
        
        public static string WriteToXml(object obj)
        {
            var thisType = obj.GetType();
            var attribType = typeof(XMLStorageAttribute);

            var builder = new StringBuilder();

            builder.Append($"<{thisType.Name} ");
            
            foreach (var fieldInfo in thisType.GetFields(Flags))
            {
                if (fieldInfo.GetCustomAttributes(attribType, false).Length < 1)
                    continue;
                if (fieldInfo.IsStatic || fieldInfo.FieldType != typeof(string))
                {
                    Logger.Log(LogLevel.Error, $"Invalid field for XML storage: {fieldInfo.Name}");
                    continue;
                }

                string val = (string)fieldInfo.GetValue(obj);
                builder.Append($"{fieldInfo.Name}=\"{val}\"");
            }
            foreach (var propertyInfo in thisType.GetProperties(Flags))
            {
                if (propertyInfo.GetCustomAttributes(attribType, false).Length < 1)
                    continue;
                
                var getMethod = propertyInfo.GetGetMethod();
                var setMethod = propertyInfo.GetSetMethod();
                if (getMethod.IsStatic || setMethod.IsStatic || propertyInfo.PropertyType != typeof(string))
                {
                    Logger.Log(LogLevel.Error, $"Invalid property for XML storage: {propertyInfo.Name}");
                    continue;
                }

                string val = (string)getMethod.Invoke(obj, null);
                builder.Append($"{propertyInfo.Name}=\"{val}\"");
            }

            builder.Append("/>");
            
            return builder.ToString();
        }

        public static void ReadFromXml(XmlReader reader, object obj)
        {
            var thisType = obj.GetType();
            var attribType = typeof(XMLStorageAttribute);

            foreach (var fieldInfo in thisType.GetFields(Flags))
            {
                if (fieldInfo.GetCustomAttributes(attribType, false).Length < 1)
                    continue;
                if (fieldInfo.FieldType != typeof(string))
                {
                    Logger.Log(LogLevel.Error, $"Invalid field for XML storage: {fieldInfo.Name}");
                    continue;
                }

                string val = reader.GetAttribute(fieldInfo.Name);
                fieldInfo.SetValue(obj, val);
            }
            foreach (var propertyInfo in thisType.GetProperties(Flags))
            {
                if (propertyInfo.GetCustomAttributes(attribType, false).Length < 1)
                    continue;
                
                var getMethod = propertyInfo.GetGetMethod();
                var setMethod = propertyInfo.GetSetMethod();
                if (propertyInfo.PropertyType != typeof(string))
                {
                    Logger.Log(LogLevel.Error, $"Invalid property for XML storage: {propertyInfo.Name}");
                    continue;
                }

                string val = reader.GetAttribute(propertyInfo.Name);
                setMethod.Invoke(obj, new object[] { val });
            }
        }

        public static void ReadFromElement(ElementInfo info, object obj)
        {
            var thisType = obj.GetType();
            var attribType = typeof(XMLStorageAttribute);

            foreach (var fieldInfo in thisType.GetFields(Flags))
            {
                if (fieldInfo.GetCustomAttributes(attribType, false).Length < 1)
                    continue;
                if (fieldInfo.FieldType != typeof(string))
                {
                    Logger.Log(LogLevel.Error, $"Invalid field for XML storage: {fieldInfo.Name}");
                    continue;
                }

                fieldInfo.SetValue(obj, info.Attributes.GetString(fieldInfo.Name, null));
            }
            foreach (var propertyInfo in thisType.GetProperties(Flags))
            {
                if (propertyInfo.GetCustomAttributes(attribType, false).Length < 1)
                    continue;
                
                var getMethod = propertyInfo.GetGetMethod();
                var setMethod = propertyInfo.GetSetMethod();
                if (propertyInfo.PropertyType != typeof(string))
                {
                    Logger.Log(LogLevel.Error, $"Invalid property for XML storage: {propertyInfo.Name}");
                    continue;
                }

                setMethod.Invoke(obj, new object[] { info.Attributes.GetString(propertyInfo.Name, null) });
            }
        }
    }
}
