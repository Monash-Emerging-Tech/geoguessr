using System.Collections.Generic;
using UnityEngine;

public class LocationManager : MonoBehaviour
{


    [System.Serializable]
    

    public struct Location
    {
        // Each Location has an ID, name and location material
        // (Name will be the long form data presented on the Screen after a guess)
        public int ID;
        public string Name;





        // Link to the 360 Image of the Location
        public Material LocationMaterial;
    }

    // For now we'll just list all locations in one big list, could also do mappacks in the future
    public List<Location> locationList;

    // Selects a random location and sets to the skybox
    // Could pass through a mappack in the future?
    public void SelectRandomLocation()
    {
        if (locationList.Count == 0) return;

        int rdmIdx = Random.Range(0, locationList.Count);
        RenderSettings.skybox = locationList[rdmIdx].LocationMaterial;
        Debug.Log("Location ID: " + locationList[rdmIdx].ID + " - " + locationList[rdmIdx].Name);
    }

}
