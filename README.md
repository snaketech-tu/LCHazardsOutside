Hazards Outside
===============  

Introducing the "Lethal Company: Outdoors Hazards Edition" mod – because why confine chaos and calamity to the comfort of four walls?  
This mod will let you spawn landmines, turrets and even modded hazards outside with spawn rates configurable to your liking.

So, if you've ever thought to yourself, "Gee, I love Lethal Company, but wouldn't it be great if it tried to kill me in the great outdoors?" – well, this mod's got your back.  
Literally. Because there's probably a turret back there too. Happy surviving!  

Installation
------------

### Automatic (Recommended)

- Install through Thunderstore or r2modman.

### Manual

- Install BepinEx.
- Place LCHazardsOutside.dll in your BepInEx/plugins folder.
- That's it! Only the host needs this mod installed! Amazing!

Details
--------
- Seeded hazard spawns so they can be replicated. Try it out on challenge moons!
- Configuration for each hazard:
    - Min/Max spawn rates per hazard.
    - Min/Max spawn rates per moon per hazard.
    - Different spawn strategies: Cover the paths to all exits or concentrate the hazards on the main path only!
    - Global "No Extra Hazard Spawn" chance from 0-100%.
- Modded moons are supported. Keep in mind that hazard spawn points might be unpredictable due to modders layering their moons differently than the base game.  
- Modded hazards are supported (if added to the game correctly). Successfully tested with Evaisa's teleport traps from [LethalThings](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalThings/).  
- The positioning of hazards will be replicated outdoors as closely as possible to their indoor setup. For example, a turret will attempt to spawn with its rear facing a wall or an obstacle, and a spike trap will be placed flush against a wall.  
This alignment might not always be perfect outdoors due to uneven terrain, but this mod aims to maintain consistent placement.
- By default, only landmines and spike traps are enabled using the `MainAndFireExit` spawn strategy. Other hazards are opt-in in the configuration for higher difficulty!  

Spawn Strategies  
----------------  
### MainAndFireExit (Default)  
This strategy distributes hazards along the paths from the ship to the main entrance and the fire exits.  
The total number of hazards is divided equally among the exits, ensuring coverage across multiple areas. 
Compared to the older `MainEntranceOnly` method, this approach covers a broader area and, as such, it is advisable to increase the number of hazards to be spawned.

### MainEntranceOnly 
Previously the default in version `1.1.3` and earlier, this method calculates the midpoint between the ship and the main entrance to concentrate hazards. 
This strategy ensures that fire exits remain mostly hazard-free.  

### FireExitsOnly 
Similar to the `MainEntranceOnly` strategy, but exclusively targeting all available fire exits. Opt for this method if you aim to enhance the challenge for players using the fire exits.