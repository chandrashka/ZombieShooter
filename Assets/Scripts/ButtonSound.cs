using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sound;
    public void OnClick()
    {
        audioSource.PlayOneShot(sound);
    }
}
