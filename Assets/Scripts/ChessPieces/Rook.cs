using System.Collections.Generic;
using UnityEngine;

namespace ChessPieces
{
    public class Rook : Direction
    {
        protected override List<Vector2Int> directions => new List<Vector2Int>{
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1)
        };
    }
}