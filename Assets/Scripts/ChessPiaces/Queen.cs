
using System.Collections.Generic;
using UnityEngine;

public class Queen : Direction
{

    protected override List<Vector2Int> _directions  => new List<Vector2Int>{
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1)
    };

   
}
