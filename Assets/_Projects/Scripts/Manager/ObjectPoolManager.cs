using PurrNet;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GridBrushBase;

public class ObjectPoolManager : NetworkBehaviour
{
    [Header("Prefabs")]
    public SoundPlayer soundPlayerPrefab; // Assign in Inspector
    public ParticleSystem environmentHitEffectPrefab, playerHitEffectPrefab;
    public GameObject bulletHolePrefab;

    private Queue<SoundPlayer> soundPlayer_pool = new Queue<SoundPlayer>();
    public Queue<ParticleSystem> environmentImpactEffect_pool = new Queue<ParticleSystem>();
    public Queue<ParticleSystem> playerImpactEffect_pool = new Queue<ParticleSystem>();
    public Queue<GameObject> bulletHole_pool = new Queue<GameObject>();

    [Header("Tracking")]
    public List<Fracture> destructibleObjects = new List<Fracture>();


    [Header("Settings")]
    public float bulletHoleLifeTime = 30f;

    [Header("Reference")]
    [SerializeField] private Transform ImpactSoundPlayerCollector;
    [SerializeField] private Transform EnvironementImpactEffectCollector, PlayerImpactEffectCollector, BulletHoleCollector;


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
        foreach (var obj in destructibleObjects)
        {
            if (obj.fragmentRoot)
            {
                Destroy(obj.fragmentRoot);
            }

            obj.bulletHoleCollector.Clear();
            obj.gameObject.SetActive(true);
        }
    }

    #region ImpactEffect
    //ENVIRONMENT HIT
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
            return Instantiate(environmentHitEffectPrefab, EnvironementImpactEffectCollector);
        }
    }

    //PLAYER HIT
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
            return Instantiate(playerHitEffectPrefab, PlayerImpactEffectCollector);
        }
    }

    //BULLET HOLE
    public GameObject GetBulletHole()
    {
        if (bulletHole_pool.Count > 0)
        {
            GameObject bulletHole = bulletHole_pool.Dequeue();
            bulletHole.gameObject.SetActive(true);
            return bulletHole;
        }
        else
        {
            return Instantiate(bulletHolePrefab, BulletHoleCollector);
        }
    }

    public void TurnAllBulletHolesOff()
    {
        if (BulletHoleCollector.childCount == 0)
        {
            return;
        }

        foreach (GameObject child in BulletHoleCollector)
        {
            if (child.activeInHierarchy)
            {
                child.SetActive(false);
            }
        }
    }

    public void ReturnToPoolAfter<T>(T obj, float delay, Queue<T> pool)
    {
        StartCoroutine(ReturnToPoolAfter_Couroutine(obj, delay, pool));
    }
    private IEnumerator ReturnToPoolAfter_Couroutine<T>(T obj, float delay, Queue<T> pool)
    {
        
        yield return new WaitForSeconds(delay);
        GameObject go = obj as GameObject; 

        if(obj == null)
        {
            yield break;
        }

        //GAMEOBJECT TYPE
        if (go != null && go.activeInHierarchy) 
        {
            go.SetActive(false);         
        }
        else if(obj is Component component)
        {
            component.gameObject.SetActive(false);          
        }
        pool.Enqueue(obj);
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
