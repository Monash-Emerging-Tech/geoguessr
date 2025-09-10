using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;
using System.IO;
using static LocationManager;




/***
 * 
 * LocationManager, Responsible for handling the data of each Location, 
 * including the current chosen one and the Map Packs\
 * Functionality for choosing and display a location is here as well
 * 
 * Written by O-Bolt
 * Last Modified: 15/07/2025
 * 
 */
public class LocationManager
{

    // Location Data Class, Locations and Mappacks
    [System.Serializable]
    public class locationData
    {
        public List<Location> Locations;
        public List<MapPack> MapPacks;
    }


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

        // Need to add more stuff here when we know how to use it better

        [System.NonSerialized]
        // Link to the 360 Image of the Location
        public Material LocationMaterial;
    }

    [System.Serializable]
    public struct MapPack
    {
        public int ID;
        public string Name;
        public List<int> locationIDs;
    }

    // For now we'll just list all locations in one big list, could also do mappacks in the future
    public Dictionary<int, Location> locationDict;
    public Dictionary<int, MapPack> mapPackDict;

    // path to the locationData.json file
    private string jsonFilePath = "Assets/Resources/locationData.json";

    
    // In game selections
    public MapPack currentMapPack;
    public Location currentLocation;

    public void Start()
    {

        LoadData();

    }



    // Selects a random location and sets to the skybox
    // Could pass through a mappack in the future?
    public void SelectRandomLocation()
    {
        List<Location> locations = getLocationsFromMapPack(currentMapPack);
        
        if (locations.Count == 0) return;
 
        int rdmIdx = Random.Range(0, locations.Count);
        RenderSettings.skybox = locations[rdmIdx].LocationMaterial;
        Debug.Log("Location ID: " + locations[rdmIdx].ID + " - " + locationDict[rdmIdx].Name);
        setCurrentLocation(locations[rdmIdx]);
    }

    public List<Location> getLocationsFromMapPack(MapPack mapPack)
    {

        List<Location> locations = new List<Location>();

        if (mapPack.Name == "all" || mapPack.Name == "")
        {
            locations = locationDict.Values.ToList();
            return locations;
        }

        foreach (int ID in mapPack.locationIDs)
        {
            locations.Append(locationDict[ID]);
        }

        return locations;
    }


    //Setters and Getters
    public void setCurrentLocation(Location location) {
        currentLocation = location;
    }

    public void setCurrentMapPack(int Id)
    {
        currentMapPack = mapPackDict[Id];
    }


    public Dictionary<int, MapPack> GetMapPacks() {
        return mapPackDict;
    }

    // Creates Locations and Mappacks from the json File
    private void LoadData() {
        
        if (jsonFilePath == "")
        {
            Debug.LogError("JSON Data File is not assigned in the inspector.");
            return;
        }

        string jsonData = File.ReadAllText(jsonFilePath);

        Debug.Log(jsonData);

        locationData data = JsonUtility.FromJson<locationData>(jsonData);
        
        foreach (Location location in data.Locations) {
            locationDict.Add(location.ID, location);
        }

        foreach (MapPack mapPack in data.MapPacks)
        {
            mapPackDict.Add(mapPack.ID, mapPack);
        }


        Debug.Log("Count: " + locationDict.Count);

        for (int i = 0; i < locationDict.Count; i++)
        {
            Location location = locationDict[i];

            location.LocationMaterial = Resources.Load<Material>($"Materials/Locations/{location.FileName}");

            if (location.LocationMaterial == null)
                Debug.LogWarning($"Material not found for location {location.Name}, MaterialName: {location.FileName}");

            locationDict[i] = location; 
        }

    }


    /*
     * 
     * OUTDATED:
    * 
    // Validates the data in that was loaded by the json file
    // All locations and Mappacks need to have the same ID and position in the JSON file for consistency
    // Makes loading data easier for now anyway, I'm sure we can just allocate memory instead but that would require validating Locations
    private void CheckAllignment() {

        // Match all the locations to each other
        int idx = 0;
        int errors = 0;
        foreach (Location location in locationList)
        {
            if (location.ID != idx)
            {
                Debug.Log($"Out of Place Location, ID: {location.ID}, Index in json File: {idx}");
                errors++;
            }

            idx++;
        }

        idx = 0;
        foreach (MapPack mapPack in mapPacks)
        {
            if (mapPack.ID != idx)
            {
                Debug.Log($"Out of Place Map Pack, ID: {mapPack.ID}, Index in json File: {idx}");
                errors++;
            }

            idx++;
        }

        if (errors == 0)
        {
            Debug.Log("All locations and Map packs alligned");
        }
    }
    */

}
