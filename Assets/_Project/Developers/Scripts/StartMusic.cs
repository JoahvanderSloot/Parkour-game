using UnityEngine;

public class StartMusic : MonoBehaviour
{
    [SerializeField] bool isMenu;

    private void Start()
    {
        AudioManager.Instance.StopAllSounds();
        if (isMenu)
        {
            if (!AudioManager.Instance.IsPlaying("MenuSong")) 
            {
                AudioManager.Instance.Play("MenuSong");
            }
            else
            {
                Debug.Log("Sus");
            }
        }
        else
        {
            if (!AudioManager.Instance.IsPlaying("GameSong"))
            {
                AudioManager.Instance.Play("GameSong");
            }
        }
    }
}
