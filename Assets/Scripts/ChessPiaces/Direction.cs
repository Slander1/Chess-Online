using System.Collections.Generic;
using UnityEngine;

namespace ChessPiaces
{
    public abstract class Direction : ChessPiece
    {
        protected abstract List<Vector2Int> _directions { get; }

        protected override List<Vector2Int> GetSteps(ChessPiece[,] board)
        {
            var steps = new List<Vector2Int>();

            foreach (var direction in _directions)
            {
                for (int i = 1; i < Chessboard.TILE_COUNT_X; i++)
                {
                    var posStep = currentPos + direction * i;
                    if (!GetValue(board, posStep.x, posStep.y, steps)) break; 
                }
            }

            return steps;
        }

        private bool GetValue(ChessPiece[,] board, int x, int y, List<Vector2Int> steps)
        {
            if (CheckBoard(x,y) || board[x, y]?.team == team)
                return false;

            steps.Add(new Vector2Int(x - currentPos.x, y - currentPos.y));

            return board[x, y] == null;
        }
    }
}
