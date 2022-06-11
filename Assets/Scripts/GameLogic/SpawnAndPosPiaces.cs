using System.Collections.Generic;
using ChessPiaces;
using UnityEngine;

namespace GameLogic
{
    public class SpawnAndPosPiaces : MonoBehaviour
    {
        
        [SerializeField] private ChessPiece[] figurePrefabs;
        [SerializeField] public Material[] teamMaterials;
        
        public static SpawnAndPosPiaces Instance { set; get; }

        private void Awake()
        {
            Instance = this;
        }

        public void SpawnAllPieces(ChessPiece[,] chessPieces)
        {
            var typesOfPiecesWhite = new List<ChessPiece.Type>()
            {
                ChessPiece.Type.Rook, ChessPiece.Type.Knight,ChessPiece.Type.Bishop,ChessPiece.Type.Queen,ChessPiece.Type.King,
                ChessPiece.Type.Bishop,ChessPiece.Type.Knight,ChessPiece.Type.Rook
            };
            var typesOfPiecesBlack = typesOfPiecesWhite;
            typesOfPiecesBlack.Reverse();
            var whiteTeam = 0;
            var blackTeam = 1;
            for (var i = 0; i < 8; i++)
            {
                chessPieces[i, 0] = SpawnSinglePiece(typesOfPiecesWhite[i], whiteTeam);
                chessPieces[i, 7] = SpawnSinglePiece(typesOfPiecesBlack[i], blackTeam);
            }

            for (var i = 0; i < 8; i++)
            {
                chessPieces[i, 1] = SpawnSinglePiece(ChessPiece.Type.Pawn, whiteTeam);
                chessPieces[i, 6] = SpawnSinglePiece(ChessPiece.Type.Pawn, blackTeam);
            }

        }

        public ChessPiece SpawnSinglePiece(ChessPiece.Type type, int team)
        {
            var cp = Instantiate(figurePrefabs[(int)type - 1], transform);
            cp.type = type;
            cp.team = team;
            cp.meshRenderer.material = teamMaterials[team];
            return cp;
        }


        public void PositionAllPieces(ChessPiece[,] chessPieces)
        {
            for (var x = 0; x < Tiles.Instance.TILE_COUNT_X; x++)
            for (var y = 0; y < Tiles.Instance.TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                    SetPiecePos(chessPieces,new Vector2Int(x, y), true);
        }

        public void SetPiecePos(ChessPiece[,] chessPieces, Vector2Int pos, bool force = false)
        {
            chessPieces[pos.x, pos.y].currentPos = pos;
            chessPieces[pos.x, pos.y].AnimateMove(Tiles.Instance.GetTileCenter(pos), force);
        }
        
        
    }
}