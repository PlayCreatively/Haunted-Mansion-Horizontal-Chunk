using UnityEngine;

public class DoorAnimationController : MonoBehaviour
{
    [SerializeField]
    Animator doorAnimator, ghostAnimator;

    void Start()
    {
        ghostAnimator.gameObject.SetActive(false);
    }

    public void CheckIn()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("CheckIn");
        }
        if (ghostAnimator != null)
        {
            ghostAnimator.gameObject.SetActive(true);
            ghostAnimator.SetTrigger("CheckIn");
        }
    }

    public void CheckOut()
    {
        if (doorAnimator != null)
        {
            ghostAnimator.gameObject.SetActive(false);
            doorAnimator.SetTrigger("CheckOut");
        }
    }
}
