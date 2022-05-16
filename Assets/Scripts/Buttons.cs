using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CameraAngle
{
    menu = 0,
    whiteTeam = 1,
    blackTeam = 2
}

public class Buttons : MonoBehaviour
{
    [SerializeField] private Image pauseMenu;
    [SerializeField] private TextMeshProUGUI victorytext;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;
    public Animator MenuAnimator;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private GameObject[] cameraAngles;

    public Action<bool> setLocaleGame;

    private string[] triggers =
    {
        "StartMenu",
        "HostMenu",
        "OnlineMenu",
        "InGameMenu"
    };


    public static Buttons Instance { set; get; }


    public Server server;
    public Client client;
    public void Awake()
    {
        Instance = this;
    }

    public void ChangeCamera(CameraAngle index)
    {
        for (int i = 0; i < cameraAngles.Length; i++)
        {
            cameraAngles[i].SetActive(false);
        }
        
        cameraAngles[(int)index].SetActive(true);
    }
    
    public void OnEnable()
    {
        Chessboard.OnCheck += Shah;
        Chessboard.OnMate += Victory;
    }

    public void OnDisable()
    {
        Chessboard.OnCheck -= Shah;
        Chessboard.OnMate -= Victory;
    }
    public void OnLocaleGameButtonClick()
    {
        setLocaleGame?.Invoke(true);
        MenuAnimator.SetTrigger(triggers[3]);
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
    }

    public void OnOnlineGameButtonClick()
    {
        MenuAnimator.SetTrigger(triggers[2]);
    }
    
    public void OnOnlineHostButtonClick()
    {
        setLocaleGame?.Invoke(false);
        server.Init(8007);
        client.Init("127.0.0.1", 8007); // Не забыть поменять
        MenuAnimator.SetTrigger(triggers[1]);
    }

    public void OnOnlineConnectButtonClick()
    {
        setLocaleGame?.Invoke(false);
        client.Init(addressInput.text, 8007);
    }

    public void OnOnlineHostBackButtonClick()
    {
        server.ShutDown();
        client.ShutDown();
        MenuAnimator.SetTrigger(triggers[0]);
    }
    
    public void OnOnlineHostBackHostButtonClick()
    {
        MenuAnimator.SetTrigger(triggers[2]);
    }
    public void OnPauseButtonClick()
    {
        pauseMenu.gameObject.SetActive(true);
        resumeButton.gameObject.SetActive(true);
        exitButton.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
    }

    public void OnResumeClick()
    {
        pauseMenu.gameObject.SetActive(false);
    }

    public void OnExitClick()
    {
        Application.Quit();
    }

    public void Victory(int team)
    {
        pauseMenu.gameObject.SetActive(true);
        resumeButton.gameObject.SetActive(false);
        victorytext.text = (team == 0) ? "White wins" : "Black wins";
    }
    
    public void Shah(int team)
    {
        pauseMenu.gameObject.SetActive(true);
        resumeButton.gameObject.SetActive(true);
        exitButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        victorytext.text = "Shah";
    }

 

}
