(function () {
  var pinActive = false; // false = inactive (auto-collapse allowed), true = active (pinned)
  // --------------------------------------------------------------- MAZE MAP INITIALIZATION
  function isMazeMapReady() {
    if (typeof mazemap !== "undefined" && typeof mazemap.Map === "function")
      return true;
    if (
      typeof window.mazemap !== "undefined" &&
      typeof window.mazemap.Map === "function"
    )
      return true;
    if (
      typeof window.MazeMap !== "undefined" &&
      typeof window.MazeMap.Map === "function"
    )
      return true;
    for (var key in window) {
      try {
        if (Object.prototype.hasOwnProperty.call(window, key)) {
          if (String(key).toLowerCase().includes("maze")) {
            var obj = window[key];
            if (
              obj &&
              typeof obj === "object" &&
              typeof obj.Map === "function"
            ) {
              window.mazemap = obj;
              return true;
            }
          }
        }
      } catch (e) {
        /* ignore */
      }
    }
    return false;
  }

  function initializeMazeMap() {
    if (!isMazeMapReady()) {
      if (window.mazeMapRetryCount === undefined) window.mazeMapRetryCount = 0;
      if (window.mazeMapRetryCount < 10) {
        window.mazeMapRetryCount++;
        return void setTimeout(initializeMazeMap, 1000);
      }
      console.error("Maze Maps failed to load after 10 attempts");
      return;
    }
    try {
      var MazeLibrary = window.Maze || mazemap;
      var map = new MazeLibrary.Map({
        container: "map",
        campuses: 159,
        center: { lng: 145.1361, lat: -37.9106 },
        zoom: 16,
        minZLevel: 0,
        maxZLevel: 12,
      });
      window.mazeMapInstance = map;
      map.on("load", function () {
        console.log("Maze Maps ready for interaction");
      });

      map.on("click", function (e) {
        createSingleMarker(map, e.lngLat, map.zLevel);
      });
    } catch (error) {
      console.error(
        "Failed to initialize Maze Maps:",
        error && error.message ? error.message : error
      );
    }
  }

  window.addEventListener("load", function () {
    setTimeout(initializeMazeMap, 1000);
  });

  // --------------------------------------------------------------- MARKER PLACEMENT
  function createSingleMarker(map, lngLat, zLevel) {
    // Keep a reference on the map object itself (or use a module-level variable)
    // zlevel is +1 from what is shown on the ui eg. ground = 0, level 1 = 2
    if (map._clickMarker) {
      map._clickMarker.remove();
    }

    map._clickMarker = new Mazemap.MazeMarker({
      imgUrl: "assets/img/markers/handthing.svg",
      imgScale: 1.7,
      color: "white",
      size: 60,
      innerCircle: false,
      shape: "marker",
      zLevel: zLevel,
    })
      .setLngLat(lngLat)
      .addTo(map);

    // Store zLevel on marker for easy retrieval later
    map._clickMarker._storedZLevel = zLevel;
    map._clickMarker._storedLngLat = lngLat;

    // Enable guess button when marker is placed
    updateGuessButtonState(true);

    console.log("Guess Marker placed at:", lngLat, "on zLevel:", zLevel);
  }

  // --------------------------------------------------------------- Z-LEVEL NAME HELPER
  function getZLevelName(zLevel) {
    if (zLevel === -4) return "P4 (Parking Level 4)";
    if (zLevel === -3) return "P3 (Parking Level 3)";
    if (zLevel === -2) return "P2 (Parking Level 2)";
    if (zLevel === -1) return "P1 (Parking Level 1)";
    if (zLevel === 0) return "LG (Lower Ground)";
    if (zLevel === 1) return "G (Ground)";
    if (zLevel === 2) return "1 (First Floor)";
    if (zLevel === 3) return "2 (Second Floor)";
    if (zLevel === 4) return "3 (Third Floor)";
    if (zLevel === 5) return "4 (Fourth Floor)";
    if (zLevel === 6) return "5 (Fifth Floor)";
    if (zLevel === 7) return "6 (Sixth Floor)";
    if (zLevel === 8) return "7 (Seventh Floor)";
    if (zLevel === 9) return "8 (Eighth Floor)";
    if (zLevel === 10) return "9 (Ninth Floor)";
    if (zLevel === 11) return "10 (Tenth Floor)";
    if (zLevel === 12) return "11 (Eleventh Floor)";
    if (zLevel < -4)
      return "B" + Math.abs(zLevel) + " (Basement " + Math.abs(zLevel) + ")";
    return zLevel + " (Level " + zLevel + ")";
  }

  // --------------------------------------------------------------- SUBMIT GUESS
  function submitGuess() {
    var map = window.mazeMapInstance;
    if (!map) {
      console.error("Map instance not available");
      return;
    }

    // Get the current active guess marker
    var marker = map._clickMarker;
    if (!marker) {
      console.warn("No guess marker placed. Please click on the map first.");
      return;
    }

    try {
      // Get coordinates from marker (try getLngLat() first, fallback to stored value)
      var lngLat = null;
      if (typeof marker.getLngLat === "function") {
        lngLat = marker.getLngLat();
      }
      // Fallback to stored value if getLngLat() doesn't work or returns null
      if (!lngLat && marker._storedLngLat) {
        lngLat = marker._storedLngLat;
      }
      if (!lngLat) {
        console.error("Could not get coordinates from marker");
        return;
      }

      // Get zLevel from stored value, marker options, or map
      var zLevel =
        marker._storedZLevel !== undefined
          ? marker._storedZLevel
          : marker.options
          ? marker.options.zLevel
          : map.zLevel || 0;

      // Build JSON payload matching EnhancedMapClickData structure
      var payload = {
        latitude: lngLat.lat,
        longitude: lngLat.lng,
        zLevel: zLevel,
        zLevelName: getZLevelName(zLevel),
        timestamp: Date.now(),
      };

      // Send to Unity
      var jsonString = JSON.stringify(payload);

      // Try to find Unity instance (could be unityInstance or gameInstance depending on Unity version)
      var unityInstance = window.unityInstance || window.gameInstance;
      if (unityInstance && typeof unityInstance.SendMessage === "function") {
        unityInstance.SendMessage(
          "MapInteractionManager", // GameObject name
          "SubmitGuess", // Method name
          jsonString // JSON string
        );
        console.log("Guess submitted to Unity:", payload);

        // Disable button after submission (will be re-enabled when new marker is placed)
        updateGuessButtonState(false);
      } else {
        console.error("Unity instance not found. Cannot submit guess.");
      }
    } catch (error) {
      console.error("Error submitting guess:", error);
    }
  }

  // Expose submitGuess to global scope so it can be called from Unity or buttons
  window.submitGuess = submitGuess;

  // --------------------------------------------------------------- GUESS BUTTON MANAGEMENT
  function updateGuessButtonState(hasMarker) {
    var button = document.getElementById("guess-button");
    if (!button) return;

    if (hasMarker) {
      button.disabled = false;
      button.textContent = "Guess";
    } else {
      button.disabled = true;
      button.textContent = "Place your pin on the map";
    }
  }

  function syncGuessButtonWidth() {
    var widget = document.getElementById("maze-map-widget");
    var button = document.getElementById("guess-button");
    if (!widget || !button) return;

    // Get computed width of widget
    var widgetWidth = widget.offsetWidth;
    if (widgetWidth > 0) {
      button.style.width = widgetWidth + "px";
    }
  }

  // Initialize guess button
  function initializeGuessButton() {
    var button = document.getElementById("guess-button");
    if (!button) return;

    // Wire up click handler
    button.addEventListener("click", function () {
      if (!button.disabled) {
        submitGuess();
      }
    });

    // Sync width initially and whenever widget size changes
    syncGuessButtonWidth();

    // Watch for widget size changes
    var widget = document.getElementById("maze-map-widget");
    if (widget) {
      var observer = new MutationObserver(function () {
        syncGuessButtonWidth();
      });
      observer.observe(widget, {
        attributes: true,
        attributeFilter: ["class"],
      });

      // Also watch for resize events
      window.addEventListener("resize", syncGuessButtonWidth);
    }
  }

  // --------------------------------------------------------------- UI CONTROLS FOR SIZE TOGGLING
  // Size toggle helpers (no logic wiring yet)
  function setWidgetSize(size) {
    var widget = document.getElementById("maze-map-widget");
    if (!widget) return;
    var sizes = ["mm-size-s", "mm-size-m", "mm-size-l"];
    for (var i = 0; i < sizes.length; i++) {
      widget.classList.remove(sizes[i]);
    }
    widget.classList.add(size);
    // Update control states after size change
    updateControlDisabled();
    // Ensure MazeMap fits the new container size
    queueMapResize();
    // Sync guess button width with new widget size
    syncGuessButtonWidth();
  }
  window.mmSetWidgetSize = setWidgetSize;

  // Logic wiring for controls: expand/minimise cycle through sizes
  function getCurrentSize() {
    var widget = document.getElementById("maze-map-widget");
    if (!widget) return "mm-size-s";
    var sizes = ["mm-size-s", "mm-size-m", "mm-size-l"];
    for (var i = 0; i < sizes.length; i++) {
      if (widget.classList.contains(sizes[i])) return sizes[i];
    }
    return "mm-size-s";
  }

  function cycleSize(direction) {
    var order = ["mm-size-s", "mm-size-m", "mm-size-l"];
    var current = getCurrentSize();
    var idx = order.indexOf(current);
    if (idx === -1) idx = 0;
    if (direction === "next") {
      idx = Math.min(order.length - 1, idx + 1);
    } else if (direction === "prev") {
      idx = Math.max(0, idx - 1);
    }
    setWidgetSize(order[idx]);
  }

  function wireControls() {
    var root = document.getElementById("maze-map-ui") || document;
    var btnExpand = root.querySelector(".mm-controls .mm-expand");
    var btnMinimize = root.querySelector(".mm-controls .mm-minimise");
    var btnPin = root.querySelector(".mm-controls .mm-pin");

    if (btnExpand) {
      btnExpand.addEventListener("click", function () {
        cycleSize("next");
      });
    }
    if (btnMinimize) {
      btnMinimize.addEventListener("click", function () {
        cycleSize("prev");
      });
    }
    if (btnPin) {
      // Toggle pin active/inactive visual state
      btnPin.setAttribute("aria-pressed", "false");
      btnPin.addEventListener("click", function (e) {
        pinActive = !pinActive;
        btnPin.setAttribute("aria-pressed", pinActive ? "true" : "false");
      });
    }

    // Initialize disabled state now and keep it in sync on class changes
    updateControlDisabled();
    var widgetEl = document.getElementById("maze-map-widget");
    if (widgetEl) {
      var classObserver = new MutationObserver(updateControlDisabled);
      classObserver.observe(widgetEl, {
        attributes: true,
        attributeFilter: ["class"],
      });
    }

    // Collapse to small on outside click when pin is inactive
    document.addEventListener("click", handleOutsideClick, true);

    // Observe container size changes to trigger map.resize()
    var containerEl = document.getElementById("maze-maps-container");
    if (containerEl && typeof ResizeObserver !== "undefined") {
      var ro = new ResizeObserver(function () {
        queueMapResize();
      });
      ro.observe(containerEl);
    }
  }

  function updateControlDisabled() {
    var root = document.getElementById("maze-map-ui") || document;
    var btnExpand = root.querySelector(".mm-controls .mm-expand");
    var btnMinimize = root.querySelector(".mm-controls .mm-minimise");
    var size = getCurrentSize();
    if (btnExpand) {
      btnExpand.disabled = size === "mm-size-l";
    }
    if (btnMinimize) {
      btnMinimize.disabled = size === "mm-size-s";
    }
  }

  function handleOutsideClick(event) {
    if (pinActive) return; // pinned: ignore outside clicks
    var widget = document.getElementById("maze-map-widget");
    var controls = document.querySelector("#maze-map-ui .mm-controls");
    var size = getCurrentSize();
    if (!widget) return;
    // Determine if click is inside widget or controls
    var clickedInsideWidget = widget.contains(event.target);
    var clickedInsideControls = controls
      ? controls.contains(event.target)
      : false;
    if (
      !clickedInsideWidget &&
      !clickedInsideControls &&
      size !== "mm-size-s"
    ) {
      setWidgetSize("mm-size-s");
    }
  }

  // --------------------------------------------------------------- MAP RESIZE HANDLING
  // Debounced map.resize to avoid white gaps after container size changes
  var mapResizeScheduled = false;
  function queueMapResize() {
    if (mapResizeScheduled) return;
    mapResizeScheduled = true;
    // Two rafs to wait for layout, plus a timeout fallback
    requestAnimationFrame(function () {
      requestAnimationFrame(function () {
        mapResizeScheduled = false;
        resizeMazeMap();
      });
    });
    setTimeout(function () {
      mapResizeScheduled = false;
      resizeMazeMap();
    }, 200);
  }

  function resizeMazeMap() {
    var map = window.mazeMapInstance;
    if (map && typeof map.resize === "function") {
      try {
        map.resize();
      } catch (e) {
        /* ignore */
      }
    }
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", function () {
      wireControls();
      initializeGuessButton();
    });
  } else {
    wireControls();
    initializeGuessButton();
  }
})();
