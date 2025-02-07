using PurrNet;
using System;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] SyncVar<int> health = new (0);
    [SerializeField] private int selfLayer, otherLayer;

    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private SoundPlayer soundPlayerPrefab;
    [SerializeField] private AudioClip deathSound;
    [SerializeField, Range(0f,1f)] private float deathAudioVolume = 0.5f;

    public Action<PlayerID> OnDeath_Server;
    public int Health => health;
    public bool isSpectatingThisPlayer;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        var actualLayer = isOwner? selfLayer : otherLayer;
        SetLayerRecursive(gameObject, actualLayer);

        if(isOwner)
        {         
            health.onChanged += OnHealthChanged;
            ChangeHealth(maxHealth);
            InstanceHandler.GetInstance<MainGameView>().UpdateHealth(health.value, maxHealth);
            InstanceHandler.GetInstance<MainGameView>().UpdateHealthBarInstant(health.value, maxHealth);
        }
        else
        {
            health.onChanged += OnHealthChanged_NotPlayer;
        }
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();

        health.onChanged -= OnHealthChanged;
    }


    private void OnHealthChanged(int newHealth)
    {
        InstanceHandler.GetInstance<MainGameView>().UpdateHealth(newHealth,maxHealth);
        InstanceHandler.GetInstance<MainGameView>().UpdateHealthBarSmooth(newHealth, maxHealth);
    }

    private void OnHealthChanged_NotPlayer(int newHealth)
    {
        if(!isSpectatingThisPlayer)
        {
            return;
        }
        InstanceHandler.GetInstance<MainGameView>().UpdateHealth(newHealth, maxHealth);
        InstanceHandler.GetInstance<MainGameView>().UpdateHealthBarSmooth(newHealth, maxHealth);
    }

    private void SetLayerRecursive(GameObject obj, int layer) 
    { 
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    } 

    [ServerRpc(requireOwnership:false)]
    public void ChangeHealth(int amount, RPCInfo info = default)
    {
        health.value += amount;
        health.value = Mathf.Clamp(health.value, 0, maxHealth);

        if (health <= 0)
        {
            if(InstanceHandler.TryGetInstance(out ScoreManager scoreManager))
            {
                scoreManager.AddKill(info.sender);
                if (owner.HasValue)
                {
                    UpdateDiePlayerHP(owner.Value);
                    scoreManager.AddDeath(owner.Value);
                }
            }

            PlayDeathEffect();
            OnDeath_Server?.Invoke(owner.Value);    
            
            Destroy(gameObject);
       
        }
    }

    [ObserversRpc(runLocally:true)]
    private void PlayDeathEffect()
    {
        Instantiate(deathParticles, transform.position + Vector3.up, Quaternion.identity);

        var soundPlayer = Instantiate(soundPlayerPrefab, transform.position + Vector3.up, Quaternion.identity);
        soundPlayer.PlaySound(deathSound, deathAudioVolume);
    }

    [TargetRpc]
    private void UpdateDiePlayerHP(PlayerID targetPlayer)
    {
        InstanceHandler.GetInstance<MainGameView>().UpdateHealth(0, maxHealth);
        InstanceHandler.GetInstance<MainGameView>().UpdateHealthBarSmooth(0, maxHealth);
    }

    public void UpdateSpecatorUI()
    {
        InstanceHandler.GetInstance<MainGameView>().UpdateHealth(health.value, maxHealth);
        InstanceHandler.GetInstance<MainGameView>().UpdateHealthBarInstant(health.value, maxHealth);
    }
}
