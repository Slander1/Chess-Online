using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
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

    private List<Vector2Int> GetMoves(ChessPiece[,] board)
    {
        var allSteps = GetSteps(board);
        return allSteps.Where(step =>
            {
                var nextStep = currentPos + step;
                return !(nextStep.x >= Chessboard.TILE_COUNT_X ||
                         nextStep.y >= Chessboard.TILE_COUNT_Y ||
                         nextStep.x < 0 || nextStep.y < 0 ||
                         (board[nextStep.x, nextStep.y] != null && board[nextStep.x, nextStep.y].team == team));
            })
            .Select(step => step + currentPos)
            .ToList();
    }
    
    public List<Vector2Int> GetAvailableMoves(ChessPiece[,] board)
    {
        var moves = GetMoves(board);

        return moves.Where(nextPos =>
        { var lastPos = currentPos;

            var lastPiece = board[nextPos.x, nextPos.y];
            board[nextPos.x, nextPos.y] = this;
            board[lastPos.x, lastPos.y] = null;
            currentPos = nextPos;

            var isCheck = IsKingUnderAttack(board);

            currentPos = lastPos;
            board[nextPos.x, nextPos.y] = lastPiece;
            board[lastPos.x, lastPos.y] = this;
            return !isCheck;
        }).ToList();

    }

    private bool IsKingUnderAttack(ChessPiece[,] board)
    {
        King ourKing = null;
        if (this is King iKing)
        {
            ourKing = iKing;
        }
        else
        {
            foreach (var chessPiece in board)
                if (chessPiece != null && chessPiece.team == team && chessPiece is King thereKing)
                {
                    ourKing = thereKing;
                    break;
                }
        }

        foreach (var chessPiece in board)
        {
            if (chessPiece == null || chessPiece.team == team)
                continue;

            if (chessPiece.GetMoves(board).Contains(ourKing.currentPos))
                return true;
        }

        return false;
    }
    
    public void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
            transform.position = desiredPosition;
    }
    
    public void SetScale(Vector3 scale, bool force = false)
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
