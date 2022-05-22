using System.Collections.Generic;
using UnityEngine;

namespace ChessPiaces
{
    public class King : ChessPiece
    {
        private List<Vector2Int> Steps => new List<Vector2Int>()
        {
            new Vector2Int(-1, -1), 
            new Vector2Int(0, -1), 
            new Vector2Int(1, -1),
            new Vector2Int(-1, 0),  
            new Vector2Int(1, 0),
            new Vector2Int(-1, 1), 
            new Vector2Int(0, 1), 
            new Vector2Int(1, 1),
        };
    
        protected override List<Vector2Int> GetSteps(ChessPiece[,] board)
        {
            return Steps;
        }
    
    }
}