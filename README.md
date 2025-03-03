# MEROptimizer

[SCP:Secret Laboratory](https://store.steampowered.com/app/700330/SCP_Secret_Laboratory/) plugin made for optimizing  [MapEditorReborn](https://github.com/Michal78900/MapEditorReborn) schematics using the [EXILED](https://github.com/ExMod-Team/EXILED) Framework.
It works by destroying primitives for the server and sending clones of the primitives to the clients while simulating their collisions.

You can ask questions on the discord [here](https://discord.gg/AYkFPb6dzv).

---

Features : 
-- 
- Exclude certain group of primitives so that they won't be optimized (useful if another plugin has to use certain primitives of the schematics)

- Optimize only primitives with no collision

- Commands to display information about the "optimized" schematics.
<br>mero.hide : Hides all of the client side primitives to display only the ones spawned on the server (only for the player executing the command)
<br>mero.refresh : Refresh all of the client side primitives (only for the player executing the command)
<br>mero\.info : Displays information about all of the optimized schematics (number of client side primitives, colliders, etc...)

---

Setup : 
-- 
[Download](https://github.com/MathMot/MEROptimizer/releases) the latest version of the plugin and place the .dll in EXILED/Plugins

---

Configuration : 
-- 
Default configuration : 
```yaml
mero:
# If the plugin is enabled or not.
  is_enabled: true
  # Displays plugin debug logs.
  debug: false
  # If the primitives that will be optimized are only non collidable
  optimize_only_non_collidable: false
  # If your primitive has any parent with a name corresponding to one of them, it will not be optimized.
  exclude_objects: []
```
optimize_only_non_collidable : If set to true, the plugin will not optimize primitives with collisions.

exclude_objects : There you can add every object name that the plugin will exclude for the optimization.
For example, if you set your config like this : 
```yml
  exclude_objects:
  - 'ExcludedPrimitives'
  - 'Skins'
  ```
All of the primitives that have at least one of its parent  that contains "ExcludedPrimitives" or "Skins" will not be optimized.
Example : <br>
[![Example](https://imgur.com/JmTM9k6.png)](https://imgur.com/JmTM9k6.png)

---

Credits : 
-- 
- Plugin made by [Math](https://github.com/MathMot) 
- Original idea/sample code by a friend

