using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform player;

    [Header("Tempo")]
    public float interval = 2.5f;

    [Header("Posição")]
    [Tooltip("Distância em que o inimigo nasce à frente do Player")]
    public float distanceAhead = 150f;

    public float spread = 20f;

    [Header("Limite simultâneo")]
    [Tooltip("Quantidade máxima de inimigos presentes ao mesmo tempo")]
    public int maxEnemiesOnScreen = 5;

    PlayerShip ship;
    float nextSpawn;
    int spawnedEnemies;

    void Start()
    {
        if (player != null)
        {
            ship = player.GetComponent<PlayerShip>();
        }

        if (ship == null)
        {
            Debug.LogError(
                "O Player não foi configurado ou não possui PlayerShip."
            );
        }
    }

    void Update()
    {
        if (ship == null || enemyPrefab == null) return;

        GameManager gameManager = GameManager.Instance;

        if (gameManager == null) return;
        if (gameManager.GameWon) return;
        if (spawnedEnemies >= gameManager.TotalEnemies) return;
        if (Time.time < nextSpawn) return;

        int enemiesOnScreen =
            GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (enemiesOnScreen >= maxEnemiesOnScreen) return;

        SpawnEnemy();
        nextSpawn = Time.time + interval;
    }

    void SpawnEnemy()
    {
        Quaternion rail = ship.RailRotation;

        Vector3 center =
            ship.RailCenter +
            ship.RailForward * distanceAhead;

        Vector3 randomOffset =
            rail * new Vector3(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0f
            );

        Quaternion rotation =
            Quaternion.LookRotation(
                -ship.RailForward,
                rail * Vector3.up
            );

        GameObject enemy = Instantiate(
            enemyPrefab,
            center + randomOffset,
            rotation
        );

        enemy.tag = "Enemy";
        spawnedEnemies++;
    }
}