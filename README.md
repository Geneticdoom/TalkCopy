# TalkCopy

A plugin to send various in game dialogue boxes to the clipboard or websocket. Useful for language learning purposes.

#### WebSocket Configuration
- **Port**: Default is 8766, configurable in settings
- **URL**: `ws://localhost:8766` (or your configured port)

#### Testing WebSocket Mode
1. Enable WebSocket mode in the plugin settings
2. Open the provided `websocket_test_client.html` in a web browser
3. Connect to the WebSocket server using the test client
4. Trigger text copying in the game
5. Watch the text appear in real-time in the test client

## Installation
You will need the .NET SDK installed to build this repo.
Download this repo and build it using this command:

```bash
dotnet build
```
Within dalamud add select the dev plugin dll and choose this one then save.
