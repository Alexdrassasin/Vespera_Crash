using NUnit.Framework;
using PurrNet;
using PurrNet.StateMachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
        if (playerHitEffect && player && player.transform)
        {
            Instantiate(playerHitEffect, player.transform.TransformPoint(localposition), Quaternion.LookRotation(normal));
        }
    }

    [ObserversRpc(runLocally:true)]
    private void EnvironmentHit(Vector3 position, Vector3 normal)
    {
        if (environmentHitEffect)
        {
            Instantiate(environmentHitEffect, position, Quaternion.LookRotation(normal));
        }
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
