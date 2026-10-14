# IT-Kamianets 3D Engine: Unity XR

`3d-unity-xr` provides reusable XR functionality for Unity applications built on the **IT-Kamianets 3D Engine**.

Its initial target is **Meta Quest**, while the architecture should remain suitable for additional XR platforms later.

### Logic

This package brings the generic interaction capabilities of the engine into VR/MR.

It should handle **how users interact with a 3D world through XR**, while physical-room understanding and room scanning belong to `3d-unity-spatial`.

### Status

Version 0.1, without Meta SDKs (Quest-specific features come with Meta XR Core / OpenXR in the consuming project).

* `XRPrimaryButtonAction`: fires once per press of an `EngineInput` action (default `Player/Jump`, the right controller's A). Reads the action, never the device, so it also answers to Space or a gamepad on PC. Doesn't know what the button does.
* `StatusHud`: world-space text (`SetStatus`); optionally follows the view, level and eased, instead of being fixed to the head.
* `XRStatus`: whether a headset is active, the XR plug-in and the headset model.

Checked in Play mode: no headset reported in the Editor; a virtual gamepad's A press fires `XRPrimaryButtonAction` once.

### Roadmap

* OpenXR foundation
* Meta Quest support
* VR controllers
* Controller input
* Hand tracking
* Hand interactions
* Object grabbing
* Object manipulation
* Ray interaction
* Direct interaction
* Teleportation
* Player locomotion
* XR UI
* VR editing tools
* Integration with `3d-unity-interaction`
* Meta Quest performance optimization
* Stable Quest release
* Additional XR platform support
