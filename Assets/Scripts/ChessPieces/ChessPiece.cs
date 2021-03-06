using System.Collections.Generic;
using System.Linq;
using GameLogic;
using UnityEngine;
using Utils.ServiceLocator;

namespace ChessPieces
{
    
    public abstract class ChessPiece : MonoBehaviour
    {
        public enum Type
        {
            None = 0,
            Pawn = 1,
            Rook = 2,
            Knight = 3,
            Bishop = 4,
            Queen = 5,
            King = 6
        }
        public MeshRenderer meshRenderer;
        public int team;
        public Vector2Int currentPos;
        public Type type;

        private Vector3 _desiredPosition;
        private Vector3 _desiredScale = Vector3.one;
        
    
        protected abstract List<Vector2Int> GetSteps(ChessPiece[,] board);

        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position, _desiredPosition, Time.deltaTime * 10);
            transform.localScale = Vector3.Lerp(transform.localScale, _desiredScale, Time.deltaTime * 10);
        }

        public List<Vector2Int> GetMoves(ChessPiece[,] board)
        {
            var allSteps = GetSteps(board);
            return allSteps.Where(step =>
                {
                    var nextStep = currentPos + step;
                    return !(nextStep.x >= Tiles.TILE_COUNT_X ||
                             nextStep.y >= Tiles.TILE_COUNT_Y ||
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
            { 
                var lastPos = currentPos;
                var lastPiece = board[nextPos.x, nextPos.y];
                board[nextPos.x, nextPos.y] = this;
                board[lastPos.x, lastPos.y] = null;
                currentPos = nextPos;

                var isCheck = ServiceL.Get<ChessBoardLogic>().IsKingUnderAttack(board, team);

                currentPos = lastPos;
                board[nextPos.x, nextPos.y] = lastPiece;
                board[lastPos.x, lastPos.y] = this;
                return !isCheck;
            }).ToList();
        }

    
    
        public void AnimateMove(Vector3 position, bool force = false)
        {
            _desiredPosition = position;
            if (force)
                transform.position = _desiredPosition;
        }
    
        public void SetScale(Vector3 scale, bool force = false)
        {
            _desiredScale = scale;
            if (force)
                transform.localScale = _desiredScale;
        }
        protected static bool CheckBoard(int x, int y)
        {
            return (x < 0 || y < 0 || x >= Tiles.TILE_COUNT_X || y >= Tiles.TILE_COUNT_Y);
        }
    
    }
}