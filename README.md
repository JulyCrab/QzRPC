# QzRPC

A Discord Rich Presence client for Qobuz that displays your current listening activity with album art support.

## Features

- Real-time display of currently playing tracks on Discord
- Automatic album art fetching from Last.fm and iTunes APIs
- Elapsed time tracking for songs
- Idle state detection after 60 seconds of inactivity
- Persistent settings storage
- Self-contained executable (no .NET installation required)

## Requirements

- Windows 10/11 (x64)
- Qobuz Desktop App
- Discord Desktop App

## Setup

### Discord Application Setup

You'll need to create a Discord application to use Rich Presence. If you want to customize the image and name, follow these steps:

1. Visit the [Discord Developer Portal](https://discord.com/developers/applications)
2. Click "New Application" and give it a name (e.g., "Qobuz")
3. Navigate to the "General Information" tab and copy your Application ID
4. (Optional) Go to "Rich Presence" → "Art Assets" and upload any image you want, then reference it in the code

Alternatively, you can use the default Discord Application ID: `1490007914461790399`

### Last.fm API Key (Optional)

For album art support, you can register for a free Last.fm API key:

1. Go to the [Last.fm API page](https://www.last.fm/api/account/create)
2. Fill out the application form
3. Copy your API Key once approved

### Running the Application

1. Download `QzRPC.exe` from the releases page
2. Launch the application
3. Enter your Discord Application ID
4. (Optional) Enter your Last.fm API Key for album art
5. Click "Start"
6. Open Qobuz and start playing music

Your settings are automatically saved for future sessions.

## How It Works

The application monitors your Qobuz desktop client and updates your Discord status accordingly:

- **Playing**: Displays track name, artist, album art, and elapsed time
- **Paused**: Shows the paused track with a pause indicator
- **Idle**: Activates after 60 seconds of inactivity or when no track is playing

## Building from Source

The project includes automated build scripts for easy compilation:

```bash
cd QobuzRPC
.\build.ps1
```

The output is a self-contained executable at `dist/QzRPC.exe` that includes all dependencies.

For more build options and metadata management, see [BUILD.md](BUILD.md).

### Manual Build

```bash
dotnet publish QzRPC.csproj -c Release -r win-x64 -p:PublishSingleFile=true --output dist
```

## Troubleshooting

**Discord status not updating**
- Verify Discord is running and you're logged in
- Double-check your Discord Application ID
- Try restarting both Qobuz and QzRPC

**Album art not displaying**
- Ensure you've entered a valid Last.fm API key
- Some tracks may not have artwork available in the Last.fm/iTunes databases
- The fallback Qobuz logo will display if no artwork is found

**Application won't launch**
- Ensure Qobuz Desktop is installed
- Try running as administrator
- Check Windows Defender or antivirus isn't blocking the executable

## Technical Details

Built with:
- C# / .NET 10
- Avalonia UI (cross-platform UI framework)
- SukiUI (modern theme library)
- DiscordRichPresence library
- Last.fm and iTunes Search APIs

## License

MIT License - See LICENSE file for details.

Created by JulyCrab
