using System;
using GameLogic;
using UnityEngine;
using Utils.ServiceLocator;

public class RayCaster : ServiceMonoBehaviour
{
    private Camera _currentCamera;

    public event Action<Ray, RaycastHit> onRaycastLayer;
    public event Action<Ray, RaycastHit> onRaycastWithoutLayer;
    
    private void Start()
    {
        _currentCamera = Camera.main;
    }

    private void Update()
    {
        var ray = _currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var info, 100, LayerMask.GetMask(Tiles.TILE, Tiles.HOVER, Tiles.HIGLIGHT)))
            onRaycastLayer?.Invoke(ray, info);
        else 
            onRaycastWithoutLayer?.Invoke(ray, info);
        
    }
    
}