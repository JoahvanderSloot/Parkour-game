using UnityEngine;

public class StartMusic : MonoBehaviour
{
    [SerializeField] bool isMenu;

    private void Start()
    {
        AudioManager.Instance.StopAllSounds();
        if (isMenu)
        {
            AudioManager.Instance.Play("MenuSong");
        }
        else
        {
            AudioManager.Instance.Play("GameSong");
        }
    }
}
