using System;
using Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.ServiceLocator;

namespace UI
{
    public class Buttons : ServiceMonoBehaviour
    {
        public Button pauseButton;
        public Image pauseMenu;
        public Image backGroundIMG;
        public Animator menuAnimator;
        public TMP_Text textRematch;
        public TMP_Text textQuit;
    
        public Action<bool, bool> setLocaleGame;
        public Action<bool> onPauseResumeButtonClick;
        public Action onRestartButtonClick;
        public Action onMenuButton;
    
        [SerializeField] private TextMeshProUGUI victoryText;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TMP_InputField addressInput;
    

        private readonly string[] _triggers =
        {
            "StartMenu",
            "HostMenu",
            "OnlineMenu",
            "InGameMenu"
        };



        public Server server;
        public Client client;
    
    
    
        public void OnEnable()
        {
            ServiceL.Get<ChessBoardLogic>().OnCheck += Shah;
            ServiceL.Get<ChessBoardLogic>().OnMate += Victory;
        }

        public void OnDisable()
        {
            ServiceL.Get<ChessBoardLogic>().OnCheck -= Shah;
            ServiceL.Get<ChessBoardLogic>().OnMate -= Victory;
        }
        public void OnLocaleGameButtonClick()
        {
            setLocaleGame?.Invoke(true, true);
            menuAnimator.SetTrigger(_triggers[3]);
        
            server.Init(8007);
            client.Init("127.0.0.1", 8007);
        
        }

        public void OnOnlineGameButtonClick()
        {
            menuAnimator.SetTrigger(_triggers[2]);
        }
    
        public void OnOnlineHostButtonClick()
        {
            setLocaleGame?.Invoke(false, true);
            server.Init(8007);
            client.Init("127.0.0.1", 8007);
            menuAnimator.SetTrigger(_triggers[1]);
        }

        public void OnOnlineConnectButtonClick()
        {
            setLocaleGame?.Invoke(false, false);
            client.Init(addressInput.text, 8007);
        }

        public void OnOnlineHostBackButtonClick()
        {
            server.ShutDown();
            client.ShutDown();
            menuAnimator.SetTrigger(_triggers[0]);
        
        }
    
        public void OnOnlineHostBackHostButtonClick()
        {
            menuAnimator.SetTrigger(_triggers[2]);
            onMenuButton?.Invoke();
        }
        public void OnPauseButtonClick()
        {
            pauseMenu.gameObject.SetActive(true);
            resumeButton.gameObject.SetActive(true);
            exitButton.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);
            onPauseResumeButtonClick?.Invoke(true);
        }

        private void OnResumeClick()
        {
            pauseMenu.gameObject.SetActive(false);
            onPauseResumeButtonClick?.Invoke(false);
        }

        public void OnExitClick()
        {
            Application.Quit();
        }

        public void OnRestartButtonClick()
        {
            onRestartButtonClick?.Invoke();
        }

        private void Victory(int team)
        {
            pauseMenu.gameObject.SetActive(true);
            resumeButton.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);
            victoryText.text = (team == 0) ? "White wins" : "Black wins";
        }
    
        public void Shah(int team)
        {
            pauseMenu.gameObject.SetActive(true);
            resumeButton.gameObject.SetActive(true);
            exitButton.gameObject.SetActive(false);
            restartButton.gameObject.SetActive(false);
            victoryText.text = "Check";
        }


        public void OnLeaveFromGameMenu()
        {
            pauseMenu.gameObject.SetActive(false);
            textQuit.gameObject.SetActive(false);
            textRematch.gameObject.SetActive(false);
            ServiceL.Get<Cameras>().ChangeCamera(0);
            menuAnimator.SetTrigger(_triggers[0]);
        
        }
    }
}
