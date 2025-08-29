import React, { useEffect, useState } from 'react';
import { MazeMapWrapper, makeMazeMapInstance } from './MazeMap';
import './App.css';

function App() {
  const [map, setMap] = useState(null);

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

  // Handle map click events
  const handleMapClick = (event) => {
    // Extract z-level (floor/level) information from the map
    // zLevel represents which floor of the building was clicked (0 = ground floor, 1 = first floor, etc.)
    const zLevel = event.target.getZLevel ? event.target.getZLevel() : 'unknown';
    const zLevelName = typeof zLevel === 'number' ? getZLevelName(zLevel) : zLevel;
    
    console.log('Map clicked at:', {
      coordinates: event.lngLat,
      zLevel: zLevel,
      zLevelName: zLevelName,
      lng: event.lngLat.lng,
      lat: event.lngLat.lat
    });
  };

  return (
    <div className="App">
      <header className="App-header">
        <h1>MNET Geoguessr + Monash MazeMaps</h1>
        <p>Click on the map to see coordinates in the console!</p>
      </header>
      <main className="App-main">
        {map && <MazeMapWrapper map={map} onMapClick={handleMapClick} />}
      </main>
    </div>
  );
}

export default App;
