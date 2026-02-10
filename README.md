# MNET Geoguessr

A Unity-based geoguessr game integrated with Monash University's MazeMaps API. Players explore 360° locations and guess their positions on an interactive campus map.

## Features

- **360° Location Exploration**: Immersive skybox-based location viewing
- **Interactive Campus Maps**: Integration with Monash MazeMaps API
- **Multi-Round Gameplay**: Configurable rounds with scoring system
- **Real-time Scoring**: Distance-based scoring with customizable curves
- **WebGL Support**: Deployable as web application
- **Debug Tools**: Comprehensive debugging and logging system

## Architecture

### Core Components

#### GameLogic.cs

- Main game controller and round management
- Singleton pattern for persistent game state
- Event system for UI updates and game progression
- Score calculation and round transitions

#### LocationManager.cs

- Manages location data from JSON resources
- Handles map pack selection by name (e.g., "all", "europe", "asia")
- Assigns skybox materials for 360° viewing
- Provides map pack validation and debugging
- MonoBehaviour-based for Unity integration

#### MapInteractionManager.cs

- Unity-JavaScript communication bridge
- Handles map clicks and coordinate processing
- Score calculation based on distance
- Singleton pattern for persistent map state

#### MapUIController.cs

- Controls map scaling and animation
- Manages map interaction UI elements
- Handles button states and user interactions
- Integrates with WebGL template overlay

#### ScoreBannerController.cs

- Dynamic score banner with automatic resizing
- Displays map pack name with overflow handling
- Balanced expansion (grows both left and right)
- Minimum width enforcement and real-time updates

## Setup Instructions

### Prerequisites

- Unity 2022.3 or later
- WebGL Build Support module
- Monash University network access (for MazeMaps API)

### Project Setup

1. **Import the project** into Unity
2. **Configure WebGL Template**:
   - Set WebGL template to custom
   - Point to `Assets/WebGLTemplate/geoguessrTemplate/`

3. **Set up Prefabs**:
   - Create LocationManager prefab with LocationManager script
   - Configure GameLogic prefab with all required references
   - Assign UI GameObjects to GameLogic prefab

4. **Configure Location Data**:
   - Ensure `locationData.json` is in `Assets/Resources/`
   - Verify location materials are in `Assets/Resources/Materials/Locations/`

### Scene Configuration

1. **Add Required GameObjects**:
   - GameLogic (with GameLogic script)
   - LocationManager (with LocationManager script)
   - MapInteractionManager (with MapInteractionManager script)
   - MapUIController (with MapUIController script)

2. **Configure UI References**:
   - Assign main Canvas to GameLogic.gameUI
   - Assign minimap prefab to GameLogic.mapUI
   - Assign results UI to GameLogic.resultsUI

3. **Set up Camera**:
   - Configure camera for skybox rendering
   - Ensure proper skybox material assignment

## Game Flow

1. **Game Initialization**:
   - LocationManager loads location data from JSON
   - GameLogic initializes round counter and scoring
   - MapInteractionManager sets up JavaScript communication

2. **Round Start**:
   - Random location selected from current map pack
   - Skybox material applied for 360° viewing
   - Map interface prepared for interaction

3. **Player Interaction**:
   - Player explores location using camera controls
   - Map opens when requested
   - Player places guess marker on campus map

4. **Guess Submission**:
   - Distance calculated between guess and actual location
   - Score calculated using configurable curve
   - Results displayed with markers

5. **Round Completion**:
   - Score added to total
   - Round counter incremented
   - Next round starts or game ends

## Configuration

### GameLogic Settings

- `totalRounds`: Number of rounds per game
- `mapPackName`: Name of map pack to use (e.g., "all", "europe", "asia")
- `enableDebugLogs`: Enable/disable debug output

### MapInteractionManager Settings

- `maxGuessDistance`: Maximum scoring distance (meters)
- `maxScore`: Maximum points per round
- `minScore`: Minimum points per round
- `scoreCurve`: Animation curve for score calculation

### LocationManager Settings

- `jsonResourcePath`: Path to location data in Resources folder

### ScoreBannerController Settings

- `enableDynamicResizing`: Enable/disable automatic banner resizing
- `mapPackText`: Text component for map pack name display
- `backgroundRect`: Background RectTransform reference
- `containerRect`: Main container RectTransform reference
- `mapTextContainerRect`: Map text container RectTransform reference

## Building and Deployment

### WebGL Build

1. **Build Settings**:
   - Platform: WebGL
   - Template: Custom (geoguessrTemplate)
   - Compression: Disabled (for development)

2. **Build Process**:
   - File → Build Settings
   - Add scenes to build
   - Click Build and select output directory

3. **Testing**:
   - Serve files using local web server
   - Test all game functionality
   - Verify MazeMaps integration

### Deployment Considerations

- Ensure MazeMaps API access from target domain
- Configure proper MIME types for WebGL files
- Test on target browsers and devices

## Troubleshooting

### Common Issues

1. **LocationManager is null**:
   - Ensure LocationManager prefab is assigned to GameLogic
   - Verify LocationManager script is attached to prefab
   - Check that locationData.json exists in Resources folder

2. **Map not loading**:
   - Check internet connection
   - Verify MazeMaps API access
   - Check browser console for JavaScript errors

3. **Scoring issues**:
   - Ensure actual location is set before guess submission
   - Verify coordinate format (latitude, longitude)
   - Check scoring curve configuration

4. **UI not updating**:
   - Verify UI GameObject references in GameLogic
   - Check event subscription in UI controllers
   - Enable debug logs to trace issues

### Debug Tools

- **DebugLogScript**: Displays real-time game state
- **Console Logging**: Comprehensive logging in all scripts
- **Browser Developer Tools**: JavaScript debugging
- **Unity Console**: C# error tracking

## Development

### Adding New Locations

1. Add location data to `locationData.json`
2. Create skybox material for location
3. Place material in `Assets/Resources/Materials/Locations/`
4. Update map pack configuration if needed

### Customizing UI

- Modify prefabs in `Assets/Prefabs/`
- Update WebGL template HTML/CSS
- Adjust UI scaling and positioning

### Extending Functionality

- Add new game modes in GameLogic
- Implement additional scoring systems
- Create new map pack types
- Add analytics and tracking

## Contributing

1. Fork the repository
2. Create feature branch
3. Make changes with proper documentation
4. Test thoroughly before submitting
5. Submit pull request with detailed description

## License

This project is developed for Monash University MNET and follows institutional guidelines.

## Support

For technical support:

- Check troubleshooting section above
- Review Unity console and browser logs
- Enable debug mode for detailed information
- Contact development team for complex issues
