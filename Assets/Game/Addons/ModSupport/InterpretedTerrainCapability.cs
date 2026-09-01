using System;
using UnityEngine;

namespace DaggerfallWorkshop.Game.Utility.ModSupport
{
    /// <summary>
    /// Context supplied to an interpreted mod when a terrain is promoted.
    /// The context is immutable from the mod's point of view; mutations go
    /// through registered capability operations.
    /// </summary>
    public sealed class InterpretedTerrainContext
    {
        public DaggerfallWorkshop.DaggerfallTerrain Sender { get; private set; }
        public TerrainData TerrainData { get; private set; }
        public int ClimateIndex { get; private set; }
        public Color32[] TileMap { get; private set; }

        public InterpretedTerrainContext(
            DaggerfallWorkshop.DaggerfallTerrain sender,
            TerrainData terrainData,
            int climateIndex,
            Color32[] tileMap)
        {
            Sender = sender;
            TerrainData = terrainData;
            ClimateIndex = climateIndex;
            TileMap = tileMap;
        }
    }

    /// <summary>
    /// Generic terrain-detail capability exposed to interpreted mod hosts.
    /// Implementations own the engine-specific work; external packages supply
    /// only asset names and validated values.
    /// </summary>
    public interface IInterpretedTerrainCapability
    {
        bool ApplyPrototypeSet(Mod mod, InterpretedTerrainContext context, TerrainDetailSet set);
        bool ApplyLayer(InterpretedTerrainContext context, TerrainDetailLayer layer);
        void Clear(InterpretedTerrainContext context);
    }

    [Serializable]
    public sealed class TerrainDetailSet
    {
        public TerrainDetailPrototype[] prototypes;
        public int detailResolution = 256;
        public int detailResolutionPerPatch = 8;
        public float objectDistance = 100f;
        public float objectDensity = 1f;
        public TerrainDetailLayer[] layers;
    }

    [Serializable]
    public sealed class TerrainDetailPrototype
    {
        public string asset;
        public bool usePrototypeMesh;
        public string renderMode = "grass";
        public float minWidth = 1f;
        public float maxWidth = 1f;
        public float minHeight = 1f;
        public float maxHeight = 1f;
        public float noiseSpread = 0.5f;
        public Color healthyColor = Color.white;
        public Color dryColor = Color.white;
    }

    [Serializable]
    public sealed class TerrainDetailLayer
    {
        public int prototypeIndex;
        public int width = 128;
        public int height = 128;
        public int[] density;

        public int Get(int x, int y)
        {
            if (density == null || x < 0 || y < 0 || x >= width || y >= height)
                return 0;

            int index = y * width + x;
            return index < density.Length ? density[index] : 0;
        }
    }
}
