using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}


public abstract class ChessPiece : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public int team;
    public Vector2Int currentPos;
    public ChessPieceType type;
    

    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;
        
    
    protected abstract List<Vector2Int> GetSteps(ChessPiece[,] board);

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }
    
    public List<Vector2Int> GetAvailableMoves(ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        var allSteps = GetSteps(board);
        allSteps = allSteps.Where(step =>
            {
                var nextStep = currentPos + step;
                if (nextStep.x >= tileCountX || nextStep.y >= tileCountY || nextStep.x < 0 || nextStep.y < 0)
                    return false;
                return (board[nextStep.x, nextStep.y] == null || board[nextStep.x, nextStep.y].team != team);
            })
            .Select(step => step + currentPos)
            .ToList();


        return allSteps;
    }
    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
            transform.position = desiredPosition;
    }
    
    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
            transform.localScale = desiredScale;
    }
    protected static bool CheckBoard(int x, int y)
    {
        return (x < 0 || y < 0 || x >= Chessboard.TILE_COUNT_X || y >= Chessboard.TILE_COUNT_Y);
    }
    
}
