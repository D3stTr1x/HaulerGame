using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButtonManager : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "Demo";

    [SerializeField] private Image menuImg;
    [SerializeField] private Button menuButton1;
    [SerializeField] private Button menuButton2;
    [SerializeField] private Button menuButton3;


    private string scene;
    private bool checkMenu = false;

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

        checkMenu = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ToMenu()
    {
        menuImg.gameObject.SetActive(false);
        menuButton1.gameObject.SetActive(false);
        menuButton2.gameObject.SetActive(false);
        menuButton3.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

//<<<<<<< HEAD
//        //SceneTransition.SwitchToScene(menuSceneName);
//=======
        SceneManager.LoadScene(menuSceneName);
//>>>>>>> 8a753caf606f4f12d1977cd188f184b4890ebb82
    }

    public void LoadGameScene()
    {
//<<<<<<< HEAD
//        //SceneTransition.SwitchToScene(gameSceneName);
//=======
        SceneManager.LoadScene(gameSceneName);
//>>>>>>> 8a753caf606f4f12d1977cd188f184b4890ebb82
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

            menuButton1.interactable = true;
            menuButton2.interactable = true;
            menuButton3.interactable = true;

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