# MEROptimizer
[![Version](https://img.shields.io/github/v/release/MathMot/MEROptimizer?&label=Version&color=d500ff)](https://github.com/MathMot/MEROptimizer/releases/latest) [![LabAPI Version](https://img.shields.io/badge/LabAPI_Version-1.1.0-51f4ff )](https://github.com/northwood-studios/LabAPI/releases/tag/1.0.2) [![EXILED Version](https://img.shields.io/badge/EXILED_Version-9.6.1-FFFFA0 )](https://github.com/ExMod-Team/EXILED/releases/tag/v9.6.1) [![SCP:SL Version](https://img.shields.io/badge/SCP:SL_Version-14.1.1-blue?&color=e5b200)](https://store.steampowered.com/app/700330/SCP_Secret_Laboratory/) [![Total Downloads](https://img.shields.io/github/downloads/MathMot/MEROptimizer/total.svg?label=Total%20Downloads&color=&color=ffbf00)]()<br>

---

[SCP:Secret Laboratory](https://store.steampowered.com/app/700330/SCP_Secret_Laboratory/) plugin made for optimizing  [MapEditorReborn](https://github.com/Michal78900/ProjectMER)(ProjectMER for now) schematics using the [LabAPI](https://github.com/northwood-studios/LabAPI) or [EXILED](https://github.com/ExMod-Team/EXILED) Framework.
It works by destroying primitives for the server and sending clones of the primitives to the clients while simulating their collisions.
It also supports a feature to dynamicly spawn primitives around players.
It got also an API to create and handle client sided primitives (ClientSidePrimitive class).<br>
You can ask me questions on discord (matmoth)
[![Plugin Preview](https://github.com/MathMot/MEROptimizer/blob/dev/doc/PDSDisplay.gif?raw=true)]()

---

## Features : 

#### <b>Server side optimization : </b>

- Completely removes the performance cost of primitives, meaning that having 60/60 tps with multiple thousands primitives loaded is possible.

#### <b>Client side optimization : </b>

- Primitives are dynamicly spawned around each players, so that clients only have to spawn and render the primitives that are near them



---
## <b>Commands : </b>
###### The available commands are only debug commands made to have a better understanding on how the plugin manages the primitives and clusters. Also giving informations about loaded primitives.

- <b>mero.clusters</b> : Displays information about all of the clusters in schematics.
- <b>mero.disable</b> : Disable or enable the plugin for the current round, for testing purposes
- <b>mero.displayClusters</b> : Display in game via primitives the radius of all clusters for the sender
- <b>mero.displayPrimitives</b> : Display or destroy all of the client side primitives for the sender
- <b>mero.info</b> : Displays information about all schematics ( Total primitive count, client side primitive count, colliders count, etc)
- <b>mero.realtimedisplay</b> : Displays as hints the number of currently loaded primitives for the sender
![](https://github.com/MathMot/MEROptimizer/blob/dev/doc/rtdp.PNG?raw=true)
*what mero.realtimedisplay shows to a player, refreshing every second*

---



Setup : 
-- 
- Make sure that your SCP:SL server version(14.1.0) corresponds <br>
- [Download](https://github.com/MathMot/MEROptimizer/releases/latest) the latest version of the plugin and place your .dll in  the *LabAPI\plugins\<global/port>* folder.

---

Configuration :
-- 
*It's the same configuration for both frameworks, some labels may change.*
Default configuration : 
```yaml
# If the plugin is enabled or not.
is_enabled: true
# Displays plugin debug logs.
debug: false
# 
#-------------Global Options-------------
# If the primitives that will be optimized are only non collidable
optimize_only_non_collidable: false
# Prevents group of primitives to be optimized (aka keeped server sided)
# Simply name one of its empty parents with one of the entered name here and it will be excluded.
exclude_objects: []
# 
#-------------Schematic cluster splitting options-------------
# Could be quite hard to understand, more info in the plugin readme
# If enabled, splits schematics into clusters of primitives to then spawn them independently per players based on their distance to the cluster
clusterize_schematic: true
# Prevents group of primitives to be used by the clusters. Useful for skyboxs, outer walls of buildings and giant primitives that requires to be seen from far away# Simply name one of its empty parents with one of the entered names here and it will be excluded.
exclude_unspawning_distant_objects: []
# In units, the distance required for a cluster to spawn/unspawn its primitives to the corresponding player
spawn_distance: 50
# Adds a specific spawn distance for cluster of each entered schematics, bypassing the previously entered SpawnDistance
custom_schematic_spawn_distance: {}
# Should spectating players be also affected by the cluster systemIf enabled, when a player spectates another, it will spawn all of the primitives that the spectated player currently sees, otherwise spectators will see all of the schematics at all time
should_spectator_be_affected_by_distance_spawning: false
# Should tutorials be affected by the cluster system, if disabled, every tutorials will see all of the schematics at all time (useful for moderation stuff, etc)
should_tutorials_be_affected_by_distance_spawning: true
# Minimum size of a primitive before being considered as a big one (size = (scale.x + y + z) )
# Huge objects don't work with the cluster system and so they need to be excluded, prevents having to manually exclude each walls/floors of schematics
# Set to zero (0) to disable it, not recommended
minimum_size_before_being_big_primitive: 10
# For each cluster, number of primitives that'll spawn per server frame (higher count means quicker spawn but potential freezes for clients)If set to zero (0), each cluster will spawn its primitives instantly, 0.5 means 1 primitive each 2 frames, etc
number_of_primitive_per_spawn: 0.100000001
# 
#-----Clusters Options-----
#In units, the maximum distance between a primitive and a specific cluster to be included in it, the more distance the less cluster will spawn
max_distance_for_primitive_cluster: 2.5
# Maximum amount of primitive per cluster, if reached, a new cluster will spawn and be used. The less primitives per cluster the more clusters will spawn
max_primitives_per_cluster: 100

```

  # -------------Global Options-------------

  
### <b><code style="color : greenyellow">optimize_only_non_collidable</code></b>
If enabled, every primitives that are collidable will not be optimized. In case colliders in game are funky.

### <b><code style="color : greenyellow">exclude_objects</code></b>
There you can add every object name that the plugin will exclude for the optimization.
For example, if you set your config like this : 


```yml
  exclude_objects:
  - 'ExcludedPrimitives'
  - 'Skins'
  ```
All of the primitives that have at least one of its parent  that contains "ExcludedPrimitives" or "Skins" will not be optimized.
Example : <br>
[![Example](https://github.com/MathMot/MEROptimizer/blob/dev/doc/optimizedExample.png?raw=true)]()<br>
*Note : If you put a schematic name in it, the plugin will exclude completly the schematic from the optimization process and none of its primitives will be optimized*
 # --------Schematic cluster splitting options--------

### <b><code style="color : greenyellow">clusterize_schematic</code></b>
If enable, the clutter system will be enabled.
The cluster system means the splitting of each schematic into small clusters of primitives, used to then spawn or unspawn the primitives of the cluster to each player based on their distance with the position of the cluster.

### <b><code style="color : greenyellow">exclude_unspawning_distant_objects</code></b>
Same system as <b><code style="color : greenyellow">exclude_objects</code></b>, every schematics that has a parent with the same name as one written in the list will be excluded of the clusters and will be spawned with the schematic without being spawn/unspawn to players.
Usefull for large outside walls of buildings, huge text, logos, walls and floors of indoors builds, etc..

### <b><code style="color : greenyellow">spawn_distance</code></b>
The distance between the cluster and the player needed for the primitives of the cluster to spawn/unspawn from the player, a larger distance means that more primitives will be spawned to players at the same time, you can match the distance with the natural fog in game if needed.

### <b><code style="color : greenyellow">custom_schematic_spawn_distance</code></b>

Overrides the default <code style="color : greenyellow">spawn_distance</code></b> value for specific schematic, if a schematic isn't listed in this property, the spawn distance will be the <code style="color : greenyellow">spawn_distance</code></b> value.

For example, setting this : 
```yml
 # Adds a specific spawn distance for cluster of each entered schematics, bypassing the previously entered SpawnDistance
  custom_schematic_spawn_distance:
    Underground: 2
```
will change the spawn distance for the schematic name exactly "Underground" to 2.
### <b><code style="color : greenyellow">should_spectator_be_affected_by_distance_spawning</code></b>
If enabled, spectators will load only what their spectated player sees, while it's being updated at the same time.
If disabled, all spectators will have all of the schematics spawned everytime whoever they spectated.
The only downside of enabling it is that when spectating new players, their corresponding primitives takes some times to spawn and as such it could be disturbing to watch.
[![Plugin Preview](https://github.com/MathMot/MEROptimizer/blob/dev/doc/ssbabds.gif?raw=true)]()

### <b><code style="color : greenyellow">should_tutorials_be_affected_by_distance_spawning</code></b>

Same principle as the one above but for tutorials, mostly made for helping the moderation, taking screenshots from far away, etc...

### <b><code style="color : greenyellow">number_of_primitive_per_spawn</code></b>

When a cluster has to spawn primitives, a fade option is available so that primitives takes multiple frame to spawn, preventing unpleasant freezes from players when trying to load a huge amount of primitives/clusters.
The recommended value is between 0.2 and 1 primitive.

-1 = instant spawn
0.1 ... 0.9 = 1 spawn each X frames (0.2 = 1 spawn every 2 frames)
1 ... ∞ = X spawn per frame (2 = 2 spawn per frame)

### <b><code style="color : greenyellow">minimum_size_before_being_big_primitive</code></b>
Big primitives ( such as walls, roofs, stuff like that ) are not clustered by default, simply because huge objects are more likely needed to be seen from far away, and it doesn't match with how the cluster optimisation work
So, if a primitive total scale (x+y+z) is higher than this value, it will not be in any cluster and will be spawned for each players independently from where they're located.

 # -------------Clusters Options-------------
###### Clusters works via an algorithm that, starting from the center of the schematic, get every primitives around it within a specified distance until it's full, if there are no more primitives to get around it or if it's full, another cluster will be spawned based on the closest primitive of the center that is not in a cluster yet, and start again

### <b><code style="color : greenyellow">max_distance_for_primitive_cluster</code></b>
The maximum distance where primitives can enter a cluster.
The higher the value the more primitives will enter, lowering the quality of the client side optimization.

### <b><code style="color : greenyellow">max_primitives_per_cluster</code></b>
Maximum number of primitives that can enter a cluster
The higher the value the more primitives can enter a cluster, lowering the quality of the client side optimization.

#### For both of the above values, the more cluster is spawned the more the server, depending on its hardware, will potentially lag.
---

Informations : 
-- 
- Quads and Planes <b>with collisions</b> will not be optimized by the plugin, the primitives will stay by default on the server side.
- For this plugin to work without breaking any in game system, the real colliders detecting the players are spawned at 2000 units above the real clusters, each player also has a rigidbody+collider 2000 units above them for the colliders to detect them, so if you have any playable area at 2000 units above another one, colliding issues could appear.
- If you're using AMERT or some sort of external plugin that interacts with in game primitives, be sure to use the exclusion feature to exclude every object that the plugin could use (for example with AMERT, every object with scripts, animations, etc.)<br>If this doesn't work, feel free to reach me via discord to find a solution.
- SCP 079 can't use the clusterize option for now, if a player is SCP 079, he will load every maps at all times to prevent desync and stuff


---

Credits : 
-- 
- Plugin made by [Math](https://github.com/MathMot) 
- Original idea/sample code by a friend
- Example map in this readme is also made by a friend

Thanks to a lot of members of the community for helping me find issues with the plugin.