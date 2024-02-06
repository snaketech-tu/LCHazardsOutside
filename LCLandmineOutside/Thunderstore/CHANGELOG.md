v1.1.1
======
- Cleared up some confusion about moon name format in the config.
- Fixed issue where moon names were expected to be lowercased otherwise they would not be recognized.
- Further improved modded moon support.

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