import React, { useEffect, useState } from 'react';
import * as Mazemap from 'mazemap';
import { MazeMapWrapper, makeMazeMapInstance } from './MazeMap';
import './App.css';

function App() {
  const [map, setMap] = useState(null);
  const [marker, setMarker] = useState(null);

  useEffect(() => {
    // Initialize Mazemap when component mounts
    const campusId = 159; // Clayton Campus
    const mazemapInstance = makeMazeMapInstance({
      campuses: campusId,
      center: { lng: 145.133963, lat: -37.911785 },
      zoom: 15.4,
      // Restrict to levels P4 and above (including parking levels P1-P4)
      zLevel: 0, // Start at ground level (0 = LG)
      minZLevel: -4, // Minimum level allowed (-4 = P4, include parking levels)
      maxZLevel: 12 // Maximum level allowed (Clayton campus has up to 12 levels)
    });
    
    setMap(mazemapInstance);
  }, []);

  // Helper function to convert z-level to readable name
  const getZLevelName = (zLevel) => {
    if (zLevel === -4) return 'P4 (Parking Level 4)';
    if (zLevel === -3) return 'P3 (Parking Level 3)';
    if (zLevel === -2) return 'P2 (Parking Level 2)';
    if (zLevel === -1) return 'P1 (Parking Level 1)';
    if (zLevel === 0) return 'LG (Lower Ground)';
    if (zLevel === 1) return 'G (Ground)';
    if (zLevel === 2) return '1 (First Floor)';
    if (zLevel === 3) return '2 (Second Floor)';
    if (zLevel === 4) return '3 (Third Floor)';
    if (zLevel === 5) return '4 (Fourth Floor)';
    if (zLevel === 6) return '5 (Fifth Floor)';
    if (zLevel === 7) return '6 (Sixth Floor)';
    if (zLevel === 8) return '7 (Seventh Floor)';
    if (zLevel === 9) return '8 (Eighth Floor)';
    if (zLevel === 10) return '9 (Ninth Floor)';
    if (zLevel === 11) return '10 (Tenth Floor)';
    if (zLevel === 12) return '11 (Eleventh Floor)';
    if (zLevel < -4) return `B${Math.abs(zLevel)} (Basement ${Math.abs(zLevel)})`;
    return `${zLevel} (Level ${zLevel})`;
  };

  // Handle map click events and add/update marker
  const handleMapClick = (event) => {
    // Use the official MazeMap API to get comprehensive click data
    const clickData = event.target.getMapClickData ? event.target.getMapClickData(event) : null;
    
    // Extract z-level information (either from clickData or fallback)
    let zLevel = 'unknown';
    let zLevelName = 'unknown';
    
    if (clickData && clickData.zLevel !== undefined) {
      zLevel = clickData.zLevel;
      zLevelName = getZLevelName(zLevel);
    } else {
      // Fallback to manual extraction if getMapClickData is not available
      zLevel = event.target.getZLevel ? event.target.getZLevel() : 'unknown';
      zLevelName = typeof zLevel === 'number' ? getZLevelName(zLevel) : zLevel;
    }
    
    // Create new marker data
    const newMarker = {
      id: Date.now(),
      lng: event.lngLat.lng,
      lat: event.lngLat.lat,
      zLevel: zLevel,
      zLevelName: zLevelName,
      timestamp: new Date().toLocaleTimeString(),
      // Additional data from getMapClickData if available
      buildingId: clickData ? clickData.buildingId : null,
      roomId: clickData ? clickData.roomId : null,
      poiId: clickData ? clickData.poiId : null
    };

    // Update marker state (this will replace any existing marker)
    setMarker(newMarker);
    
    // Add marker to the map
    addMarkerToMap(newMarker);
    
    console.log('Marker placed at:', {
      coordinates: event.lngLat,
      zLevel: zLevel,
      zLevelName: zLevelName,
      lng: event.lngLat.lng,
      lat: event.lngLat.lat,
      clickData: clickData
    });
  };

  // Function to add marker to the map
  const addMarkerToMap = (markerData) => {
    if (!map) {
      console.error('Map not available for marker placement');
      return;
    }

    console.log('Adding marker to map:', markerData);

    // Remove existing marker if it exists
    if (map._currentMarker) {
      console.log('Removing existing marker');
      map._currentMarker.remove();
    }

    try {
      // Create new marker using the official MazeMap API
      const newMarker = new Mazemap.MazeMarker({
        zLevel: markerData.zLevel // Set the floor zLevel coordinate
      })
      .setLngLat([markerData.lng, markerData.lat]) // Set the LngLat coordinates
      .addTo(map); // Add to the map

      console.log('Marker created and added to map:', newMarker);

      // Store reference to current marker
      map._currentMarker = newMarker;
    } catch (error) {
      console.error('Error creating marker:', error);
    }
  };

  return (
    <div className="App">
      <header className="App-header">
        <h1>MNET Geoguessr + Monash MazeMaps</h1>
        <p>Click on the map to place your guess!</p>
        {marker && (
          <div className="marker-info">
            <h3>Your Guess</h3>
            <p><strong>Level:</strong> {marker.zLevelName}</p>
            <p><strong>Coordinates:</strong> {marker.lng.toFixed(6)}, {marker.lat.toFixed(6)}</p>
            {marker.buildingId && (
              <p><strong>Building ID:</strong> {marker.buildingId}</p>
            )}
            {marker.roomId && (
              <p><strong>Room ID:</strong> {marker.roomId}</p>
            )}
            {marker.poiId && (
              <p><strong>POI ID:</strong> {marker.poiId}</p>
            )}
          </div>
        )}
      </header>
      <main className="App-main">
        {map && <MazeMapWrapper map={map} onMapClick={handleMapClick} />}
      </main>
    </div>
  );
}

export default App;
