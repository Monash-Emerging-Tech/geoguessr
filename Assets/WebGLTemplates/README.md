# MNET Geoguessr WebGL Template with MazeMaps Integration

This custom WebGL template integrates MazeMaps for campus-based geoguessr gameplay at Monash University.

## Features

- **MazeMaps Integration**: Interactive campus map with click-to-guess functionality
- **Unity-JavaScript Communication**: Seamless data flow between Unity and web interface
- **Scoring System**: Distance-based scoring with configurable parameters
- **Marker System**: Visual markers for guess and actual locations
- **Responsive UI**: Clean, modern interface with map controls
- **Round Management**: Multi-round gameplay with timer support

## Setup Instructions

### 1. Unity Project Setup

1. **Add Scripts to Project**:
   - Copy the provided C# scripts to your `Assets/Scripts/` folder:
     - `MapInteractionManager.cs` - Handles Unity-JavaScript communication
     - `GeoguessrGameManager.cs` - Main game logic and round management
     - `MapUIController.cs` - UI controls for map interaction

2. **Configure WebGL Build Settings**:
   - Go to `File > Build Settings`
   - Select `WebGL` platform
   - Click `Player Settings`
   - In `Publishing Settings`, set:
     - `Compression Format`: `Disabled` (for easier debugging)
     - `Data Caching`: `Enabled`
   - In `Resolution and Presentation`, set:
     - `Default Canvas Width`: `1920`
     - `Default Canvas Height`: `1080`
     - `Run In Background`: `Enabled`

3. **Set Custom WebGL Template**:
   - In `Player Settings > Publishing Settings`
   - Set `WebGL Template` to `Custom`
   - Set `WebGL Template` path to `Assets/WebGLTemplate/geoguessrTemplate`

### 2. Scene Setup

1. **Create Game Objects**:
   - Create an empty GameObject named "MapInteractionManager"
   - Add the `MapInteractionManager` script to it
   - Create an empty GameObject named "GeoguessrGameManager"
   - Add the `GeoguessrGameManager` script to it
   - Create an empty GameObject named "MapUIController"
   - Add the `MapUIController` script to it

2. **Configure Script References**:
   - In `GeoguessrGameManager`, assign the `MapInteractionManager` reference
   - In `GeoguessrGameManager`, assign the `LocationManager` reference (if you have one)
   - In `MapUIController`, assign UI element references (buttons, text components)

3. **UI Setup**:
   - Create UI Canvas with the following elements:
     - Map Button (to open/close map)
     - Submit Guess Button (to submit current guess)
     - Score Text (to display current score)
     - Round Text (to display current round)
     - Timer Text (to display time remaining)
     - Guess Info Text (to display guess coordinates)

### 3. Build and Deploy

1. **Build WebGL**:
   - Go to `File > Build Settings`
   - Click `Build` and select your output directory
   - Wait for the build to complete

2. **Test Locally**:
   - Serve the built files using a local web server (required for WebGL)
   - You can use Python: `python -m http.server 8000` in the build directory
   - Or use any other local server solution

3. **Deploy**:
   - Upload the built files to your web server
   - Ensure the server supports the required MIME types for WebGL

## Usage

### Basic Gameplay Flow

1. **Start Game**: The game automatically starts with the first round
2. **View Location**: Player sees a 360° image or scene
3. **Open Map**: Player clicks "Open Map" button to access MazeMaps
4. **Make Guess**: Player clicks on the map to place their guess
5. **Submit Guess**: Player clicks "Submit Guess" to confirm
6. **View Results**: Game shows actual location and calculates score
7. **Next Round**: Process repeats for remaining rounds

### JavaScript API

The template provides several JavaScript functions for Unity communication:

```javascript
// Show the map interface
showMapFromUnity();

// Hide the map interface
hideMapFromUnity();

// Add a marker to the map
addMarkerFromUnity(latitude, longitude, label, type);

// Update score display
updateScoreFromUnity(score, round);

// Show/hide loading indicator
showLoading(true/false);
```

### Unity C# API

The C# scripts provide these key methods:

```csharp
// MapInteractionManager
mapManager.ShowMap();
mapManager.HideMap();
mapManager.SetActualLocation(lat, lng);
mapManager.UpdateScoreDisplay(score, round);

// GeoguessrGameManager
gameManager.StartRound();
gameManager.EndRound();
gameManager.RestartGame();
```

## Configuration

### MapInteractionManager Settings

- `maxGuessDistance`: Maximum distance for scoring (meters)
- `maxScore`: Maximum possible score per round
- `minScore`: Minimum possible score per round
- `scoreCurve`: Animation curve for score calculation
- `enableDebugLogs`: Enable debug logging

### GeoguessrGameManager Settings

- `totalRounds`: Number of rounds in the game
- `roundTimeLimit`: Time limit per round (seconds)
- `enableTimer`: Whether to show timer
- `enableDebugLogs`: Enable debug logging

## Troubleshooting

### Common Issues

1. **Map Not Loading**:
   - Check internet connection (MazeMaps requires online access)
   - Verify MazeMaps API is accessible
   - Check browser console for JavaScript errors

2. **Unity-JavaScript Communication Not Working**:
   - Ensure WebGL build is used (not Editor)
   - Check that function names match exactly
   - Verify Unity instance is properly initialized

3. **Markers Not Appearing**:
   - Check that coordinates are valid
   - Verify MazeMap instance is loaded
   - Check browser console for errors

4. **Scoring Issues**:
   - Verify actual location is set before guess submission
   - Check that coordinates are in correct format (lat, lng)
   - Ensure distance calculation is working properly

### Debug Tips

1. **Enable Debug Logs**: Set `enableDebugLogs = true` in all scripts
2. **Check Browser Console**: Look for JavaScript errors and Unity logs
3. **Test Coordinates**: Use known coordinates to verify map functionality
4. **Verify Build Settings**: Ensure WebGL template is properly configured

## File Structure

```
Assets/WebGLTemplate/geoguessrTemplate/
├── index.html              # Main HTML template
└── README.md              # This file

Assets/Scripts/
├── MapInteractionManager.cs    # Unity-JavaScript bridge
├── GeoguessrGameManager.cs     # Main game logic
└── MapUIController.cs          # UI controls
```

## Dependencies

- **Unity WebGL**: For web deployment
- **MazeMaps API**: For campus mapping (loaded from CDN)
- **Modern Browser**: With WebGL support
- **Local Web Server**: For testing (WebGL requires HTTP/HTTPS)

## License

This template is part of the MNET Geoguessr project at Monash University.
