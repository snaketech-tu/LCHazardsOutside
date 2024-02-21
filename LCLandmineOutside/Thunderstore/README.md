Hazards Outside
===============  

Introducing the "Lethal Company: Outdoors Hazards Edition" mod – because why confine chaos and calamity to the comfort of four walls?  
This mod will let you spawn landmines, turrets and even modded hazards outside with spawn rates configurable to your liking.

So, if you've ever thought to yourself, "Gee, I love Lethal Company, but wouldn't it be great if it tried to kill me in the great outdoors?" – well, this mod's got your back.  
Literally. Because there's probably a turret back there too. Happy surviving!

Details
--------
- Seeded hazard spawns so they can be replicated. Try it out on challenge moons!
- Configuration for each hazard:
    - Min/Max spawn rates per hazard.
    - Min/Max spawn rates per moon per hazard.
    - Global "No Extra Hazard Spawn" chance from 0-100%.
- Modded moons are supported. Keep in mind that hazard spawn points might be unpredictable due to modders layering their moons differently than the base game.
- Modded hazards are supported (if added to the game correctly). Successfully tested with Evaisa's teleport traps from [LethalThings](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalThings/).
- "Safe" zones for turret spawns to avoid game breaking (because that's no fun).
- Fire exits should be relatively safe, although this may change in the future.

Installation
------------

### Automatic (Recommended)

- Install through Thunderstore or r2modman.

### Manual

- Install BepinEx.
- Place BepInEx/plugins/LCHazardsOutside.dll in your BepInEx/plugins folder.
- That's it! Only the host needs this mod installed! Amazing!

FAQ
---

> **Q**: Does only the host need this mod?  
> **A**: Yes.

> **Q**: What if the others have it? Will it break?  
> **A**: No, the mod will simply not do anything if the user is not hosting.

> **Q**: Why is [_insert any modded object here_] also spawning outside?  
> **A**: It means that the modder put that object into the base game's list of hazards. If it causes issues, please create an issue on github for a compatibility patch. I'll look into it if I have the time.

Upcoming features
-----------------

- Various configurable spawn strategies. This could include making fire exits less safe, or having a turret guard an exit.

Known issues
------------

- Rotation does not work at all on Vow for some reason.