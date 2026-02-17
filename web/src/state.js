/**
 * Shared app state: pin and guessing state.
 * All reads/writes go through getters/setters so the rest of the app stays decoupled.
 */

let pinActive = false; // false = inactive (auto-collapse allowed), true = active (pinned)
let isGuessingState = true; // true = guessing, false = results/end-round

export function getPinActive() {
  return pinActive;
}

export function setPinActive(value) {
  pinActive = !!value;
}

export function getGuessingState() {
  return isGuessingState;
}

export function setGuessingState(value) {
  isGuessingState = !!value;
}
