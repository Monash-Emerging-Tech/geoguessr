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
        public float x;
        public float y;
        public float z;

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
        return mapPackDict.Values.Select(mp => mp.Name).ToArray();
    }
    
    /// <summary>
    /// Gets MapPack name by ID
    /// </summary>
    /// <param name="id">MapPack ID</param>
    /// <returns>MapPack name or "Unknown" if not found</returns>
    public string GetMapPackNameById(int id)
    {
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
        LoadData();
    }
    
    #endregion

    #region Location Management
    
    /// <summary>
    /// Selects a random location from the current map pack and sets it as skybox
    /// </summary>
    public void SelectRandomLocation()
    {
        List<Location> locations = GetLocationsFromMapPack(currentMapPack);
        
        if (locations.Count == 0) return;

        int randomIndex = Random.Range(0, locations.Count);
        RenderSettings.skybox = locations[randomIndex].LocationMaterial;
        
        Debug.Log($"Location ID: {locations[randomIndex].ID} - {locationDict[randomIndex].Name}");
        SetCurrentLocation(locations[randomIndex]);
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
        if (mapPackDict.ContainsKey(id))
        {
            currentMapPack = mapPackDict[id];
            Debug.Log($"MapPack set to: {currentMapPack.Name} (ID: {id})");
        }
        else
        {
            Debug.LogWarning($"MapPack with ID {id} not found");
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
            Debug.LogError("JSON Data Resource path is not assigned.");
            return;
        }

        TextAsset jsonFile = Resources.Load<TextAsset>(jsonResourcePath);
        if (jsonFile == null)
        {
            Debug.LogError($"JSON Data file not found at Resources path: {jsonResourcePath}");
            return;
        }

        string jsonData = jsonFile.text;
        Debug.Log($"Loaded JSON data: {jsonData}");

        locationData data = JsonUtility.FromJson<locationData>(jsonData);
        
        // Initialize dictionaries
        locationDict = new Dictionary<int, Location>();
        mapPackDict = new Dictionary<int, MapPack>();
        
        // Load locations
        foreach (Location location in data.Locations)
        {
            locationDict.Add(location.ID, location);
        }

        // Load map packs
        foreach (MapPack mapPack in data.MapPacks)
        {
            mapPackDict.Add(mapPack.ID, mapPack);
        }

        Debug.Log($"Loaded {locationDict.Count} locations and {mapPackDict.Count} map packs");

        // Assign materials to locations
        AssignLocationMaterials();
    }

    /// <summary>
    /// Assigns materials to all loaded locations
    /// </summary>
    private void AssignLocationMaterials()
    {
        foreach (Location location in locationDict.Values.ToList())
        {
            Location updatedLocation = location;
            updatedLocation.LocationMaterial = Resources.Load<Material>($"Materials/Locations/{location.FileName}");

            if (updatedLocation.LocationMaterial == null)
            {
                Debug.LogWarning($"Material not found for location {location.Name}, MaterialName: {location.FileName}");
            }

            locationDict[location.ID] = updatedLocation;
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