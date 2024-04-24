1.2.0 (v50 Update)  
==================  
- Updated for full support of v50.
- Added backwards compatibility for v49.
- Added the Spike Roof Trap from v50 as a new configurable hazard.  
- Added configurable spawn strategies! Currently there are three options: MainAndFireExit, MainEntranceOnly and FireExitsOnly.  
- Added better error handling and made spawning more reliable.  
- Spike Roof Traps will spawn flush against a wall if possible.  
- Increased highest max value from 50 to 100.  
- Now only landmines and spike traps are enabled by default and other hazards are opt-in for higher difficulty if desired.  
- Removed experimental BrutalCompanyMinus compatibility patch as it was not needed.  
- Min/Max values are now sliders in the config.  
- Updated README.

v1.1.3
======
- Added compatibility patch for LategameUpgrades -> Shopping carts are now blocked from spawning outside.
- Added experimental fix for BrutalCompanyMinus. Might revert this one if it has no effect.
- Fixed issue where min/max config values for custom hazards were not applied.

v1.1.2
======
- Fixed max number being excluded causing no turrets to spawn by default. This closes [issue #1](https://github.com/snaketech-tu/LCHazardsOutside/issues/1).

v1.1.1
======
- Cleared up some confusion about moon name format in the config.
- Fixed issue where moon names were expected to be lowercased otherwise they would not be recognized.
- Further improved modded moon support.
- Decreased turret safety area sizes a bit.

v1.1.0
======
- Added a new configuration using a moon list per hazard.
- Added a global outside hazard spawn chance (0-100%) to the configuration.
- Added modded moon support.
- Reworked spawning objects so that they spawn more reliably.
- Fixed wonky rotation on some slopes. 
- Refactored code to make more spawn strategies possible.
- Updated README.

v1.0.2
======
- Fixed slightly levitating hazards.
- Added rotation data to hazards according to ground slope angle. Does not yet work 100% of the time.
- Fixed a spawn issue when the main entrance and the ship had a considerable height difference like on Titan. The spread should be considerably better. 
- Fixed hazards spawning on water or other non-walkable objects.
- Updated README with known issues and upcoming features.

v1.0.1
======
- Fixed hazards spawning in mid-air by improving the spawn method.
- Increased default minimum spawn rate for landmines to 5.
- Updated README.

v1.0.0
======
- Initial release.