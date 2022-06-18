using System.Collections.Generic;
using UnityEngine;

namespace ChessPieces
{
    public class Bishop : Direction
    {
        protected override List<Vector2Int> _directions => new List<Vector2Int>
        {
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };
    };
}