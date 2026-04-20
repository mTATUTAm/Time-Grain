using UnityEngine;

public class CreditManager : MonoBehaviour
{
    public void OnBackButton()
    {
        SceneTransitionManager.Instance.FadeToScene("Title");
    }
}
