# MNET Geoguessr MazeMaps Integration Guide

## Quick Start

### 1. Automatic Setup (Recommended)
1. Add the `ExampleSceneSetup.cs` script to any GameObject in your scene
2. In the Inspector, click "Setup Game" or let it run automatically on Start
3. Click "Validate Setup" to ensure everything is configured correctly
4. Build for WebGL using the custom template

### 2. Manual Setup
Follow the detailed instructions in `README.md` for step-by-step manual configuration.

## Key Components

### JavaScript Side (WebGL Template)
- **`index.html`**: Main template with MazeMaps integration
- **Map Initialization**: Loads MazeMaps API and creates map instance
- **Click Handling**: Captures map clicks and sends coordinates to Unity
- **Marker System**: Displays guess and actual location markers
- **UI Controls**: Map controls, score display, and round information

### Unity C# Side
- **`MapInteractionManager.cs`**: Handles Unity-JavaScript communication
- **`GeoguessrGameManager.cs`**: Main game logic and round management
- **`MapUIController.cs`**: UI controls for map interaction
- **`ExampleSceneSetup.cs`**: Automated setup and validation

## Data Flow

```
Player clicks map → JavaScript captures event → SendMessage to Unity
Unity receives coords → CalculateScore() → PlaceMarkerOnMap()
Unity sends marker data → JavaScript renders markers on MazeMap
```

## Testing Checklist

### Before Building
- [ ] All scripts added to project
- [ ] WebGL template set to custom
- [ ] Scene setup validated
- [ ] UI elements created and assigned

### After Building
- [ ] WebGL build loads without errors
- [ ] Map opens when "Open Map" clicked
- [ ] Map clicks register and send coordinates to Unity
- [ ] Markers appear on map for guess and actual locations
- [ ] Score calculation works correctly
- [ ] Round progression functions properly

### Common Issues
1. **Map not loading**: Check internet connection and MazeMaps API access
2. **Unity-JS communication failing**: Verify WebGL build and function names
3. **Markers not appearing**: Check coordinate format and map instance
4. **Scoring issues**: Ensure actual location is set before guess submission

## Configuration Options

### MapInteractionManager
- `maxGuessDistance`: Maximum distance for scoring (meters)
- `maxScore`: Maximum possible score per round
- `minScore`: Minimum possible score per round
- `scoreCurve`: Animation curve for score calculation

### GeoguessrGameManager
- `totalRounds`: Number of rounds in the game
- `roundTimeLimit`: Time limit per round (seconds)
- `enableTimer`: Whether to show timer

## API Reference

### JavaScript Functions (Called from Unity)
```javascript
showMapFromUnity()                    // Show map interface
hideMapFromUnity()                    // Hide map interface
addMarkerFromUnity(lat, lng, label, type)  // Add marker to map
updateScoreFromUnity(score, round)    // Update score display
showLoading(show)                     // Show/hide loading indicator
```

### Unity C# Methods
```csharp
// MapInteractionManager
mapManager.ShowMap()                  // Show map
mapManager.HideMap()                  // Hide map
mapManager.SetActualLocation(lat, lng) // Set actual location
mapManager.UpdateScoreDisplay(score, round) // Update score display

// GeoguessrGameManager
gameManager.StartRound()              // Start new round
gameManager.EndRound()                // End current round
gameManager.RestartGame()             // Restart entire game
```

## File Structure
```
Assets/WebGLTemplate/geoguessrTemplate/
├── index.html              # Main WebGL template
├── README.md              # Detailed setup instructions
└── INTEGRATION_GUIDE.md   # This quick reference

Assets/Scripts/
├── MapInteractionManager.cs    # Unity-JavaScript bridge
├── GeoguessrGameManager.cs     # Main game logic
├── MapUIController.cs          # UI controls
└── ExampleSceneSetup.cs        # Automated setup
```

## Support

For issues or questions:
1. Check the troubleshooting section in `README.md`
2. Enable debug logs in all scripts
3. Check browser console for JavaScript errors
4. Verify Unity console for C# errors
5. Test with known coordinates to isolate issues

## Next Steps

1. **Customize UI**: Modify the HTML/CSS in the WebGL template
2. **Add Features**: Extend the scoring system or add new game modes
3. **Optimize Performance**: Implement object pooling for markers
4. **Add Analytics**: Track player performance and map usage
5. **Mobile Support**: Optimize for mobile devices and touch controls
