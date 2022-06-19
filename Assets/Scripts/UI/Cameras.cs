using UnityEngine;
using Utils.ServiceLocator;

namespace UI
{
    public class Cameras : ServiceMonoBehaviour
    {
        [SerializeField] private GameObject[] camerasAngles;
        public enum CameraAngle
        {
            menu = 0,
            whiteTeam = 1,
            blackTeam = 2
        }
        public void ChangeCamera(CameraAngle index)
        {
            foreach (var camera in camerasAngles)
                camera.SetActive(false);

            camerasAngles[(int)index].SetActive(true);
        }
        
    }
}