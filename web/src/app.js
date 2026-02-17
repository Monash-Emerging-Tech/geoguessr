import { initializeMazeMap } from "./mapCore.js";
import {
  createSingleMarker,
  drawGuessToActualLine,
  clearMapStateFromUnity,
} from "./markersAndLines.js";
import {
  submitGuess,
  addActualLocationFromUnity,
  showMapFromUnity,
  hideMapFromUnity,
  setGuessingStateFromUnity,
} from "./unityBridge.js";
import {
  updateGuessButtonState,
  initializeGuessButton,
  wireControls,
  setWidgetSize,
} from "./ui.js";

// --------------------------------------------------------------- MAP INIT (with click handler)

function onMapClick(map, lngLat, zLevel) {
  createSingleMarker(map, lngLat, zLevel);
  updateGuessButtonState(true);
  drawGuessToActualLine();
}

window.addEventListener("load", () => {
  setTimeout(() => initializeMazeMap(onMapClick), 1000);
});

// --------------------------------------------------------------- DOM READY

function bootstrap() {
  wireControls();
  initializeGuessButton(submitGuess);
}

if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", bootstrap);
} else {
  bootstrap();
}

// --------------------------------------------------------------- UNITY GLOBALS

window.submitGuess = submitGuess;
window.addActualLocationFromUnity = addActualLocationFromUnity;
window.showMapFromUnity = showMapFromUnity;
window.hideMapFromUnity = hideMapFromUnity;
window.setGuessingStateFromUnity = setGuessingStateFromUnity;
window.mmSetWidgetSize = setWidgetSize;

window.clearMapStateFromUnity = () => {
  clearMapStateFromUnity();
  updateGuessButtonState(false);
};
