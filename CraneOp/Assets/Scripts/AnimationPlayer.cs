/*
    This script us used to play animations in the game.
    Currently it only uses toggle but you may use bool, int, etc. by creating more functions.
*/
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    [SerializeField] Animator animator;
    public void PlayAnimationViaToggle(string toggleName)
    {
        animator.SetTrigger(toggleName);
    }
    
    public void PlayAnimationViaInt(string parameterName, int index)
    {
        animator.SetInteger(parameterName, index);
    }
    
    void Awake()
    {
        if (animator == null)
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator not found. Ensure that one is assigned if needed.");
        }
    }
}
