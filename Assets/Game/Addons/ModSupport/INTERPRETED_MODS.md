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

Operations receive the owning `Mod`, string arguments, and the event context.
Terrain events provide `TerrainData`; weather events provide `WeatherType`.
Implementations must validate arguments and remain on the main Unity thread.

Legacy `.cs.txt` and `.dll.bytes` assets continue to use the desktop runtime
compiler/loader and are deliberately not treated as interpreted behavior.
