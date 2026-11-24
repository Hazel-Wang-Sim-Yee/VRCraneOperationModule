/*
    Plays the signalman signal animation with its corresponding audio based on the current signal.
    Note: Please replace the animations in the animator with the actual animations for the signalman.
    The current animations are placeholders. If the animator breaks, redo the entire setup for the signalman animator.
*/

using UnityEngine;

[RequireComponent(typeof(AnimationPlayer))]
public class SignalmanAnimator : MonoBehaviour
{
    AnimationPlayer animationPlayer;
    [SerializeField] AudioClip[] signalAudios; // Array to hold the audio clips for each signal
    [SerializeField] AudioSource audioSource; // Audio source to play the signal audios
    string lastSignal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        animationPlayer = GetComponent<AnimationPlayer>();
        if (animationPlayer == null)
        {
            Debug.LogError("Unable to find animation player. Please assign it to allow the signalman to have animation.");
        }
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("Unable to find audio source. Please assign it to allow the signalman to have audio.");
            }
        }
    }
    static readonly string[] signalNames = { "Start", "Stop", "Up", "Down", "East", "South", "North", "West" };

    void Update()
    {
        var sm = SignalmanManager.Instance;
        string signal = (sm != null && sm.isLoadAttached) ? sm.currentSignal : "Idle";
        if (signal == lastSignal) return;
        lastSignal = signal;

        int index = System.Array.IndexOf(signalNames, signal);   // -1 if not found
        animationPlayer.PlayAnimationViaInt("Signal", index + 1);  // 0 = idle, 1..8 match your ids
        PlaySignalAudio(index);
    }

    /// <summary>
    /// Play the signal audio based on the index.
    /// </summary>
    /// <param name="index"></param>
    void PlaySignalAudio(int index)
    {
        audioSource.Stop();
        if (index < 0 || index >= signalAudios.Length) return;
        audioSource.PlayOneShot(signalAudios[index]);
    }
}
