using System.Collections.Generic;
using System.Threading.Tasks;
using Net;
using UnityEngine;
using Utils.ServiceLocator;

namespace GameLogic
{
    public class Tiles : ServiceMonoBehaviour
    {
        public static float YOffset = 0.01f;
        public Vector3 bounds;
        public GameObject[,] tiles;
        [SerializeField]private Material tileMaterial;
        private readonly Vector3 _boardCenter = Vector3.zero;
        public static int TILE_COUNT_X = 8;
        public static int TILE_COUNT_Y = 8;

        public const string HOVER = "Hover";
        public const string TILE = "Tile";
        public const string HIGLIGHT = "Hightlight";
        public float tileSize = 10.0f;

        private async void OnEnable()
        {
            await Task.Delay(1000);
            ServiceL.Get<ChessBoardLogic>().lockUpTile += LockupTileIndex;
            ServiceL.Get<ChessBoardLogic>().swapTileLayer += SwapTileHover;
            ServiceL.Get<ChessBoardLogic>().highlightTiles += HighlightTiles;
            ServiceL.Get<ChessBoardLogic>().getTileCenter += GetTileCenter;
            ServiceL.Get<ChessBoardLogic>().removeHighlightTiles += RemoveHighlightTiles;
        }

        private void OnDisable()
        {
            ServiceL.Get<ChessBoardLogic>().lockUpTile -= LockupTileIndex;
            ServiceL.Get<ChessBoardLogic>().swapTileLayer -= SwapTileHover;
            ServiceL.Get<ChessBoardLogic>().highlightTiles -= HighlightTiles;
            ServiceL.Get<ChessBoardLogic>().getTileCenter -= GetTileCenter;
        }

        private void SwapTileHover(Vector2Int pos, string Hover)
        {
            tiles[pos.x, pos.y].layer = LayerMask.NameToLayer(Hover);
        }

        public void GenerateAllTiles(int tileCountX, int tileCountY, Transform transform)
        {
            YOffset += transform.position.y;
            bounds = new Vector3((tileCountX * 0.5f) * tileSize, 0, (tileCountX * 0.5f) * tileSize) + _boardCenter;
            tiles = new GameObject[tileCountX, tileCountY];
            for (var x = 0; x < tileCountX; x++)
            for (var y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y, transform);
        }


        private GameObject GenerateSingleTile(float tileSize, int x, int y, Transform transform)
        {
            var tileObject = new GameObject($"X:{x}, Y:{y}");
            tileObject.transform.parent = transform;
            var mesh = new Mesh();
            tileObject.AddComponent<MeshFilter>().mesh = mesh;
            tileObject.AddComponent<MeshRenderer>().material = tileMaterial;
            var vertices = new Vector3[4]
            {
                new Vector3(x * tileSize, YOffset, y * tileSize) - bounds,
                new Vector3(x * tileSize, YOffset, (y + 1) * tileSize) - bounds,
                new Vector3((x + 1) * tileSize, YOffset, (y) * tileSize) - bounds,
                new Vector3((x + 1) * tileSize, YOffset, (y + 1) * tileSize) - bounds
            };

            var tris = new[] { 0, 1, 2, 1, 3, 2 };
            mesh.vertices = vertices;
            mesh.triangles = tris;

            mesh.RecalculateNormals();
            tileObject.layer = LayerMask.NameToLayer(TILE);
            tileObject.AddComponent<BoxCollider>();

            return tileObject;
        }


        public void RemoveHighlightTiles(List<Vector2Int> availableMoves)
        {
            foreach (var movePos in availableMoves)
                tiles[movePos.x, movePos.y].layer = LayerMask.NameToLayer(TILE);

            availableMoves.Clear();
        }

        public Vector3 GetTileCenter(Vector2Int pos)
        {
            return new Vector3(pos.x * tileSize, YOffset + 0.2f, pos.y * tileSize) - bounds +
                   new Vector3(tileSize / 2, 0, tileSize / 2);
        }


        private void HighlightTiles(List<Vector2Int> availableMoves)
        {
            foreach (var movePos in availableMoves)
            {
                tiles[movePos.x, movePos.y].layer = LayerMask.NameToLayer(HIGLIGHT);
            }
        }

        private Vector2Int LockupTileIndex(GameObject hitInfo)
        {
            for (var x = 0; x < TILE_COUNT_X; x++)
            for (var y = 0; y < TILE_COUNT_Y; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);

            return -Vector2Int.one;
        }
    }
}
