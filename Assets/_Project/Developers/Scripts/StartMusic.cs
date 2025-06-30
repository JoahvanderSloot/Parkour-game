using UnityEngine;

public class StartMusic : MonoBehaviour
{
    [SerializeField] bool isMenu;

    private void Start()
    {
        if (isMenu)
        {
            AudioManager.Instance.Stop("GameSong");
            AudioManager.Instance.Stop("Run");

            if (!AudioManager.Instance.IsPlaying("MenuSong")) 
            {
                AudioManager.Instance.Play("MenuSong");
            }
        }
        else
        {
            AudioManager.Instance.Stop("MenuSong");

            if (!AudioManager.Instance.IsPlaying("GameSong"))
            {
                AudioManager.Instance.Play("GameSong");
            }
        }
    }
}
