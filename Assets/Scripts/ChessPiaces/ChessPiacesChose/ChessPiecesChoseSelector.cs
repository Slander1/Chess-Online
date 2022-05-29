using System;
using System.Collections.Generic;
using ChessPiaces;
using UnityEngine;

public class ChessPiecesChoseSelector : MonoBehaviour
{
    [SerializeField] ChessPiacesChosen[] chosenPrefabs;
    private List<ChessPiacesChosen> _chosePieces = new List<ChessPiacesChosen>();
    private Camera _currentCamera;
    private const string CHOSENPIACE = "ChosenPiace";
    private Action<ChessPiece.Type> _action;
    
    private void Awake()
    {
        _currentCamera = Camera.main;
    }

    public void SpawnPiecesForChoose(Material material, Action<ChessPiece.Type> action)
    {
        _action = action;
        var startX = -15;
        var sizeX = 22;
        for (int i = 0; i < chosenPrefabs.Length; i++)
        {
            var posX = startX + i * ((float)sizeX / (chosenPrefabs.Length-1)); 
            var item = Instantiate(chosenPrefabs[i], transform);
            item.transform.position = new Vector3(posX, 60, -40);
            item.material = material;
            _chosePieces.Add(item);
        }
    }

    private void Update()
    {
        var ray = _currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var info, 100, LayerMask.GetMask(CHOSENPIACE)))
        {
            if (Input.GetMouseButtonDown(0))
            {
                
                if (info.transform.TryGetComponent<ChessPiacesChosen>(out var piece))
                    SwapPawn(piece.type);
            }
        }
    }
    
    private void SwapPawn(ChessPiece.Type pieceType)
    {
        _action?.Invoke(pieceType);
        foreach (var piace in _chosePieces)
        {
            Destroy(piace.gameObject);
        }
    }
}
