using System;
using Net;
using UnityEngine;
using UnityEngine.UI;
using Utils.ServiceLocator;
using static UI.PopUpManager;

namespace UI
{
    public class Buttons : ServiceMonoBehaviour
    {
        public Button pauseButton; 
        public Action<bool, bool> setLocaleGame;
        public Action<int> setTrigger;
        public Action<bool> onPauseResumeButtonClick;
        public Action onRestartButtonClick;
        public Action onMenuButton;
        public Action shutDown;

        public void OnLocaleGameButtonClick()
        {
            setLocaleGame?.Invoke(true, true);
            setTrigger?.Invoke(3);
            
        }

        public void OnOnlineGameButtonClick()
        {
            setTrigger?.Invoke(2);
        }
    
        public void OnOnlineHostButtonClick()
        {
            setLocaleGame?.Invoke(false, true);
            setTrigger?.Invoke(1);
        }

        public void OnOnlineConnectButtonClick()
        {
            setLocaleGame?.Invoke(false, false);
            // client.Init(addressInput.text, 8007);
        }

        public void OnOnlineHostBackButtonClick()
        {
            shutDown?.Invoke();
            setTrigger?.Invoke(0);
        }
    
        public void OnOnlineHostBackHostButtonClick()
        {
            setTrigger(2);
            onMenuButton?.Invoke();
        }
        public void OnPauseButtonClick()
        {
            ServiceL.Get<PopUpManager>().ShowPopUP(PopUpsName.PauseMenu, transform);
            onPauseResumeButtonClick?.Invoke(true);
        }

        private void OnResumeClick()
        {
            // pauseMenu.gameObject.SetActive(false);
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

        
    
        

        public void OnLeaveFromGameMenu()
        {
            //contolActiveObject.Invoke(true,true,false,false,"Check");
            // pauseMenu.gameObject.SetActive(false);
            // textQuit.gameObject.SetActive(false);
            // textRematch.gameObject.SetActive(false);
            // ServiceL.Get<Cameras>().ChangeCamera(0);
            // menuAnimator.SetTrigger(_triggers[0]);
        }
    }
}
