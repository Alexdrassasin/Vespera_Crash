using PurrNet;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
public class TakeDamageEffect : NetworkBehaviour
{
    public Material screenDamageMat;
    public CinemachineImpulseSource impulseSource;
    public float impulseMultiplier = 0.4f;
    private Coroutine screenDamageTask;

    public void ScreenDamageEffect(float intensity, Vector3 hitDirection)
    {
        if(screenDamageTask != null)
        {
            StopCoroutine(screenDamageTask);
        }
        screenDamageTask = StartCoroutine(screenDamage(intensity,hitDirection));
    }

    private IEnumerator screenDamage(float intensity, Vector3 hitDirection)
    {
        //camera shake
        var velocity = hitDirection;
        impulseSource.GenerateImpulse(velocity.normalized * intensity* impulseMultiplier);

        //screen effect
        var targetRadius = Remap(intensity, 0, 1, 1f, 0.6f);
        float curRadius = 1f; //No Damage
        for(float t= 0; curRadius != targetRadius; t += Time.deltaTime*4f)
        {
            curRadius = Mathf.Lerp(1, targetRadius, t);
            screenDamageMat.SetFloat("_Vignette_radius",curRadius);
            yield return null;
        }

        for (float t = 0; curRadius < 1; t += Time.deltaTime*4f)
        {
            curRadius = Mathf.Lerp(targetRadius, 1, t);
            screenDamageMat.SetFloat("_Vignette_radius", curRadius);
            yield return null;
        }
    }

    private float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return Mathf.Lerp(toMin, toMax, Mathf.InverseLerp(fromMin, fromMax, value));
    }
}
