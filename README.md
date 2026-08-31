> DEPRECATED! Migrated to https://codeberg.org/Haaxor1689/cinematic-camera-mod

# Cinematic Camera

![Tooltip](/assets/tooltip.png)

A gameplay mod for Allumeria that adds **Camera** item that can be used to set up panning shots.

![Previe](/assets/camera.gif)

## How to Install

1. Install Ignitron mod loader (skip if you already did)
   - Create `/mods` folder inside the game directory
     > Default location: `C:\Program Files (x86)\Steam\steamapps\common\Allumeria`
   - Download the latest [Ignitron.Loader.zip](https://allumeria-db.haaxor1689.dev/api/ignitron-loader)
   - Extract the zip into the `/mods` folder directly (not into another folder within)
1. Download the latest [cinematic-camera-mod.zip](https://github.com/haaxor1689/cinematic-camera-mod/releases/latest/download/cinematic-camera-mod.zip)
1. Put the zip (not extracted) into the `/mods` folder in the game files

## Acquisition

The Camera is sold by the Merchant NPC for 10 silver tokens.

## Features

Use the camera item and its radial menu to set up waypoints the shot should follow.

Each waypoint is represented by a floating camera model and the nearest waypoint will be highlighted in green.

There is also a preview of the path the camera will take. The steepness of the curve and the color (going from green to red) depends on the set speed.

UI is automatically hidden while the camera plays and the view is reset back to your position when it ends.

The camera waypoints and path preview are only visible to you while you hold the camera item.

![Path preview](/assets/preview.png)

While holding the Camera, **hold R** to bring up the radial menu with options.

- **Add** new camera waypoints at your current location and orientation.
- **Remove** the nearest waypoint or **Move** it to your current location and orientation.
- **Increase or decrease the speed** of the camera.
- Select **Play** to watch the shot. You can always go back and adjust.
- **Clear all** once you are done or want to start from scratch.

![Camera features](/assets/radial.png)
