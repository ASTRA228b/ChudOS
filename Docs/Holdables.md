# ChudOS local holdables

For VR menu controls, choose **Holdables > Premade Holdable**, then enable **Equip Premade Holdable**. The twelve included models are Toy Sword, Toy Hammer, Round Shield, Magic Wand, Banana, Flower, Toy Microphone, ChudOS Flag, Wooden Cross, Bisexual Flag, Gay Pride Flag (rainbow), and Trans Flag. All are created by the plugin; no bundles or downloads are needed. Their original colors are the default; Settings can apply a theme/player/custom preset tint.

**Holdable Browser** also lists all twelve premade models, with preview, favorites, and independent left/right equipped slots. Custom visual bundles are optional. The built-in camera, squeeze toy, flashlight, measuring tape and orb tools also work without imports. One visual-tool controller runs at a time to avoid overlapping held objects.

Place a Unity asset bundle and its manifest in `Gorilla Tag/ChudOS/Holdables`. For example, `example.holdable.json`:

```json
{
  "Name": "Example Holdable",
  "Bundle": "example.bundle",
  "Prefab": "assets/holdables/example.prefab"
}
```

`Bundle` is a filename in that folder. `Prefab` is the exact asset name inside the bundle. Build Windows-compatible bundles using a Unity version compatible with the installed game. Models may use built-in rendering, animation and particle components, but this format rejects prefabs containing script components. Audio and collisions are disabled on visual model clones.

The browser lists up to 64 manifests, accepts bundles up to 64 MiB, and caches up to four bundles while open. Two prefabs in the same bundle share its cache. Reopening releases the cache. Legacy HoldablePad bundles are not automatically converted.

Choose **Holdable Hand** in Settings, then press **Equip**. Each hand retains its own model and base scale. Size, tint, position and rotation settings apply to equipped models. **Remove** clears the selected hand. **Reset Holdable Placement** restores size and offsets. Favorites save in `favorites.json` beside the manifests; normal option values use Save Menu State and profiles.

Imported models, camera previews, flashlight lighting, squeeze animation and orb effects are local. Native sticky/physical holdable tools require compatible items owned by the local player. **Sticky Holdables** retains items after you grab them; B/R explicitly releases them. **Physical Holdables** uses approximate local sphere proxies after release and disables sticky/prop-control tools to avoid competing controllers.

The camera preview refreshes at 10 Hz. Trigger/LMB or its GUI button saves a 256 × 144 PNG in `ChudOS/Photos`. The flashlight toggles with A/Space. Measuring Tape uses the selected trigger/LMB to start and finish measuring to the other hand; X/F resets it. Hand-dependent hints update when Holdable Hand changes.
