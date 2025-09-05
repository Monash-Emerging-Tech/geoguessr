using UnityEngine;

public class SpawnPrefab : MonoBehaviour
{
    [SerializeField] GameObject prefab;   // drag the PREFAB asset (blue) here
    [SerializeField] Transform parent;    // drag the SCENE parent (e.g., your overlay or Canvas)
    [SerializeField] bool preventMultiple = true;

    GameObject spawned;

    public void Spawn()
    {
        if (!prefab) { Debug.LogError("SpawnPrefab: No prefab assigned."); return; }
        if (!parent) { Debug.LogError("SpawnPrefab: No parent assigned."); return; }
        if (preventMultiple && spawned) return;

        // For UI prefabs, 'false' keeps RectTransform correct under parent
        spawned = Instantiate(prefab, parent, false);
        spawned.transform.SetAsLastSibling(); // on top

        // Optional: center it
        if (spawned.TryGetComponent<RectTransform>(out var rt))
            rt.anchoredPosition = Vector2.zero;
    }

    public void Despawn()
    {
        if (spawned) Destroy(spawned);
        spawned = null;
    }
}
