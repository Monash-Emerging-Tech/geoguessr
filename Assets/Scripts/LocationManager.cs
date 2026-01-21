using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// LocationManager, Responsible for handling location data, map packs, and location selection.
/// Manages JSON data loading, material assignment, and current location tracking.
/// 
/// Written by O-Bolt
/// Modified by aleu0007
/// Last Modified: 24/09/2025
/// </summary>
public class LocationManager : MonoBehaviour
{
    #region Data Structures
    
    /// <summary>
    /// Container for location and map pack data loaded from JSON
    /// </summary>
    [System.Serializable]
    public class locationData
    {
        public List<Location> Locations;
        public List<MapPack> MapPacks;
    }

    /// <summary>
    /// Represents a single location with ID, name, coordinates, and material
    /// </summary>
    [System.Serializable]
    public struct Location
    {
        // Each Location has an ID, name and location material
        // (Name will be the long form data presented on the Screen after a guess)
        public int ID;
        public string Name;
        public string FileName;
        public float lat;  // Latitude (replaces x)
        public float lng;  // Longitude (replaces y)
        public int zLevel; // Z-level (replaces z)

        [System.NonSerialized]
        // Link to the 360 Image of the Location
        public Material LocationMaterial;
    }

    /// <summary>
    /// Represents a collection of locations grouped together
    /// </summary>
    [System.Serializable]
    public struct MapPack
    {
        public int ID;
        public string Name;
        public List<int> locationIDs;
    }

    #endregion

    #region Inspector Variables
    
    [Header("Data Configuration")]
    [SerializeField] private string jsonResourcePath = "locationData";
    
    #endregion

    #region Private Variables
    
    private Dictionary<int, Location> locationDict;
    private Dictionary<int, MapPack> mapPackDict;
    private MapPack currentMapPack;
    private Location currentLocation;
    
    #endregion

    #region Public Getters
    
    /// <summary>
    /// Checks if LocationManager has been initialized with data
    /// </summary>
    /// <returns>True if initialized, false otherwise</returns>
    public bool IsInitialized()
    {
        return mapPackDict != null && locationDict != null;
    }
    
    public Location GetCurrentLocation() => currentLocation;
    public MapPack GetCurrentMapPack() => currentMapPack;
    public string GetCurrentMapPackName() => currentMapPack.Name;
    public Dictionary<int, Location> GetLocationDict() => locationDict;
    public Dictionary<int, MapPack> GetMapPackDict() => mapPackDict;
    
    /// <summary>
    /// Gets all available MapPack names
    /// </summary>
    /// <returns>Array of MapPack names</returns>
    public string[] GetAllMapPackNames()
    {
        if (mapPackDict == null)
        {
            Debug.LogError("LocationManager: GetAllMapPackNames() called before initialization. MapPack dictionary is null.");
            return new string[0];
        }
        return mapPackDict.Values.Select(mp => mp.Name).ToArray();
    }
    
    /// <summary>
    /// Gets MapPack name by ID
    /// </summary>
    /// <param name="id">MapPack ID</param>
    /// <returns>MapPack name or "Unknown" if not found</returns>
    public string GetMapPackNameById(int id)
    {
        if (mapPackDict == null)
        {
            Debug.LogError("LocationManager: GetMapPackNameById() called before initialization. MapPack dictionary is null.");
            return "Unknown";
        }
        return mapPackDict.ContainsKey(id) ? mapPackDict[id].Name : "Unknown";
    }
    
    /// <summary>
    /// Gets MapPack ID by name
    /// </summary>
    /// <param name="name">MapPack name</param>
    /// <returns>MapPack ID or -1 if not found</returns>
    public int GetMapPackIdByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return -1;
        
        if (mapPackDict == null)
        {
            Debug.LogError("LocationManager: GetMapPackIdByName() called before initialization. MapPack dictionary is null.");
            return -1;
        }
        
        foreach (var kvp in mapPackDict)
        {
            if (kvp.Value.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Key;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Validates if a MapPack name exists
    /// </summary>
    /// <param name="name">MapPack name to validate</param>
    /// <returns>True if MapPack exists, false otherwise</returns>
    public bool IsValidMapPackName(string name)
    {
        return GetMapPackIdByName(name) != -1;
    }
    
    #endregion

    #region Initialization
    
    /// <summary>
    /// Initializes the LocationManager by loading data from JSON
    /// </summary>
    public void Start()
    {
        Debug.Log("LocationManager: Initializing...");
        LoadData();
        Debug.Log($"LocationManager: Init complete. Loaded {locationDict?.Count ?? 0} locations, {mapPackDict?.Count ?? 0} map packs.");
    }
    
    #endregion

    #region Location Management
    
    /// <summary>
    /// Selects a random location from the current map pack and sets it as skybox
    /// </summary>
    public void SelectRandomLocation()
    {
        if (string.IsNullOrEmpty(currentMapPack.Name))
        {
            Debug.LogWarning("LocationManager: Cannot select random location - no map pack is set. Call SetCurrentMapPack() first.");
            return;
        }
        
        List<Location> locations = GetLocationsFromMapPack(currentMapPack);
        
        if (locations.Count == 0)
        {
            Debug.LogWarning($"LocationManager: No locations found in map pack '{currentMapPack.Name}'");
            return;
        }

        int randomIndex = Random.Range(0, locations.Count);
        var selectedLocation = locations[randomIndex];
        
        // Check if material is loaded before setting skybox
        if (selectedLocation.LocationMaterial == null)
        {
            Debug.LogError($"LocationManager: Material missing for '{selectedLocation.Name}' (ID:{selectedLocation.ID}), FileName:'{selectedLocation.FileName}'");
        }
        else
        {
            RenderSettings.skybox = selectedLocation.LocationMaterial;
            DynamicGI.UpdateEnvironment();
        }
        
        Debug.Log($"LocationManager: Location selected - ID:{selectedLocation.ID}, Name:{selectedLocation.Name}, lat:{selectedLocation.lat}, lng:{selectedLocation.lng}, z:{selectedLocation.zLevel}");
        SetCurrentLocation(selectedLocation);
    }

    /// <summary>
    /// Gets all locations from a specific map pack
    /// </summary>
    /// <param name="mapPack">The map pack to get locations from</param>
    /// <returns>List of locations in the map pack</returns>
    public List<Location> GetLocationsFromMapPack(MapPack mapPack)
    {
        List<Location> locations = new List<Location>();

        if (mapPack.Name == "all" || mapPack.Name == "")
        {
            locations = locationDict.Values.ToList();
            return locations;
        }

        foreach (int id in mapPack.locationIDs)
        {
            locations.Add(locationDict[id]);
        }

        return locations;
    }
    
    #endregion

    #region Setters
    
    /// <summary>
    /// Sets the current location
    /// </summary>
    /// <param name="location">The location to set as current</param>
    public void SetCurrentLocation(Location location)
    {
        currentLocation = location;
    }

    /// <summary>
    /// Sets the current map pack by ID
    /// </summary>
    /// <param name="id">The ID of the map pack to set</param>
    public void SetCurrentMapPack(int id)
    {
        if (mapPackDict == null || mapPackDict.Count == 0)
        {
            Debug.LogError("LocationManager: Cannot set map pack - map pack dictionary not initialized.");
            return;
        }
        
        if (mapPackDict.ContainsKey(id))
        {
            currentMapPack = mapPackDict[id];
            int locationCount = GetLocationsFromMapPack(currentMapPack).Count;
            Debug.Log($"LocationManager: MapPack set '{currentMapPack.Name}' (ID:{id}) locations:{locationCount}");
        }
        else
        {
            Debug.LogWarning($"LocationManager: MapPack ID {id} not found. Available IDs: {string.Join(", ", mapPackDict.Keys)}");
        }
    }
    
    #endregion

    #region Data Loading
    
    /// <summary>
    /// Loads location and map pack data from JSON file and assigns materials
    /// </summary>
    private void LoadData()
    {
        if (string.IsNullOrEmpty(jsonResourcePath))
        {
            Debug.LogError("LocationManager: JSON Data Resource path is not assigned.");
            return;
        }

        Debug.Log($"LocationManager: Loading JSON from Resources path: {jsonResourcePath}");
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonResourcePath);
        if (jsonFile == null)
        {
            Debug.LogError($"LocationManager: JSON Data file not found at Resources path: {jsonResourcePath}");
            return;
        }

        string jsonData = jsonFile.text;
        if (string.IsNullOrEmpty(jsonData))
        {
            Debug.LogError("LocationManager: JSON file is empty!");
            return;
        }
        
        Debug.Log($"LocationManager: JSON data loaded, length: {jsonData.Length} characters");

        locationData data = JsonUtility.FromJson<locationData>(jsonData);
        if (data == null)
        {
            Debug.LogError("LocationManager: Failed to parse JSON data!");
            return;
        }
        
        if (data.Locations == null)
        {
            Debug.LogError("LocationManager: JSON data has null Locations array!");
            return;
        }
        
        if (data.MapPacks == null)
        {
            Debug.LogError("LocationManager: JSON data has null MapPacks array!");
            return;
        }
        
        // Initialize dictionaries
        locationDict = new Dictionary<int, Location>();
        mapPackDict = new Dictionary<int, MapPack>();
        
        // Load locations
        Debug.Log($"LocationManager: Loading {data.Locations.Count} locations...");
        foreach (Location location in data.Locations)
        {
            locationDict.Add(location.ID, location);
        }

        foreach (MapPack mapPack in data.MapPacks)
        {
            mapPackDict.Add(mapPack.ID, mapPack);
        }

        // Assign materials to locations
        AssignLocationMaterials();
    }

    /// <summary>
    /// Assigns materials to all loaded locations
    /// </summary>
    private void AssignLocationMaterials()
    {
        int failureCount = 0;
        
        foreach (Location location in locationDict.Values.ToList())
        {
            Location updatedLocation = location;
            string materialPath = $"Materials/Locations/{location.FileName}";
            
            updatedLocation.LocationMaterial = Resources.Load<Material>(materialPath);

            if (updatedLocation.LocationMaterial == null)
            {
                Debug.LogError($"LocationManager: Material not found for location '{location.Name}' (ID: {location.ID}), FileName: '{location.FileName}', Path: '{materialPath}'");
                failureCount++;
            }

            locationDict[location.ID] = updatedLocation;
        }
        
        if (failureCount > 0)
        {
            Debug.LogError($"LocationManager: Material assignment complete with {failureCount} failures");
        }
        else
        {
            Debug.Log("LocationManager: Material assignment complete with no failures");
        }
    }
    
    #endregion

    #region Legacy Methods (Deprecated)
    
    /// <summary>
    /// Legacy method - use GetLocationsFromMapPack instead
    /// </summary>
    [System.Obsolete("Use GetLocationsFromMapPack instead")]
    public List<Location> getLocationsFromMapPack(MapPack mapPack)
    {
        return GetLocationsFromMapPack(mapPack);
    }

    /// <summary>
    /// Legacy method - use SetCurrentLocation instead
    /// </summary>
    [System.Obsolete("Use SetCurrentLocation instead")]
    public void setCurrentLocation(Location location)
    {
        SetCurrentLocation(location);
    }

    /// <summary>
    /// Legacy method - use SetCurrentMapPack instead
    /// </summary>
    [System.Obsolete("Use SetCurrentMapPack instead")]
    public void setCurrentMapPack(int id)
    {
        SetCurrentMapPack(id);
    }
    
    #endregion
}