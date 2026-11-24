/*
    This script is used to handle the SFX in the game.
    It plays the correct and wrong SFX.
    It also allows other scripts to access and use the completed AudioClip as making it subscribe to the onSequenceComplete may not work properly.
*/
using UnityEngine;

public class SFXHandler : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip correct;
    [SerializeField] AudioClip wrong;
    public AudioClip completed;

    public static SFXHandler Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        if (audioSource == null) audioSource = FindAnyObjectByType<AudioSource>();
    }

    void Start()
    {
        Events.Instance.onItemCorrect += () => {Play(correct);};
        Events.Instance.onItemWrong += () => {Play(wrong);};
    }

    public void Play(AudioClip clip)
    {   
        audioSource.volume = 0.2f;
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }
}
