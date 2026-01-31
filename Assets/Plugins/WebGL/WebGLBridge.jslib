mergeInto(LibraryManager.library, {
  showMapFromUnity: function () {
    if (typeof window.showMapFromUnity === "function")
      window.showMapFromUnity();
  },
  hideMapFromUnity: function () {
    if (typeof window.hideMapFromUnity === "function")
      window.hideMapFromUnity();
  },
  addActualLocationFromUnity: function (jsonPtr) {
    var json = UTF8ToString(jsonPtr);
    if (typeof window.addActualLocationFromUnity === "function")
      window.addActualLocationFromUnity(json);
  },
  updateScoreFromUnity: function (score, round) {
    if (typeof window.updateScoreFromUnity === "function")
      window.updateScoreFromUnity(score, round);
  },
  showLoading: function (show) {
    if (typeof window.showLoading === "function") window.showLoading(!!show);
  },
  addMarkerFromUnity: function (lat, lng, labelPtr, typePtr) {
    var label = UTF8ToString(labelPtr);
    var type = UTF8ToString(typePtr);
    if (typeof window.addMarkerFromUnity === "function")
      window.addMarkerFromUnity(lat, lng, label, type);
  },
});
