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

    // Update is called once per frame
    void Update()
    {
        var sm = SignalmanManager.instance;
        string signal = (sm != null && sm.isLoadAttached) ? sm.currentSignal : "Idle";
        if (signal == lastSignal)
        return;
        lastSignal = signal;
        animationPlayer.PlayAnimationViaInt("Signal", SignalId(signal));
        Debug.Log($"Playing audio: {audioSource.clip?.name ?? "None"} for signal: {signal}");
    }
    
    /// <summary>
    /// Set the int by the signal which is used to set the int in the animator.
    /// </summary>
    /// <param name="signal"></param>
    /// <returns></returns>
    int SignalId(string signal)
    {
        switch (signal)
        {
            case "Start":
            audioSource.clip = signalAudios[0];
            audioSource.Play();
            return 1;
            case "Stop":
            audioSource.clip = signalAudios[1];
            audioSource.Play();
            return 2;
            case "Up":
            audioSource.clip = signalAudios[2];
            audioSource.Play();
            return 3;
            case "Down":
            audioSource.clip = signalAudios[3];
            audioSource.Play();
            return 4;
            case "Left":
            audioSource.clip = signalAudios[4];
            audioSource.Play();
            return 5;
            case "Right":
            audioSource.clip = signalAudios[5];
            audioSource.Play();
            return 6;
            case "Front":
            audioSource.clip = signalAudios[6];
            audioSource.Play();
            return 7;
            case "Back":
            audioSource.clip = signalAudios[7];
            audioSource.Play();
            return 8;
            default:
            audioSource.clip = null;
            audioSource.Stop();
            return 0;
        }
    }
}
