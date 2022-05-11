using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Buttons : MonoBehaviour
{
    [SerializeField] private Image pauseMenu;
    [SerializeField] private TextMeshProUGUI victorytext;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Animator MenuAnimator;
    [SerializeField] private TMP_InputField addressInput;

    private string[] triggers =
    {
        "StartMenu",
        "HostMenu",
        "OnlineMenu",
        "InGameMenu"
    };


    public Buttons Instance { set; get; }


    public Server server;
    public Client client;
    private void Awake()
    {
        Instance = this;
    }

    public void OnEnable()
    {
        Chessboard.OnCheck += Shah;
        Chessboard.OnMate += Victory;
    }

    public void OnLocaleGameButtonClick()
    {
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
        server.Init(8007);
        client.Init("127.0.0.1", 8005);
        MenuAnimator.SetTrigger(triggers[1]);
    }

    public void OnOnlineConnectButtonClick()
    {
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
