using PurrNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : NetworkBehaviour
{
    public SoundPlayer soundPlayerPrefab; // Assign in Inspector
    public ParticleSystem environmentHitEffect, playerHitEffect;

    private Queue<SoundPlayer> soundPlayer_pool = new Queue<SoundPlayer>();
    public Queue<ParticleSystem> environmentImpactEffect_pool = new Queue<ParticleSystem>();
    public Queue<ParticleSystem> playerImpactEffect_pool = new Queue<ParticleSystem>();

    public List<Fracture> destructibleObjects = new List<Fracture>();

    [SerializeField] private Transform ImpactSoundPlayerCollector, EnvironementImpactEffectCollector, PlayerImpactEffectCollector;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<ObjectPoolManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetFracture();
        }
    }

    public void ResetFracture()
    {
        foreach(var obj in destructibleObjects)
        {
            if (obj.fragmentRoot)
            {
                Destroy(obj.fragmentRoot);
            }
            obj.gameObject.SetActive(true);
        }
    }

    #region ImpactEffect
    public ParticleSystem GetEnvironmentImpactEffect()
    {
        if (environmentImpactEffect_pool.Count > 0)
        {
            ParticleSystem impactEffect = environmentImpactEffect_pool.Dequeue();
            impactEffect.gameObject.SetActive(true);
            return impactEffect;
        }
        else
        {
            return Instantiate(environmentHitEffect, EnvironementImpactEffectCollector);
        }
    }

    public void ReturnToPoolAfter<T>(T obj, float delay, Queue<T> pool)
    {
        StartCoroutine(ReturnToPoolAfter_Couroutine(obj,delay, pool));
    }
    private IEnumerator ReturnToPoolAfter_Couroutine<T>(T obj, float delay, Queue<T> pool)
    {
        yield return new WaitForSeconds(delay);
        if (obj is Component component)
        {
            component.gameObject.SetActive(false);
        }
        pool.Enqueue(obj); 
    }

    public ParticleSystem GetPlayerImpactEffect()
    {
        if (playerImpactEffect_pool.Count > 0)
        {
            ParticleSystem impactEffect = playerImpactEffect_pool.Dequeue();
            impactEffect.gameObject.SetActive(true);
            return impactEffect;
        }
        else
        {
            return Instantiate(playerHitEffect, PlayerImpactEffectCollector);
        }
    }
    #endregion

    #region SoundPlayer
    public SoundPlayer GetSoundPlayer()
    {
        if (soundPlayer_pool.Count > 0)
        {
            SoundPlayer soundPlayer = soundPlayer_pool.Dequeue();
            soundPlayer.gameObject.SetActive(true);
            return soundPlayer;
        }
        else
        {
            return Instantiate(soundPlayerPrefab, ImpactSoundPlayerCollector); // Create new if none in pool
        }
    }

    public void ReturnToPool_SoundPlayer(SoundPlayer soundPlayer)
    {
        soundPlayer.gameObject.SetActive(false);
        soundPlayer_pool.Enqueue(soundPlayer);
    }
    #endregion
}
