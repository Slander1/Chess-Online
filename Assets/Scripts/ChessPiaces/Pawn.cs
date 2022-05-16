using System.Collections.Generic;
using UnityEngine;


public class Pawn : ChessPiece
{
    protected override List<Vector2Int> GetSteps(ChessPiece[,] board)
    {
        var steps = new List<Vector2Int>();
        var direction = team == 0 ? 1 : -1;
        
        if (currentPos.y == (team == 0 ? 1 : 6) && board[currentPos.x,currentPos.y+direction*2] == null && 
            board[currentPos.x,currentPos.y+direction] == null)
            steps.Add(new Vector2Int(0, direction*2));
        for (int i = -1; i <= 1; i++)
        {
            if (CheckBoard(currentPos.x+i, currentPos.y+direction)) continue;
            var chessPiaces = board[currentPos.x+i, currentPos.y+direction];
            var isMoveForward = (i==0);
            if ((chessPiaces == null) == isMoveForward && (!isMoveForward || (chessPiaces?.team != team)))
                steps.Add(new Vector2Int(i, direction));
        }
        return steps;
    }

}