using UnityEngine;

public class DoorAnimationController : MonoBehaviour
{
    [SerializeField]
    Animator doorAnimator, ghostAnimator;

    public void CheckIn()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("CheckIn");
        }
        if (ghostAnimator != null)
        {
            ghostAnimator.SetTrigger("CheckIn");
        }
    }

    public void CheckOut()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("CheckOut");
        }
    }
}
