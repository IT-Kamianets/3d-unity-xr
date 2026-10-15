# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `XRCharacterRig` (headset camera and hands on a `CharacterMotor`, room-scale steps, eye height, snap/smooth turn, teleport re-facing, body hidden, view smoothing, UI tracking origin), `XRComfort`, `ComfortVignette`, `XRModeSwitch`, `XRHeadDriver`, `XRVisuals`, `XRStatus.IsActive`.
- Depends on `3d-unity-character` and the Input System.

### Added

- `StatusHud` (world-space status text; can follow the view), `XRStatus`.

### Changed

- `XRPrimaryButtonAction` reads an `EngineInput` action (default `Player/Jump`) instead of the XR device; its serialized `controllerNode` field is replaced by `action`.
