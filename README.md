# WOL Manager

A Windows desktop tool for managing Wake-on-LAN and remote access to computers on your local network.

## Features

- **Wake on LAN** — sends magic packets through all active network interfaces (multi-homed support)
- **Remote Power** — Restart and Shutdown remote Windows computers
- **RDP** — launch Remote Desktop Connection with one click
- **SSH** — open SSH session in Windows Terminal or cmd
- **Network Share** — browse shared folders in Explorer
- **Info / Port Scan** — ping target, detect OS from TTL, scan 20 common TCP ports in parallel
- **Network Scanner** — auto-discover Windows computers on LAN (ping + ARP + hostname resolution)
- **System Tray** — minimize to tray, balloon notification when Wake-on-LAN target comes online
- **Notes** — per-computer notes field (role, location, owner)
- **Config** — stored in `%APPDATA%\WOLManager\computers.json`

## Requirements

- Windows 10 / 11
- [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)
- OpenSSH Client (optional, for SSH feature) — Settings → Apps → Optional features

## Installation

Download `WOLManager-Setup-1.0.exe` from [Releases](../../releases) and run it.  
The installer requires admin rights (needed to place files in Program Files).

## Building from source

```
git clone https://github.com/toevi/WOLManager.git
cd WOLManager
dotnet build -c Release
```

## Usage

1. Add computers manually (**Add**) or auto-discover via **Add Scan**
2. Select a computer in the list
3. Use the buttons on the right panel:
   - **Wake Up** — sends WOL magic packet (requires MAC address)
   - **Restart / Shutdown** — remote power commands (requires admin share access)
   - **RDP** — opens mstsc.exe
   - **SSH** — opens terminal with `ssh [user@]host`
   - **Network Share** — opens `\\hostname` in Explorer
   - **Info / Port Scan** — shows ping, OS hint, and open ports

### Wake on LAN tips

- MAC address is required (format `AA:BB:CC:DD:EE:FF`)
- Target computer must have WOL enabled in BIOS/UEFI and network adapter settings
- Works on the same LAN segment; for cross-subnet WOL configure directed broadcast on your router

## Security

- All external processes use `ProcessStartInfo.ArgumentList` — no shell argument injection
- Network operations use .NET socket APIs directly (no shell commands for ping/port scan)
- Config file location: `%APPDATA%\WOLManager\computers.json` (user profile, not Program Files)

## License

[MIT](LICENSE) — © 2025 tmfgroup
