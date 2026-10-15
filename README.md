# IT-Kamianets 3D Engine: Unity XR

`3d-unity-xr` provides reusable XR functionality for Unity applications built on the **IT-Kamianets 3D Engine**.

Its initial target is **Meta Quest**, while the architecture should remain suitable for additional XR platforms later.

### Logic

This package brings the generic interaction capabilities of the engine into VR/MR.

It should handle **how users interact with a 3D world through XR**, while physical-room understanding and room scanning belong to `3d-unity-spatial`.

### Status

Version 0.1, vendor-neutral: no Meta SDK and no XR plug-in inside. The consuming project picks the runtime (Temnorid Arena: OpenXR with Meta Quest support, turned on only for its Quest build); the rig reads the headset and controllers through `3d-unity-input`.

* `XRCharacterRig`: the VR view of a `CharacterMotor`. Builds an XR origin at the scene root with the headset as the main camera (the scene's previous main camera is switched off meanwhile) and two controller anchors (aim poses, `TryGetHandRay`). The rig follows the motor; the player's own steps in the room come back as `HeadOffset`, which the controls walk the body after, so walls still block (a head the body can't reach is held back to a 0.3 m leash). The player's standing height maps to the character's eyes (`Recenter`), real crouching lowers the view, a crouched body keeps the view under the ceiling. Snap turn (30°) or smooth turn around the head (`XRComfort`), a teleport of the body (respawn, blink) re-faces the rig, the local body draws only its shadow, the view follows the body with a 25 ms smoothing that hides a 60 Hz network tick on a 72–120 Hz headset. Tells the scene's UI module its tracking origin, so controllers point at world-space canvases.
* `XRComfort`: snap or smooth turning, snap angle, turn speed, comfort vignette on/off and strength; saved to `EngineStorage` (`xr.comfort`).
* `ComfortVignette`: darkens the edges of the view while moving with the stick (a uGUI image on the camera, so no extra shader in builds).
* `XRModeSwitch`: enables the VR set of components (rig, VR controls) while `XRStatus.IsActive` and the flat set otherwise; disabled, it turns both off, so as an owner-only component of a networked player it leaves other copies without controls.
* `XRHeadDriver`: for a scene camera without a rig (behind a main menu): the camera follows the headset from where it stood, both controllers show a pointer ray, and the UI module gets the matching tracking origin.
* `XRVisuals`: controller markers, pointer rays, setting the UI tracking origin.
* `XRStatus.IsActive`: a headset is running, or a simulated one (the XR Interaction Toolkit's simulator) is connected; game code picks VR or flat layouts from it.
* `XRPrimaryButtonAction`: fires once per press of an `EngineInput` action (default `Player/Jump`, the right controller's A). Reads the action, never the device, so it also answers to Space or a gamepad on PC. Doesn't know what the button does.
* `StatusHud`: world-space text (`SetStatus`); optionally follows the view, level and eased, instead of being fixed to the head.
* `XRStatus`: whether a headset is active, the XR plug-in and the headset model.

Checked in Play mode: no headset reported in the Editor; a virtual gamepad's A press fires `XRPrimaryButtonAction` once. With simulated headset and controllers in Temnorid Arena: the view sits at the mage's eyes (1.70 m), a 0.5 m step in the room walks the body after the head, real crouching crouches, a snap turn turns 30° once per push, walking goes where the head looks with the head carried along, the origin stays still to the millimetre while standing, menus and the wrist panel are clicked with the controller ray.

```csharp
var rig = player.AddComponent<XRCharacterRig>();      // headset camera + hands on the character
if (rig.TryGetHandRay(HandSide.Right, out var ray)) { /* world-space aim */ }
XRComfort.Current.SnapTurn = false; XRComfort.Current.Save();
```

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
