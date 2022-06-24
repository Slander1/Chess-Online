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
            
            ServiceL.Get<ChessBoardLogic>().OnCheck += Check;
            ServiceL.Get<ChessBoardLogic>().OnMate += Victory;
        }
        

        private void OnDisable()
        {
            ServiceL.Get<Buttons>().setTrigger -= SetTrigger;
            ServiceL.Get<ChessBoardLogic>().OnCheck -= Check;
            ServiceL.Get<ChessBoardLogic>().OnMate -= Victory;
            
        }

        private void Check(int team)
        {
            ServiceL.Get<PopUpManager>().ShowPopUP(PopUpsName.Check, transform);
        }
        private void Victory(int team)
        {
            ServiceL.Get<PopUpManager>().ShowPopUP((team == 0) ? PopUpsName.WhiteWin : PopUpsName.BlackWin, transform);
        }
        private void SetTrigger(int value)
        {
            menuAnimator.SetTrigger(_triggers[value]);
        }
    }
}