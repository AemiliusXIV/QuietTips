# Quiet Tips

A Dalamud plugin for FFXIV that keeps tooltips out of your way. Hide action, item,
pop-up and crossbar hint tooltips on your own terms, and get them back when you
actually need them.

## Installation

1. In-game, open **ESC → Dalamud Settings → Experimental → Custom Plugin Repositories** and add:
   ```
   https://aemiliusxiv.github.io/DalamudPlugins/pluginmaster.json
   ```
2. Open the Plugin Installer (`/xlplugins`) and search for **Quiet Tips**.

Quiet Tips is part of the [AemiliusXIV plugin repository](https://github.com/AemiliusXIV/DalamudPlugins). Visit that page for an overview of all available plugins.

## What it does

Each tooltip type can be hidden separately for **in combat** and **out of combat**,
so you can, say, suppress the action and item popups while you're just clicking around
town but keep them during a fight (or the other way round).

On top of that:

- **Duty override**: while you're inside an instance you can force tooltips to always
  show, always hide, or just keep following the combat rules.
- **Inventory exception**: whenever an Inventory, Armoury Chest or Character window is
  open, hidden tooltips come back automatically so you can still inspect gear. This
  always takes priority.

Type `/quiettips` to open the settings.

## How it works

Rather than intercepting anything, the plugin flips the game's own UI options
(`Display Action Help`, `Display Item Help`, `Pop-up Help`, `Cross Hotbar hints`) to
match your rules as combat, duty and menu state change. Your original settings are
captured on login and restored when the plugin is disabled or unloaded.

## Heads up

If you also run Simple Tweaks' "Hide Tooltips in Combat", both plugins write the same
game options and will fight over them. Pick one.

## Privacy

Quiet Tips runs entirely on your machine. It reads and writes the game's own tooltip display
options and watches which inventory-type windows are open. Nothing is collected, stored off your
PC, or sent over the network, and there is no telemetry.

## License

Copyright (c) 2026 AemiliusXIV

This project is source-available. You may fork and modify it, but the source code may not be copied into other projects or plugins, in source or compiled form, without explicit written permission. Forks must preserve this license and credit the original author. See the [LICENSE](LICENSE) file for full terms.

This project is not affiliated with or endorsed by Square Enix Co., Ltd. The use of third-party tools is prohibited under the FINAL FANTASY XIV Terms of Service; use of this plugin is entirely at your own risk. FINAL FANTASY XIV is a registered trademark of Square Enix Holdings Co., Ltd.
