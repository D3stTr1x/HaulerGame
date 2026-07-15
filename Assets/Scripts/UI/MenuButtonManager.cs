using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButtonManager : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private string gameDemoSceneName = "Demo";
    [SerializeField] private string gameSceneName = "CItyScene 1";
    [SerializeField] private GameObject scriptToDisableMusic;
    [SerializeField] private GameObject scriptToDisableSound;
    private AudioSource soundController;
    private AudioSource musicController;

    [SerializeField] private Image menuImg;
    [SerializeField] private Button menuButton1;
    [SerializeField] private Button menuButton2;
    [SerializeField] private Button menuButton3;
    [SerializeField] private Button menuButton4;
    [SerializeField] private Button menuButton5;


    private string scene;
    private bool checkMenu = false;

    private void Start()
    {
        if (scriptToDisableSound != null)
        {
            soundController = scriptToDisableSound.GetComponent<AudioSource>();
            Debug.Log("soundScript: " + soundController);
        }

        if (scriptToDisableMusic != null)
        {
            musicController = scriptToDisableMusic.GetComponent<AudioSource>();
            Debug.Log("musicScript: " + musicController);
        }
    }

    private void Update()
    {
        scene = SceneManager.GetActiveScene().name;
        OpenMenu();
    }

    public void ContnueBut()
    {
        menuImg.gameObject.SetActive(false);
        menuButton1.gameObject.SetActive(false);
        menuButton2.gameObject.SetActive(false);
        menuButton3.gameObject.SetActive(false);
        menuButton4.gameObject.SetActive(false);
        menuButton5.gameObject.SetActive(false);

        checkMenu = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void StopStartSound()
    {
        if (soundController != null)
        {
            soundController.enabled = !soundController.enabled;
            Debug.Log("Car Audio Controller теперь: " + soundController.enabled);
        }
    }

    public void StopStartMusic()
    {
        if (musicController != null)
        {
            musicController.enabled = !musicController.enabled;
            Debug.Log("Car Audio Controller теперь: " + musicController.enabled);
        }
    }
        

    public void ToMenu()
    {
        menuImg.gameObject.SetActive(false);
        menuButton1.gameObject.SetActive(false);
        menuButton2.gameObject.SetActive(false);
        menuButton3.gameObject.SetActive(false);
        menuButton4.gameObject.SetActive(false);
        menuButton5.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(menuSceneName);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void LoadGameDemoScene()
    {
        SceneManager.LoadScene(gameDemoSceneName);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void OpenMenu()
    {
        if ((scene != "MainMenu") && (!checkMenu) && (Input.GetKeyDown(KeyCode.Escape)))
        {
            menuImg.gameObject.SetActive(true);
            menuButton1.gameObject.SetActive(true);
            menuButton2.gameObject.SetActive(true);
            menuButton3.gameObject.SetActive(true);
            menuButton4.gameObject.SetActive(true);
            menuButton5.gameObject.SetActive(true);

            menuButton1.interactable = true;
            menuButton2.interactable = true;
            menuButton3.interactable = true;
            menuButton4.interactable = true;
            menuButton5.interactable = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            checkMenu = true;
        }
        else if ((checkMenu) && (Input.GetKeyDown(KeyCode.Escape)))
        {
            ContnueBut();
        }
    }
}