
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Knight : ChessPiece
{
    private List<Vector2Int> Steps => new List<Vector2Int>()
    {
        new Vector2Int(2, 1),
        new Vector2Int(1, 2), 
        new Vector2Int(-1, 2),
        new Vector2Int(-2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(1, -2), 
        new Vector2Int(-1, -2),
        new Vector2Int(-2, -1),
    };

    protected override List<Vector2Int> GetSteps(ChessPiece[,] board)
    {
        return Steps;
    }
}