(function(){
  var pinActive = false; // false = inactive (auto-collapse allowed), true = active (pinned)
  function isMazeMapReady(){
    if (typeof mazemap !== 'undefined' && typeof mazemap.Map === 'function') return true;
    if (typeof window.mazemap !== 'undefined' && typeof window.mazemap.Map === 'function') return true;
    if (typeof window.MazeMap !== 'undefined' && typeof window.MazeMap.Map === 'function') return true;
    for (var key in window){
      try {
        if (Object.prototype.hasOwnProperty.call(window, key)){
          if (String(key).toLowerCase().includes('maze')){
            var obj = window[key];
            if (obj && typeof obj === 'object' && typeof obj.Map === 'function'){
              window.mazemap = obj;
              return true;
            }
          }
        }
      } catch(e) { /* ignore */ }
    }
    return false;
  }

  function initializeMazeMap(){
    if (!isMazeMapReady()){
      if (window.mazeMapRetryCount === undefined) window.mazeMapRetryCount = 0;
      if (window.mazeMapRetryCount < 10){
        window.mazeMapRetryCount++;
        return void setTimeout(initializeMazeMap, 1000);
      }
      console.error('Maze Maps failed to load after 10 attempts');
      return;
    }
    try {
      var MazeLibrary = window.Maze || mazemap;
      var map = new MazeLibrary.Map({
        container: 'map',
        campuses: 159,
        center: { lng: 145.1361, lat: -37.9106 },
        zoom: 16
      });
      window.mazeMapInstance = map;
      map.on('load', function(){ console.log('Maze Maps ready for interaction'); });
      map.on('click', function(e){ console.log('Map clicked:', e.lngLat.lng, e.lngLat.lat); });
    } catch (error){
      console.error('Failed to initialize Maze Maps:', error && error.message ? error.message : error);
    }
  }

  window.addEventListener('load', function(){ setTimeout(initializeMazeMap, 1000); });

  // Size toggle helpers (no logic wiring yet)
  function setWidgetSize(size){
    var widget = document.getElementById('maze-map-widget');
    if (!widget) return;
    var sizes = ['mm-size-s','mm-size-m','mm-size-l'];
    for (var i=0;i<sizes.length;i++){ widget.classList.remove(sizes[i]); }
    widget.classList.add(size);
    // Update control states after size change
    updateControlDisabled();
    // Ensure MazeMap fits the new container size
    queueMapResize();
  }
  window.mmSetWidgetSize = setWidgetSize;

  // Logic wiring for controls: expand/minimise cycle through sizes
  function getCurrentSize(){
    var widget = document.getElementById('maze-map-widget');
    if (!widget) return 'mm-size-s';
    var sizes = ['mm-size-s','mm-size-m','mm-size-l'];
    for (var i=0;i<sizes.length;i++){
      if (widget.classList.contains(sizes[i])) return sizes[i];
    }
    return 'mm-size-s';
  }

  function cycleSize(direction){
    var order = ['mm-size-s','mm-size-m','mm-size-l'];
    var current = getCurrentSize();
    var idx = order.indexOf(current);
    if (idx === -1) idx = 0;
    if (direction === 'next'){
      idx = Math.min(order.length - 1, idx + 1);
    } else if (direction === 'prev'){
      idx = Math.max(0, idx - 1);
    }
    setWidgetSize(order[idx]);
  }

  function wireControls(){
    var root = document.getElementById('maze-map-ui') || document;
    var btnExpand = root.querySelector('.mm-controls .mm-expand');
    var btnMinimize = root.querySelector('.mm-controls .mm-minimise');
    var btnPin = root.querySelector('.mm-controls .mm-pin');

    if (btnExpand){
      btnExpand.addEventListener('click', function(){ cycleSize('next'); });
    }
    if (btnMinimize){
      btnMinimize.addEventListener('click', function(){ cycleSize('prev'); });
    }
    if (btnPin){
      // Toggle pin active/inactive visual state
      btnPin.setAttribute('aria-pressed', 'false');
      btnPin.addEventListener('click', function(e){
        pinActive = !pinActive;
        btnPin.setAttribute('aria-pressed', pinActive ? 'true' : 'false');
      });
    }

    // Initialize disabled state now and keep it in sync on class changes
    updateControlDisabled();
    var widgetEl = document.getElementById('maze-map-widget');
    if (widgetEl){
      var classObserver = new MutationObserver(updateControlDisabled);
      classObserver.observe(widgetEl, { attributes: true, attributeFilter: ['class'] });
    }

    // Collapse to small on outside click when pin is inactive
    document.addEventListener('click', handleOutsideClick, true);

    // Observe container size changes to trigger map.resize()
    var containerEl = document.getElementById('maze-maps-container');
    if (containerEl && typeof ResizeObserver !== 'undefined'){
      var ro = new ResizeObserver(function(){ queueMapResize(); });
      ro.observe(containerEl);
    }
  }

  function updateControlDisabled(){
    var root = document.getElementById('maze-map-ui') || document;
    var btnExpand = root.querySelector('.mm-controls .mm-expand');
    var btnMinimize = root.querySelector('.mm-controls .mm-minimise');
    var size = getCurrentSize();
    if (btnExpand){ btnExpand.disabled = (size === 'mm-size-l'); }
    if (btnMinimize){ btnMinimize.disabled = (size === 'mm-size-s'); }
  }

  function handleOutsideClick(event){
    if (pinActive) return; // pinned: ignore outside clicks
    var widget = document.getElementById('maze-map-widget');
    var controls = document.querySelector('#maze-map-ui .mm-controls');
    var size = getCurrentSize();
    if (!widget) return;
    // Determine if click is inside widget or controls
    var clickedInsideWidget = widget.contains(event.target);
    var clickedInsideControls = controls ? controls.contains(event.target) : false;
    if (!clickedInsideWidget && !clickedInsideControls && size !== 'mm-size-s'){
      setWidgetSize('mm-size-s');
    }
  }

  // Debounced map.resize to avoid white gaps after container size changes
  var mapResizeScheduled = false;
  function queueMapResize(){
    if (mapResizeScheduled) return;
    mapResizeScheduled = true;
    // Two rafs to wait for layout, plus a timeout fallback
    requestAnimationFrame(function(){
      requestAnimationFrame(function(){
        mapResizeScheduled = false;
        resizeMazeMap();
      });
    });
    setTimeout(function(){ mapResizeScheduled = false; resizeMazeMap(); }, 200);
  }

  function resizeMazeMap(){
    var map = window.mazeMapInstance;
    if (map && typeof map.resize === 'function'){
      try { map.resize(); } catch (e) { /* ignore */ }
    }
  }

  if (document.readyState === 'loading'){
    document.addEventListener('DOMContentLoaded', wireControls);
  } else {
    wireControls();
  }
})();


