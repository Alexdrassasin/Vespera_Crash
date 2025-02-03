using PurrNet;
using PurrNet.StateMachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Gun : StateNode
{
    [Header("Stats")]
    [SerializeField] private float range = 20f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private int damage = 10;
    [SerializeField] private bool automatic;

    [Header("Recoil")]
    [SerializeField] private float recoilStrength = 1f;
    [SerializeField] private float recoilDuration = 0.2f;
    [SerializeField] private AnimationCurve recoilCurve;
    [SerializeField] private float rotationAmount = 25f;
    [SerializeField] private AnimationCurve rotationCurve;

    [Header("Reference")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Transform rightHandTarget, leftHandTarget;
    [SerializeField] private Transform rightIKTarget, leftIKTarget;
    [SerializeField] private List<Renderer> renderers = new();
    [SerializeField] private ParticleSystem environmentHitEffect, playerHitEffect;
    [SerializeField] private SoundPlayer soundPlayerPrefab;
    [SerializeField] private AudioSource shootSoundPlayer;
    [SerializeField, Range(0f,1f)] private float environmentHitVolume, playerHitVolume, shootVolume;
    [SerializeField] private List<AudioClip> environmentHitSounds, playerHitSounds, shootSounds;


    private float _lastFireTime;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Coroutine _recoilCoroutine;

    private void Awake()
    {
        ToggleVisuals(false);
    }

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        ToggleVisuals(true);
    }

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
        ToggleVisuals(false);
    }

    private void Start()
    {
        _originalPosition = transform.localPosition;
        _originalRotation = transform.localRotation;
    }

    private void ToggleVisuals(bool toggle)
    {
        foreach (var renderer in renderers)
        {
            renderer.enabled = toggle;
        }
    }

    public override void StateUpdate(bool asServer)
    {
        base.StateUpdate(asServer);

        SetIKTargets();

        if (!isOwner)
        {
            return;
        }

        if ((automatic && !Input.GetKey(KeyCode.Mouse0)) || (!automatic && !Input.GetKeyDown(KeyCode.Mouse0)))
        {
            StopShooting();
            return;
        }

        if (_lastFireTime + fireRate > Time.unscaledTime)
        {
            return;
        }

        PlayShotEffect();

        _lastFireTime = Time.unscaledTime;

        if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, range, hitLayer))
        {
            return;
        }

        if (!hit.transform.TryGetComponent(out PlayerHealth playerHealth))
        {
            EnvironmentHit(hit.point, hit.normal);
            return;
        }

        PlayerHit(playerHealth, playerHealth.transform.InverseTransformPoint(hit.point), hit.normal);

        playerHealth.ChangeHealth(-damage);
    }

    [ObserversRpc(runLocally: true)]
    private void PlayerHit(PlayerHealth player, Vector3 localposition, Vector3 normal)
    {
        if (!player || !player.transform)
        {
            return;
        }

        if (playerHitEffect)
        {
            Instantiate(playerHitEffect, player.transform.TransformPoint(localposition), Quaternion.LookRotation(normal));
        }

        var soundPlayer = Instantiate(soundPlayerPrefab, player.transform.TransformPoint(localposition), Quaternion.identity);
        soundPlayer.PlaySound(playerHitSounds[Random.Range(0, playerHitSounds.Count)],playerHitVolume);
    }

    [ObserversRpc(runLocally:true)]
    private void EnvironmentHit(Vector3 position, Vector3 normal)
    {
        if (environmentHitEffect)
        {
            Instantiate(environmentHitEffect, position, Quaternion.LookRotation(normal));
        }

        var soundPlayer = Instantiate(soundPlayerPrefab, position, Quaternion.identity);
        soundPlayer.PlaySound(environmentHitSounds[Random.Range(0, environmentHitSounds.Count)],environmentHitVolume);
    }

    private void SetIKTargets()
    {
        rightIKTarget.SetPositionAndRotation(rightHandTarget.position, rightHandTarget.rotation);
        leftIKTarget.SetPositionAndRotation(leftHandTarget.position, leftHandTarget.rotation);
    }

    [ObserversRpc(runLocally:true)]
    private void PlayShotEffect()
    {
        if (muzzleFlash)
        {
            muzzleFlash.Play();
        }

        if (_recoilCoroutine != null)
        {
            StopCoroutine(_recoilCoroutine);
        }

        _recoilCoroutine = StartCoroutine(PlayRecoil());

        if (!automatic)
        {
            if (isOwner)
            {
                shootSoundPlayer.PlayOneShot(shootSounds[Random.Range(0, shootSounds.Count)], shootVolume / 3f);
            }
            else
            {
                shootSoundPlayer.PlayOneShot(shootSounds[Random.Range(0, shootSounds.Count)], shootVolume);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                StartShooting();
            }
        }   
    }

    private bool isShooting = false;

    [ObserversRpc(runLocally:true)]
    void StartShooting()
    {
        if (!isShooting && shootSounds.Count > 0)
        {
            isShooting = true;
            shootSoundPlayer.clip = shootSounds[Random.Range(0, shootSounds.Count)];
            shootSoundPlayer.volume = shootVolume / 3f;
            shootSoundPlayer.loop = true; // Enable looping for continuous fire
            shootSoundPlayer.Play();
        }
    }

    [ObserversRpc(runLocally: true)]
    void StopShooting()
    {
        if (isShooting)
        {
            isShooting = false;
            shootSoundPlayer.Stop();
        }
    }

    private IEnumerator PlayRecoil()
    {
        float elapsed = 0f;

        while (elapsed < recoilDuration)
        {
            elapsed += Time.deltaTime;
            float curveTime = elapsed / recoilDuration;

            //Position Recoil
            float recoilValue = recoilCurve.Evaluate(curveTime);
            Vector3 recoilOffset = Vector3.back * (recoilValue * recoilStrength);
            transform.localPosition = _originalPosition + recoilOffset;

            //Rotation Recoil
            float rotationValue = rotationCurve.Evaluate(curveTime);
            Vector3 rotationOffset = new Vector3(rotationValue + rotationAmount, 0f, 0f);
            transform.localRotation = _originalRotation * Quaternion.Euler(rotationOffset);

            yield return null;
        }

        transform.localPosition = _originalPosition;
        transform.localRotation = _originalRotation;
    }
}
