using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ResourceTracker : MonoBehaviour
{
    public static ResourceTracker Instance { get; private set; } = null;

    private Dictionary<WorldResource.EType, List<WorldResource>> _trackedResources = new Dictionary<WorldResource.EType, List<WorldResource>>();

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"{gameObject.name} 이미 있어요");
            Destroy(gameObject);
            return;
        }
    
        var resourceTypes = System.Enum.GetValues(typeof(WorldResource.EType));
        foreach (var value in resourceTypes)
        {
            _trackedResources[(WorldResource.EType)value] = new List<WorldResource>();
        }

        Instance = this;
    }

    public void RegisterResource(WorldResource resource)
    {
        _trackedResources[resource.Type].Add(resource);
    }

    public void DeRegisterResource(WorldResource resource)
    {
        _trackedResources[resource.Type].Remove(resource);
    }



    private float Distance2D(Vector3 pos1, Vector3 pos2)
    {
        return Mathf.Sqrt((pos1.x - pos2.x) * (pos1.x - pos2.x) +
                          (pos1.z - pos2.z) * (pos1.z - pos2.z) +
                          (pos1.y - pos2.y) * (pos1.y - pos2.y));
    }

    public List<WorldResource> GetResourcesInRange(WorldResource.EType type, Vector3 location, float range)
    {
        return _trackedResources[type].Where(resource => Distance2D(resource.transform.position, location) <= range).ToList();
    }
}
