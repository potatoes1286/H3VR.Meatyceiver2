# H3VR.Meatyceiver2
H3VR mod to add jamming into the game!

Current released version: 0.2.3

# How To Install

Download the release version from [BoneTome](https://bonetome.com/h3vr/weapons/171/) and drop it in H3VR/BepInEx/plugins.

# Dependencies

[BepInEx](https://github.com/BepInEx/BepInEx/releases)

[Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases)

# Current Features

Meatyceiver 2 currently features 5 different failures in three catagories, and have 3 extra unprogrammed failures.

## Failures

### Ammunition Failures

Ammunition failures are failures that are caused by poor ammunition quality.

1. Light Primer Strikes
1. (Unfinished) Hangfire

### Firearm Failures

Firearm failures are standard failures that can occur in even properly working firearms, if rarely.

1. Failure to Feed
1. Failure to Extract
1. (Unfinished) Stovepipe
1. (Unfinished) Double feed

### Broken Firearm Failures

Broken firearm failures are caused by the firearm being mechanically broken / worn down.

1. Hammer Follow
1. Slam Fire

# Configuration

## General Settings

### Enable Ammunition Failures

Enable Ammunition Failures, as you may assume, enables all types of Ammunition Failures listed above, being

1. Light Primer Strikes
1. (Unfinished) Hangfire

### Enable Firearm Failures

Same with Ammunition Failures, this enables all Firearm Failures, being

1. Failure to Feed
1. Failure to Extract
1. (Unfinished) Stovepipe
1. (Unfinished) Double feed

### Enable Broken Firearm Failures

Same with Ammunition Failures, this enables all Broken Firearm Failures, being

1. Hammer Follow
1. Slam Fire

### Enable Secondary Failure Multipliers

This enables extra code to affect the regular percentage chance for guns to jam.

The only one currently implemented is the increased chance for a Failure To Feed with higher capacity magazines.

### Enable Console Debugging

This displays the RNG number and percentage chance it would've failed after *every* chance for a gun to jam, which means a regular rifle will display five of these every shot, and displays when it jams. Generally avoid turning this on, it spams your console.

## Multipliers

### Failure Chance Multiplier

This is a simple multiplier multiplying every single percentage chance of failure by this amount.

### Additional Failure Chance Per Round

Meatyceiver 2 reads the capacity of the magazine inserted into a gun, and for every round capacity above Minimum Mag Count, it increases the chance of an FTF by this much. (Belt feds are exempt from this.)

### Minimum Mag Count

See above.

## Failures

All the failure ones are just the percentage chance for it to happen when it can. Meatyceiver 2's random numbers go to 2 decimal places, so differences smaller than 0.01 will not change anything.
