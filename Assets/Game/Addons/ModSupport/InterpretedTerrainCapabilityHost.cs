using System;
using UnityEngine;

namespace DaggerfallWorkshop.Game.Utility.ModSupport
{
    /// <summary>
    /// Built-in host for the generic terrain-detail capability. Mod packages
    /// select assets and values; this class contains no mod-specific data.
    /// </summary>
    public static class InterpretedTerrainCapabilityHost
    {
        private const string ApplyOperation = "terrain.details.apply";
        private const string ClearOperation = "terrain.details.clear";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            InterpretedModRuntime.RegisterOperation(ApplyOperation, Apply);
            InterpretedModRuntime.RegisterOperation(ClearOperation, Clear);
        }

        private static bool Apply(Mod mod, string[] arguments, object context)
        {
            InterpretedTerrainContext terrain = context as InterpretedTerrainContext;
            if (terrain == null || arguments == null || arguments.Length != 1)
                return false;

            TextAsset asset = mod.GetAsset<TextAsset>(arguments[0]);
            if (asset == null)
                return false;

            TerrainDetailSet set = JsonUtility.FromJson<TerrainDetailSet>(asset.text);
            if (set == null || set.prototypes == null || set.prototypes.Length == 0)
                return false;

            TerrainData data = terrain.TerrainData;
            data.SetDetailResolution(
                Mathf.Clamp(set.detailResolution, 32, 4096),
                Mathf.Clamp(set.detailResolutionPerPatch, 8, 128));

            TerrainDetailCapability.ApplyPrototypes(mod, data, set);
            if (set.layers != null)
                foreach (TerrainDetailLayer layer in set.layers)
                    TerrainDetailCapability.ApplyLayer(data, layer);

            Terrain unityTerrain = terrain.Sender != null
                ? terrain.Sender.GetComponent<Terrain>()
                : null;
            if (unityTerrain != null)
            {
                unityTerrain.detailObjectDistance = Mathf.Max(0f, set.objectDistance);
                unityTerrain.detailObjectDensity = Mathf.Clamp01(set.objectDensity);
            }

            return true;
        }

        private static bool Clear(Mod mod, string[] arguments, object context)
        {
            InterpretedTerrainContext terrain = context as InterpretedTerrainContext;
            if (terrain == null)
                return false;

            TerrainDetailCapability.Clear(terrain.TerrainData);
            return true;
        }
    }

    internal static class TerrainDetailCapability
    {
        internal static void ApplyPrototypes(Mod mod, TerrainData data, TerrainDetailSet set)
        {
            DetailPrototype[] prototypes = new DetailPrototype[set.prototypes.Length];
            for (int i = 0; i < prototypes.Length; i++)
            {
                TerrainDetailPrototype spec = set.prototypes[i];
                DetailPrototype prototype = new DetailPrototype
                {
                    minWidth = Mathf.Max(0f, spec.minWidth),
                    maxWidth = Mathf.Max(spec.minWidth, spec.maxWidth),
                    minHeight = Mathf.Max(0f, spec.minHeight),
                    maxHeight = Mathf.Max(spec.minHeight, spec.maxHeight),
                    noiseSpread = Mathf.Max(0f, spec.noiseSpread),
                    healthyColor = spec.healthyColor,
                    dryColor = spec.dryColor,
                    usePrototypeMesh = spec.usePrototypeMesh,
                    renderMode = ParseRenderMode(spec.renderMode),
                };

                if (!string.IsNullOrEmpty(spec.asset))
                {
                    if (spec.usePrototypeMesh)
                        prototype.prototype = mod.GetAsset<GameObject>(spec.asset);
                    else
                        prototype.prototypeTexture = mod.GetAsset<Texture2D>(spec.asset);
                }

                prototypes[i] = prototype;
            }

            data.detailPrototypes = prototypes;
        }

        internal static void ApplyLayer(TerrainData data, TerrainDetailLayer layer)
        {
            if (layer == null || layer.density == null ||
                layer.prototypeIndex < 0 || layer.prototypeIndex >= data.detailPrototypes.Length)
                return;

            int width = data.detailWidth;
            int height = data.detailHeight;
            int[,] values = new int[height, width];
            int copyWidth = Mathf.Min(width, Mathf.Max(0, layer.width));
            int copyHeight = Mathf.Min(height, Mathf.Max(0, layer.height));
            for (int y = 0; y < copyHeight; y++)
                for (int x = 0; x < copyWidth; x++)
                    values[y, x] = layer.Get(x, y);

            data.SetDetailLayer(0, 0, layer.prototypeIndex, values);
        }

        internal static void Clear(TerrainData data)
        {
            if (data == null)
                return;

            int[,] empty = new int[data.detailHeight, data.detailWidth];
            for (int i = 0; i < data.detailPrototypes.Length; i++)
                data.SetDetailLayer(0, 0, i, empty);
            data.detailPrototypes = new DetailPrototype[0];
        }

        private static DetailRenderMode ParseRenderMode(string value)
        {
            if (string.Equals(value, "vertexlit", StringComparison.OrdinalIgnoreCase))
                return DetailRenderMode.VertexLit;
            if (string.Equals(value, "grassbillboard", StringComparison.OrdinalIgnoreCase))
                return DetailRenderMode.GrassBillboard;
            return DetailRenderMode.Grass;
        }
    }
}
