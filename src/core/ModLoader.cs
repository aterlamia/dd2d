using Godot;
using System.Collections.Generic;

namespace dd2d.core
{
    public static class ModLoader
    {
        private static string ModsPath = "res://mods";
        private static List<string> _activeMods = null;
        
        public static List<string> GetActiveMods()
        {
            if (_activeMods != null) return _activeMods;
            
            _activeMods = new List<string>();
            
            string absoluteModsPath = ProjectSettings.GlobalizePath(ModsPath);
            if (DirAccess.DirExistsAbsolute(absoluteModsPath))
            {
                var dir = DirAccess.Open(ModsPath);
                if (dir != null)
                {
                    dir.ListDirBegin();
                    string modName = dir.GetNext();
                    while (modName != "")
                    {
                        if (dir.CurrentIsDir() && !modName.StartsWith("."))
                        {
                            string modPath = $"{ModsPath}/{modName}";
                            _activeMods.Add(modPath);
                            Log.Debug($"Found mod: {modName}", "ModLoader");
                        }
                        modName = dir.GetNext();
                    }
                    dir.ListDirEnd();
                }
            }
            else
            {
                Log.Debug("No mods folder found", "ModLoader");
            }
            
            return _activeMods;
        }
        
        public static Godot.Collections.Dictionary LoadMergedJson(string basePath)
        {
            // 1. Load base game JSON
            var baseData = LoadJson(basePath);
            if (baseData == null)
            {
                Log.Error($"Failed to load base JSON: {basePath}", "ModLoader");
                return new Godot.Collections.Dictionary();
            }
            
            // 2. Merge each mod's version
            foreach (var modPath in GetActiveMods())
            {
                string modFilePath = $"{modPath}/{basePath}";
                if (FileAccess.FileExists(modFilePath))
                {
                    var modData = LoadJson(modFilePath);
                    if (modData != null)
                    {
                        Log.Debug($"Merging mod data: {modFilePath}", "ModLoader");
                        baseData = MergeJson(baseData, modData);
                    }
                }
            }
            
            return baseData;
        }
        
        private static Godot.Collections.Dictionary LoadJson(string path)
        {
            if (!FileAccess.FileExists(path))
            {
                Log.Error($"JSON file not found: {path}", "ModLoader");
                return null;
            }
            
            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            var jsonText = file.GetAsText();
            file.Close();
            
            var result = Json.ParseString(jsonText);
            if (result.Obj is Godot.Collections.Dictionary dict)
                return dict;
            
            Log.Error($"Invalid JSON format: {path}", "ModLoader");
            return null;
        }
        
        private static Godot.Collections.Dictionary MergeJson(
            Godot.Collections.Dictionary baseDict, 
            Godot.Collections.Dictionary modDict)
        {
            var result = new Godot.Collections.Dictionary();
            
            // Start with base
            foreach (var key in baseDict.Keys)
                result[key] = baseDict[key];
            
            // Merge mod over base
            foreach (var key in modDict.Keys)
            {
                if (result.ContainsKey(key))
                {
                    var baseVal = result[key];
                    var modVal = modDict[key];
                    
                    // If both are dictionaries, recursive merge
                    if (baseVal.Obj is Godot.Collections.Dictionary baseSubDict &&
                        modVal.Obj is Godot.Collections.Dictionary modSubDict)
                    {
                        result[key] = MergeJson(baseSubDict, modSubDict);
                    }
                    // If both are arrays, append
                    else if (baseVal.Obj is Godot.Collections.Array baseArr &&
                             modVal.Obj is Godot.Collections.Array modArr)
                    {
                        var merged = new Godot.Collections.Array();
                        foreach (var item in baseArr) merged.Add(item);
                        foreach (var item in modArr) merged.Add(item);
                        result[key] = merged;
                    }
                    // Otherwise, mod overrides
                    else
                    {
                        result[key] = modVal;
                    }
                }
                else
                {
                    result[key] = modDict[key];
                }
            }
            
            return result;
        }
    }
}
