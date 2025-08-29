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
      zoom: 15.4
    });
    
    setMap(mazemapInstance);
  }, []);

  return (
    <div className="App">
      <header className="App-header">
        <h1>MNET Geoguessr + Monash MazeMaps</h1>
        <p>Click on the map to place your guess!</p>
      </header>
      <main className="App-main">
        {map && <MazeMapWrapper map={map} />}
      </main>
    </div>
  );
}

export default App;
