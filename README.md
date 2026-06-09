# Quiet Tips

A Dalamud plugin for FFXIV that keeps tooltips out of your way. Hide action, item,
pop-up and crossbar hint tooltips on your own terms, and get them back when you
actually need them.

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

## Building

```
dotnet build -c Release
```

The build copies the output into `%AppData%\XIVLauncher\customPlugins\QuietTips`
for dev loading.

## Privacy

Quiet Tips runs entirely on your machine. It reads and writes the game's own tooltip display
options and watches which inventory-type windows are open. Nothing is collected, stored off your
PC, or sent over the network, and there is no telemetry.

## License

See [LICENSE](LICENSE). The source is available under a forks-only license: fork and modify it as
a standalone repository that credits the original, but the code may not be lifted into other
projects.

This plugin is not affiliated with or endorsed by Square Enix Co., Ltd. Square Enix does not permit the use of third-party plugins and this plugin may violate the FINAL FANTASY XIV Terms of Service. Use of this plugin is entirely at your own risk. FINAL FANTASY XIV is a registered trademark of Square Enix Holdings Co., Ltd.
