# TalkCopy

A Final Fantasy XIV plugin that automatically copies text from the game to your clipboard or WebSocket server.

## Features

- **Normal Mode**: Automatically copies a select number of text elements to your clipboard or WebSocket server
- **Text Copy Mode**: Allows you to copy the text of ANY text element on the screen
- **WebSocket Support**: Option to send text data via WebSocket server instead of clipboard
- **Configurable**: Extensive settings to customize what text gets copied

## Output Modes

### Clipboard Mode (Default)
Text is copied directly to your system clipboard, just like the original plugin.

### WebSocket Mode
Text is sent via WebSocket server to connected clients. This is useful for:
- Integration with other applications
- Real-time text processing
- Multi-client setups
- Automation workflows

#### WebSocket Configuration
- **Port**: Default is 8766, configurable in settings
- **URL**: `ws://localhost:8766` (or your configured port)
- **Protocol**: Standard WebSocket protocol
- **Data Format**: Plain text messages

#### Testing WebSocket Mode
1. Enable WebSocket mode in the plugin settings
2. Open the provided `websocket_test_client.html` in a web browser
3. Connect to the WebSocket server using the test client
4. Trigger text copying in the game
5. Watch the text appear in real-time in the test client

## Installation

1. Install the plugin through your XIVLauncher plugin manager
2. Configure the plugin settings via `/talkcopy settings`
3. Use `/talkcopy` to toggle the plugin on/off

## Commands

- `/talkcopy` - Toggle the plugin on/off
- `/talkcopy settings` - Open settings window
- `/talkcopy logs` - Open copy log window
- `/talkcopy help` - Show help information

## Keybinds

Default keybind to toggle between Normal and Text Copy modes:
- **Modifier 1**: Left Control
- **Modifier 2**: Left Shift  
- **Key**: Spacebar

All keybinds are configurable in the settings.

## Settings

### Global Settings
- **Remove Text Between Angled Brackets**: Strips `<Example Text>` formatting
- **Copy ANY Text**: Master toggle for all text copying

### Output Settings
- **Use WebSocket Server**: Switch between clipboard and WebSocket modes
- **WebSocket Port**: Configure the port for the WebSocket server (default: 8766)

### Normal Mode Settings
Configure which types of text elements get copied:
- Dialog boxes
- Tooltips
- Subtitles
- Battle toasts
- Area toasts
- Error messages
- Lists

### Text Copy Mode Settings
- **Show Preview Tooltip**: Display text preview when hovering
- **Key Selection Toggles Mode**: Enable/disable toggle behavior
- **Prevent Key Passthrough**: Stop keys from reaching the game
- **Show Warning Outline/Text**: Visual indicators when mode is active

## Development

### Building
```bash
dotnet build
```

### WebSocket Integration
To integrate with the WebSocket server in your own application:

```javascript
// JavaScript example
const socket = new WebSocket('ws://localhost:8766');
socket.onmessage = function(event) {
    console.log('Received text:', event.data);
};
```

```python
# Python example
import websocket

def on_message(ws, message):
    print(f"Received text: {message}")

ws = websocket.WebSocketApp("ws://localhost:8766", on_message=on_message)
ws.run_forever()
```

## License

AGPL-3.0-or-later
