# Interpreted external mods

The iOS mod runtime accepts an optional `*.dfmod.behavior.json` asset in a
mod bundle. This is a data-only behavior format. It does not load C# source,
managed assemblies, native libraries, or executable code.

Version 1 format:

```json
{
  "version": 1,
  "handlers": [
    {
      "eventName": "start",
      "actions": [
        { "operation": "example.operation", "arguments": ["value"] }
      ]
    }
  ]
}
```

The runtime currently dispatches `start`, `terrain.promoted`, and
`weather.changed`. Host systems register operation implementations with
`InterpretedModRuntime.RegisterOperation`. Operation implementations are
part of the generic app capability surface; mod-specific behavior remains in
the external behavior file.

The built-in terrain capability provides `terrain.details.apply`, whose single
argument is the name of a `TerrainDetailSet` JSON asset in the same bundle, and
`terrain.details.clear`. A set describes prototype assets and optional density
layers without requiring executable code.

Operations receive the owning `Mod`, string arguments, and the event context.
Terrain events provide `InterpretedTerrainContext`; weather events provide
`WeatherType`.
Implementations must validate arguments and remain on the main Unity thread.

Legacy `.cs.txt` and `.dll.bytes` assets continue to use the desktop runtime
compiler/loader and are deliberately not treated as interpreted behavior.
