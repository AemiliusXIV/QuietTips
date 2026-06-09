# Security

## Reporting a vulnerability

Found a security issue? Please open a private report through GitHub's Security advisories
("Report a vulnerability" on the repo's Security tab) rather than a public issue. Include what
you found and how to reproduce it. I'll respond as soon as I can.

## What the plugin can access

- Reads and writes FFXIV's own tooltip display options (the same settings found under Character
  Configuration), and watches for inventory-type windows opening and closing. All of this stays
  local to your game client.
- No network access. Nothing is sent anywhere, and there is no telemetry.

## Secrets

No API keys, tokens or client secrets are committed to this repository or used by the plugin.
