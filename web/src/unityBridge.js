/**
 * Unity instance helper, submit guess, add actual location, show/hide map, set guessing state.
 */

import { getGuessingState, setGuessingState } from "./state.js";
import { getMapboxMap } from "./mapCore.js";
import {
  getZLevelName,
  drawGuessToActualLine,
  clearGuessLine,
} from "./markersAndLines.js";
import { updateGuessButtonState, syncGuessButtonWidth } from "./ui.js";

// --------------------------------------------------------------- UNITY INSTANCE

/**
 * @returns {Object|null} Unity instance or null if not found
 */
export function getUnityInstance() {
  if (
    window.unityInstance &&
    typeof window.unityInstance.SendMessage === "function"
  ) {
    return window.unityInstance;
  }
  if (
    window.gameInstance &&
    typeof window.gameInstance.SendMessage === "function"
  ) {
    return window.gameInstance;
  }
  const canvas = document.querySelector("#unity-canvas");
  if (canvas && canvas._unityInstance) {
    return canvas._unityInstance;
  }
  return null;
}

// --------------------------------------------------------------- SUBMIT GUESS

export function submitGuess() {
  const map = window.mazeMapInstance;
  if (!map) {
    console.error("Map instance not available");
    return;
  }

  const marker = map._clickMarker;
  if (!marker) {
    console.warn("No guess marker placed. Please click on the map first.");
    return;
  }

  try {
    let lngLat = null;
    if (typeof marker.getLngLat === "function") {
      lngLat = marker.getLngLat();
    }
    if (!lngLat && marker._storedLngLat) {
      lngLat = marker._storedLngLat;
    }
    if (!lngLat) {
      console.error("Could not get coordinates from marker");
      return;
    }

    const zLevel =
      marker._storedZLevel !== undefined
        ? marker._storedZLevel
        : marker.options
          ? marker.options.zLevel
          : map.zLevel || 0;

    const payload = {
      latitude: lngLat.lat,
      longitude: lngLat.lng,
      zLevel: zLevel,
      zLevelName: getZLevelName(zLevel),
    };

    const jsonString = JSON.stringify(payload);
    const unityInstance = getUnityInstance();

    if (unityInstance && typeof unityInstance.SendMessage === "function") {
      try {
        unityInstance.SendMessage(
          "MapInteractionManager",
          "SubmitGuess",
          jsonString
        );
        console.log("Guess submitted to Unity:", payload);
        updateGuessButtonState(false);
      } catch (error) {
        console.error("Error sending message to Unity:", error);
      }
    } else {
      console.error("Unity instance not found. Cannot submit guess.");
      console.error("Debug info:", {
        unityInstance: typeof window.unityInstance,
        gameInstance: typeof window.gameInstance,
        canvas: document.querySelector("#unity-canvas") ? "found" : "not found",
      });
    }
  } catch (error) {
    console.error("Error submitting guess:", error);
  }
}

// --------------------------------------------------------------- ACTUAL LOCATION FROM UNITY

/**
 * @param {string} jsonPayload - {"latitude": float, "longitude": float, "zLevel": int, "zLevelName": string}
 */
export function addActualLocationFromUnity(jsonPayload) {
  try {
    const map = window.mazeMapInstance;
    if (!map) {
      console.error(
        "Map instance not available for addActualLocationFromUnity"
      );
      return;
    }

    const mapboxMap = getMapboxMap(map);
    const locationData = JSON.parse(jsonPayload);
    const lat = locationData.latitude;
    const lng = locationData.longitude;
    const zLevel = locationData.zLevel || 0;

    console.log("Received actual location from Unity:", locationData);

    if (map._actualLocationMarker) {
      map._actualLocationMarker.remove();
    }

    const markerOptions = {
      zLevel: zLevel,
      innerCircle: false,
      color: "#9D9DDC",
      imgUrl: "assets/img/markers/fat.png",
      imgScale: 1.7,
      size: 60,
    };

    const marker = new Mazemap.MazeMarker(markerOptions)
      .setLngLat({ lng: lng, lat: lat })
      .addTo(map);

    map._actualLocationMarker = marker;
    drawGuessToActualLine();

    console.log("Actual location added from Unity:", {
      coordinates: { lat: lat, lng: lng },
      zLevel: zLevel,
      zLevelName: locationData.zLevelName,
    });

    if (map._clickMarker && map._actualLocationMarker) {
      const guessMarker = map._clickMarker;
      const actualMarker = map._actualLocationMarker;
      const guessLngLat =
        typeof guessMarker.getLngLat === "function"
          ? guessMarker.getLngLat()
          : guessMarker._storedLngLat;
      const actualLngLat =
        typeof actualMarker.getLngLat === "function"
          ? actualMarker.getLngLat()
          : actualMarker._storedLngLat;
      if (
        guessLngLat &&
        actualLngLat &&
        mapboxMap &&
        typeof mapboxMap.fitBounds === "function"
      ) {
        const bounds = [
          [guessLngLat.lng, guessLngLat.lat],
          [actualLngLat.lng, actualLngLat.lat],
        ];
        mapboxMap.fitBounds(bounds, { padding: 80 });
      }
    } else {
      map.flyTo({
        center: [lng, lat],
        zoom: map.getZoom(),
      });
    }
  } catch (error) {
    console.error("Error adding actual location from Unity:", error);
    console.error("Payload received:", jsonPayload);
  }
}

// --------------------------------------------------------------- MAP VISIBILITY

export function showMapFromUnity() {
  const mapUI = document.getElementById("maze-map-ui");
  if (mapUI) {
    mapUI.style.display = "block";
    window.isMapVisible = true;
    syncGuessButtonWidth();
    requestAnimationFrame(syncGuessButtonWidth);
    console.log("Unity call to show Minimap UI");
  } else {
    console.error("maze-map-ui element not found");
  }
}

export function hideMapFromUnity() {
  const mapUI = document.getElementById("maze-map-ui");
  if (mapUI) {
    mapUI.style.display = "none";
    window.isMapVisible = false;
    console.log("Unity call to hide Minimap UI");
  } else {
    console.error("maze-map-ui element not found");
  }
}

// --------------------------------------------------------------- GUESSING STATE FROM UNITY

export function setGuessingStateFromUnity(isGuessing) {
  setGuessingState(!!isGuessing);

  const button = document.getElementById("guess-button");
  if (button) {
    button.disabled = !getGuessingState();
    button.style.display = getGuessingState() ? "" : "none";
  }
  const controls = document.querySelector("#maze-map-ui .mm-controls");
  if (controls) {
    controls.style.display = getGuessingState() ? "" : "none";
  }

  const map = window.mazeMapInstance;
  updateGuessButtonState(!!(map && map._clickMarker));

  if (getGuessingState()) {
    clearGuessLine();
  } else {
    drawGuessToActualLine();
  }
}
