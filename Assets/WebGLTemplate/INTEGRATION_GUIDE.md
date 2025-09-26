# MNET Geoguessr MazeMaps Integration Guide

## Quick Start

### Manual Setup
Follow the detailed instructions in `README.md` for step-by-step manual configuration. This involves:

1. Adding the required scripts to GameObjects in your scene
2. Configuring script references and UI elements
3. Setting up the WebGL template
4. Building and testing your WebGL application

## Key Components

### JavaScript Side (WebGL Template)
- **`index.html`**: Main template with MazeMaps integration
- **Map Initialization**: Loads MazeMaps API and creates map instance
- **Click Handling**: Captures map clicks and sends coordinates to Unity
- **Marker System**: Displays guess and actual location markers
- **UI Controls**: Map controls, score display, and round information

### Unity C# Side
- **`GameLogic.cs`**: Main game logic, round management, and scoring system
- **`LocationManager.cs`**: Manages location data, map packs, and skybox materials
- **`MapInteractionManager.cs`**: Handles Unity-JavaScript communication and map interactions
- **`MapUIController.cs`**: UI controls for map interaction, scaling, and button management
- **`GuessButtonManager.cs`**: Manages guess button states and interactions
- **`DebugLogScript.cs`**: Displays game state information for debugging

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

### GameLogic
- `totalRounds`: Number of rounds in the game
- `mapPackId`: ID of the map pack to use for locations
- `mapManager`: Reference to MapInteractionManager
- `locationManager`: Reference to LocationManager
- `gameUI`: Reference to main game UI GameObject
- `mapUI`: Reference to map UI GameObject
- `resultsUI`: Reference to results UI GameObject

### LocationManager
- `jsonResourcePath`: Path to location data JSON file in Resources folder (default: "locationData")

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
// GameLogic
gameLogic.LoadGame()                  // Load game scene
gameLogic.RestartGame()               // Restart entire game
gameLogic.submitGuess()               // Submit current guess
gameLogic.GetCurrentScore()           // Get current score
gameLogic.GetCurrentRound()           // Get current round
gameLogic.GetLocationManager()        // Get LocationManager instance

// MapInteractionManager
mapManager.ShowMap()                  // Show map
mapManager.HideMap()                  // Hide map
mapManager.SetActualLocation(lat, lng) // Set actual location
mapManager.UpdateScoreDisplay(score, round) // Update score display
mapManager.ResetRound()               // Reset round data

// LocationManager
locationManager.Start()               // Initialize and load data
locationManager.SelectRandomLocation() // Select random location from current map pack
locationManager.SetCurrentMapPack(id) // Set current map pack by ID
locationManager.GetCurrentLocation()  // Get current location
```

## File Structure
```
Assets/WebGLTemplate/geoguessrTemplate/
├── index.html              # Main WebGL template
├── README.md              # Detailed setup instructions
└── INTEGRATION_GUIDE.md   # This quick reference

Assets/Scripts/
├── GameLogic.cs                # Main game logic and round management
├── LocationManager.cs          # Location data and map pack management
├── MapInteractionManager.cs    # Unity-JavaScript bridge
├── MapUIController.cs          # Map UI controls and scaling
├── GuessButtonManager.cs       # Guess button state management
├── DebugLogScript.cs           # Debug information display
└── ScoreBannerController.cs    # Score and round display

Assets/Prefabs/
├── GameLogic.prefab            # Main game logic prefab
├── LocationManager.prefab      # Location manager prefab
├── MapInteractionManager.prefab # Map interaction manager prefab
├── minimap.prefab              # Minimap UI prefab
├── score-banner.prefab         # Score display prefab
└── Debug Log.prefab            # Debug log prefab

Assets/Resources/
└── locationData.json           # Location and map pack data
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
