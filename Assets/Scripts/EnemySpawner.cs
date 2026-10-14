using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public float spawnInterval = 3f;
    public float minInterval = 0.8f;
    public int maxAlive = 10;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        int alive = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (alive >= maxAlive) return;

        int e = Random.Range(0, enemyPrefabs.Length);
        int s = Random.Range(0, spawnPoints.Length);
        Instantiate(enemyPrefabs[e], spawnPoints[s].position, Quaternion.identity);
        spawnInterval = Mathf.Max(minInterval, spawnInterval - 0.1f);
    }
}
