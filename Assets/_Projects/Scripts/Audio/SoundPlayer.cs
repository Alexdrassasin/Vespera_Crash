using PurrNet;
using System.Collections;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(gameObject, clip.length + 0.1f);
        //StartCoroutine(ReturnToPoolAfter(clip.length + 0.1f)); // Use pool instead of Destroy()
    }
    
    private IEnumerator ReturnToPoolAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        InstanceHandler.GetInstance<ObjectPoolManager>().ReturnToPool_SoundPlayer(this); 
    }
}
