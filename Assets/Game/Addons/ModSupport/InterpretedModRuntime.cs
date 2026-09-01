using System;
using System.Collections.Generic;
using UnityEngine;

namespace DaggerfallWorkshop.Game.Utility.ModSupport
{
    /// <summary>
    /// Small AOT-safe behavior runner for external mods. It executes data only;
    /// no assemblies, native libraries, or source code are loaded.
    /// </summary>
    public static class InterpretedModRuntime
    {
        public const string BehaviorExtension = ".dfmod.behavior.json";

        public delegate bool Operation(Mod mod, string[] arguments, object context);

        private static readonly Dictionary<string, Operation> operations =
            new Dictionary<string, Operation>(StringComparer.Ordinal);
        private static readonly Dictionary<Mod, Program> programs =
            new Dictionary<Mod, Program>();

        public static void RegisterOperation(string name, Operation operation)
        {
            if (string.IsNullOrEmpty(name) || operation == null)
                throw new ArgumentException("A behavior operation requires a name and implementation.");

            operations[name] = operation;
        }

        public static void Load(Mod mod)
        {
            if (mod == null || mod.AssetBundle == null)
                return;

            foreach (string assetName in mod.AssetNames)
            {
                if (!assetName.EndsWith(BehaviorExtension, StringComparison.OrdinalIgnoreCase))
                    continue;

                TextAsset asset = mod.GetAsset<TextAsset>(assetName);
                if (asset == null)
                    continue;

                Program program = JsonUtility.FromJson<Program>(asset.text);
                if (program == null || program.version != 1)
                {
                    Debug.LogError("Unsupported interpreted behavior in mod: " + mod.Title);
                    return;
                }

                programs[mod] = program;
                return;
            }
        }

        public static bool HasBehavior(Mod mod)
        {
            if (mod == null || mod.AssetNames == null)
                return false;

            foreach (string assetName in mod.AssetNames)
                if (assetName.EndsWith(BehaviorExtension, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        public static void Unload(Mod mod)
        {
            if (mod != null)
                programs.Remove(mod);
        }

        public static void Dispatch(string eventName)
        {
            Dispatch(eventName, null);
        }

        public static void Dispatch(string eventName, object context)
        {
            foreach (KeyValuePair<Mod, Program> pair in programs)
            {
                if (!pair.Key.Enabled || pair.Value.handlers == null)
                    continue;

                foreach (Handler handler in pair.Value.handlers)
                {
                    if (handler != null && handler.eventName == eventName)
                        Execute(pair.Key, handler.actions, context);
                }
            }
        }

        private static void Execute(Mod mod, Action[] actions, object context)
        {
            if (actions == null)
                return;

            foreach (Action action in actions)
            {
                if (action == null || string.IsNullOrEmpty(action.operation))
                    continue;

                Operation operation;
                if (!operations.TryGetValue(action.operation, out operation))
                {
                    Debug.LogWarning("Unknown interpreted mod operation '" + action.operation + "' in " + mod.Title);
                    continue;
                }

                try
                {
                    operation(mod, action.arguments ?? new string[0], context);
                }
                catch (Exception exception)
                {
                    Debug.LogError("Interpreted mod operation failed in " + mod.Title + ": " + exception.Message);
                }
            }
        }

        [Serializable]
        private class Program
        {
            public int version;
            public Handler[] handlers;
        }

        [Serializable]
        private class Handler
        {
            public string eventName;
            public Action[] actions;
        }

        [Serializable]
        private class Action
        {
            public string operation;
            public string[] arguments;
        }
    }
}
