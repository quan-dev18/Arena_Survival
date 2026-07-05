using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private AudioSource MusicSource;
    void Awake()
    {
        MusicSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        MusicSource.Play();
        MusicSource.loop = true;
    }
    public void StartGame()
    {
        SceneManager.LoadScene("MainGame");
    }
    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
