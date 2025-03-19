# MEROptimizer

[SCP:Secret Laboratory](https://store.steampowered.com/app/700330/SCP_Secret_Laboratory/) plugin made for optimizing  [MapEditorReborn](https://github.com/Michal78900/MapEditorReborn) schematics using the [EXILED](https://github.com/ExMod-Team/EXILED) Framework.
It works by destroying primitives for the server and sending clones of the primitives to the clients while simulating their collisions.
It also supports a feature to dynamicly spawn primitives around players.
You can ask me questions on discord (matmoth)
[![Plugin Preview](https://github.com/MathMot/MEROptimizer/blob/dev/doc/PDSDisplay.gif?raw=true)]()

Features : 
---
### <b>Server side optimization : </b>

- Completely removes the performance cost of primitives, meaning that having 60/60 tps with multiple thousands primitives loaded is possible.

### <b>Client side optimization : </b>

- Primitives are dynamicly spawned around each players, so that clients only have to spawn and render the primitives that are near them




### <b>Commands : </b>
###### The available commands are only debug commands made to have a better understanding on how the plugin manages the primitives and clusters. Also giving informations about loaded primitives.

mero\.info (dprtp)             |  mero.realtimedisplay 
:-------------------------:|:---------------------------:|
Displays information about all of the spawned schematics<br><img style="display: flex , and align-items: center" src="https://i.imgur.com/v6NE7uL.png" width="500" /><br><br><br> |  Starts to display as a hint the number of primitives loaded for your client.<br>Updating every seconds<br><img style="display: flex , and align-items: center" src="https://github.com/MathMot/MEROptimizer/blob/dev/doc/PDSDisplay.gif?raw=true" width="500" /><br> 




<br>mero.hide : Hides all of the client side primitives to display only the ones spawned on the server (only for the player executing the command)
<br>mero.refresh : Refresh all of the client side primitives (only for the player executing the command)
<br>mero\.info : Displays information about all of the optimized schematics (number of client side primitives, colliders, etc...)

---

Setup : 
-- 
- Make sure that your SCP:SL server version(14.0.2) and EXILED version (9.5.1) are corresponding <br>
- [Download](https://github.com/MathMot/MEROptimizer/releases/latest) the latest version of the plugin and place the .dll in EXILED/Plugins

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

Informations : 
-- 
- Quads and Planes <b>with collisions</b> will not be optimized by the plugin, the primitives will stay by default on the server side.
