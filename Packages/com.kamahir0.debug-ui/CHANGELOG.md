# Changelog

## [Unreleased]

- Declare UI Toolkit and native Vector Graphics module dependencies; require Unity 6.3 or later.
- Make repeated initialization safe and add `DebugMenu.Shutdown()`.
- Release menu state when its GameObject is destroyed and when entering Play Mode without Domain Reload.
- Correct package links and add installation, lifecycle, and sample instructions.
- Add a Basic Debug Menu sample and Unity regression tests using a separate Git consumer project.
- Keep navigation stable when revisiting the current page and release temporary pages after returning.
- Prevent duplicate open buttons created in the same frame; support keyboard submission on toggle items.
- Adapt menu width, scaling, labels, and horizontal groups to compact viewports.
- Correct progress bar height and dark theme focus/hover text contrast.
- Constrain restored positions and resized menus to the viewport; remove position callbacks on detach.
- Add a development validation scene and regression coverage for controls, navigation, and layout.

## [0.1.0]

- Initial standalone Unity package with runtime debug pages and a linked Editor window.
