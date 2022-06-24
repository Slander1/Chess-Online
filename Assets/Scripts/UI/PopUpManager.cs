using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.ServiceLocator;

namespace UI
{
    public enum PopUpsName
    {
        PauseMenu,
        WhiteWin,
        BlackWin,
        Check
    }
    

    public class PopUpManager : ServiceMonoBehaviour
    {

        [SerializeField] private List<GameObject> popUpObjects = new List<GameObject>();
        
        private readonly List<GameObject> _nowActivePopUp = new List<GameObject>();

        public void ShowPopUP(PopUpsName popUpName, Transform parent)
        {
            foreach (var popUp in popUpObjects)
            {
                if (popUp.name == popUpName.ToString())
                {
                    var curPopUp = Instantiate(popUp, parent);
                    _nowActivePopUp.Add(curPopUp);
                    return;
                }
            }
        }

        public void HidePopUp(PopUpsName popUpName)
        {
            var deletedObject = _nowActivePopUp.First(popUp => popUp.name == popUpName.ToString());
            _nowActivePopUp.Remove(deletedObject);
            Destroy(deletedObject);
        }

        public void HideAllPopUps()
        {
            foreach (var popUp in _nowActivePopUp)
                Destroy(popUp);
            
            _nowActivePopUp.Clear();
        }
        
    }
}