using UnityEngine;

public class SFXHandler : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip correct;
    [SerializeField] AudioClip wrong;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        Events.Instance.onItemCorrect += PlayCorrect;
        Events.Instance.onItemWrong += PlayWrong;
    }

    void PlayCorrect() => Play(correct);
    void PlayWrong() => Play(wrong);

    void Play(AudioClip clip)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }
}
