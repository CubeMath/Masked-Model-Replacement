# Masked-Model-Replacement
When a masked enemy spawns from a vent, a random suit gets applied. This mod has ModelReplacementAPI support.

Supports ModelReplacementAPI mod.

# Configuration
You can use the config file to filter the suits for the masked enemies.

## Log Available Suits
Log the unlockable names of the available suits into the console when you spawn in the ship.
Names will be written in lowercase and spaces will be removed.

Useful to find out what the names of the suits are for the **Masked Ignore Suits** configuration.

## Model Replacements Only
Masked enemies should only pick model replacements and ignore all suits that does not have a model replacement.

The ModelReplacementAPI is a soft dependecy.
When this configuration is enabled, the masked will only pick suits with a custom model.

## Masked Ignore Suits
Comma separated list of suits that the masked enemy should be excluded.
Example: "default, greensuit, hazardsuit, pajamasuit, purplesuit, beesuit"

Use the **Log Available Suits** config to find out what the names of the suits are.

## Preferred Suits per Moon
Comma separated list of suits that the masked enemy should wear for specific moons.
This setting will overwrite the "Masked Ignore Suits" configuration if entries matches for the current moon.
Example: "assurance: greensuit hazardsuit pajamasuit,offense: purplesuit beesuit bunnysuit"

# Thank you Mirage developers
They added an option to turn off the ability to disable the *Copy masked visuals* on masked enemies.
Masked-Model-Replacement will no longer break if you disable that option on Mirage.
