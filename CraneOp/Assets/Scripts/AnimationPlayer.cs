using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    [SerializeField] Animator animator;
    public void PlayAnimationViaToggle(string toggleName)
    {
        animator.SetTrigger(toggleName);
    }
}
