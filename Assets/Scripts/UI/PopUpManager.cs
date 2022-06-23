using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class PopUpManager : MonoBehaviour
    {
        [SerializeField] public List<GameObject> popUpObjects = new List<GameObject>();
        public static readonly Dictionary<string, GameObject> PopUps = new Dictionary<string, GameObject>();

        private void Awake()
        {
            foreach (var popUp in popUpObjects)
            {
                PopUps.Add(popUp.name, popUp);
            }
        }
        public static void ShowPopUP(string popUpName, string whoWin = "")
        {
            if (!PopUps.ContainsKey(popUpName))
                return;
            var popUp = PopUps.First(popUp => 
                popUp.Key == popUpName);
            Instantiate(popUp.Value);
        }
    }
}