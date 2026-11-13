using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Partida")]
    [Min(1)]
    public int totalEnemies = 10;

    [Header("Interface")]
    public TMP_Text scoreText;
    public TMP_Text healthText;
    public TMP_Text enemiesText;
    public GameObject victoryPanel;

    public int TotalEnemies => totalEnemies;
    public int RemainingEnemies { get; private set; }
    public bool GameWon { get; private set; }

    int score;
    PlayerHealth playerHealth;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RemainingEnemies = totalEnemies;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        UpdateScoreUI();
        UpdateEnemiesUI();
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Nenhum objeto com a tag Player foi encontrado.");
            return;
        }

        playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("O Player não possui o script PlayerHealth.");
            return;
        }

        playerHealth.Damaged += OnPlayerDamaged;
        playerHealth.Died += OnPlayerDied;

        UpdateHealthUI();
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.Damaged -= OnPlayerDamaged;
            playerHealth.Died -= OnPlayerDied;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void AddScore(int amount)
    {
        if (GameWon) return;

        score += amount;
        UpdateScoreUI();
    }

    public void EnemyDestroyed()
    {
        if (GameWon || RemainingEnemies <= 0) return;

        RemainingEnemies--;
        UpdateEnemiesUI();

        if (RemainingEnemies == 0)
        {
            WinGame();
        }
    }

    void OnPlayerDamaged(int damage, int currentHealth)
    {
        UpdateHealthUI();
    }

    void OnPlayerDied()
    {
        UpdateHealthUI();
    }

    void WinGame()
    {
        GameWon = true;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        EnemyShip[] enemies = FindObjectsByType<EnemyShip>(
        FindObjectsSortMode.None
        );

        foreach (EnemyShip enemy in enemies)
        {
            enemy.StopCombat();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Pontos: " + score;
        }
    }

    void UpdateHealthUI()
    {
        if (healthText == null || playerHealth == null) return;

        healthText.text =
        "Vida: " +
        playerHealth.CurrentHealth +
        " / " +
        playerHealth.maxHealth;
    }

    void UpdateEnemiesUI()
    {
        if (enemiesText != null)
        {
            enemiesText.text = "Inimigos: " + RemainingEnemies;
        }
    }
}