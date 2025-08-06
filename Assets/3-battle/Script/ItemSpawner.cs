using System.Collections;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject itemPrefab;
    public int maxItemsOnMap = 5;
    public float spawnInterval = 8f;
    public Vector3 areaMin;    // góc min của khu vực spawn
    public Vector3 areaMax;    // góc max của khu vực spawn
    public LayerMask spawnObstacleMask; // chỗ không spawn (vd: tường, vật cản)

    private int currentCount = 0;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (currentCount >= maxItemsOnMap) continue;

            Vector3 pos = GetRandomPosition();
            if (Physics.CheckSphere(pos, 1f, spawnObstacleMask))
                continue; // có vật cản, bỏ qua lần này

            Instantiate(itemPrefab, pos, Quaternion.identity);
            currentCount++;
        }
    }

    private Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(areaMin.x, areaMax.x),
            Random.Range(areaMin.y, areaMax.y),
            Random.Range(areaMin.z, areaMax.z)
        );
    }

    // Giảm count khi item bị hủy
    private void OnEnable()
    {
        ItemPickup[] items = FindObjectsOfType<ItemPickup>();
        foreach (var item in items)
            item.gameObject.AddComponent<OnDestroyNotify>().spawner = this;
    }

    public void NotifyItemDestroyed()
    {
        currentCount = Mathf.Max(0, currentCount - 1);
    }
}

public class OnDestroyNotify : MonoBehaviour
{
    [HideInInspector] public ItemSpawner spawner;
    private void OnDestroy()
    {
        if (spawner != null)
            spawner.NotifyItemDestroyed();
    }
}
