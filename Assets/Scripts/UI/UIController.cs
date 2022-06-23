using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.ServiceLocator;


namespace UI
{
    public class UIController : ServiceMonoBehaviour
    {
        
        [SerializeField] private Image backGroundIMG;
        [SerializeField] private Animator menuAnimator;
        [SerializeField] private TMP_Text textRematch;
        [SerializeField] private TMP_Text textQuit;
        [SerializeField] private TMP_InputField addressInput;
        

        private readonly string[] _triggers =
        {
            "StartMenu",
            "HostMenu",
            "OnlineMenu",
            "InGameMenu"
        };

        
        private void OnEnable()
        {
            ServiceL.Get<Buttons>().setTrigger += SetTrigger;
            ServiceL.Get<Buttons>().contolActiveObject += ContolActiveObject;
            
            //ServiceL.Get<ChessBoardLogic>().OnCheck += Shah;
            //ServiceL.Get<ChessBoardLogic>().OnMate += Victory;
        }

        private void ContolActiveObject(bool pauseMenu, bool resumeBtn, bool exitBtn, bool restartBtn, string victoryTxt)
        {
            // this.pauseMenu.gameObject.SetActive(pauseMenu);
            // resumeButton.gameObject.SetActive(resumeBtn);
            // exitButton.gameObject.SetActive(exitBtn);
            // restartButton.gameObject.SetActive(true);
            // victoryText.text = victoryTxt;
        }

        private void OnDisable()
        {
            ServiceL.Get<Buttons>().setTrigger -= SetTrigger;
            ServiceL.Get<Buttons>().contolActiveObject -= ContolActiveObject;
            //ServiceL.Get<ChessBoardLogic>().OnCheck -= Shah;
            //ServiceL.Get<ChessBoardLogic>().OnMate -= Victory;
            
        }

        private void SetTrigger(int value)
        {
            menuAnimator.SetTrigger(_triggers[value]);
        }
    }
}