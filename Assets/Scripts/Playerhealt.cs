using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Basta adicionar este script ao Player: o DamageFlash (efeito vermelho) é adicionado junto.
[RequireComponent(typeof(DamageFlash))]
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    [Tooltip("Tempo sem receber dano depois de ser acertado")]
    public float invincibleTime = 0.4f;
    public bool reloadSceneOnDeath = true;
    public float reloadDelay = 2f;

    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    // (dano recebido, vida atual)
    public event Action<int, int> Damaged;
    public event Action Died;

    float invincibleUntil;

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || Time.time < invincibleUntil) return;

        invincibleUntil = Time.time + invincibleTime;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        if (Damaged != null) Damaged(amount, CurrentHealth);

        if (CurrentHealth == 0)
        {
            IsDead = true;
            if (Died != null) Died();
            if (reloadSceneOnDeath) Invoke(nameof(ReloadScene), reloadDelay);
        }
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}