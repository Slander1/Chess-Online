using Assets.Scripts.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public class Tiles : SingletonBehaviour<Tiles>
    {
        public float yOffset = 0.2f;
        public Vector3 bounds;
        public GameObject[,] tiles;
        [SerializeField]private Material tileMaterial;
        private Vector3 _boardCenter = Vector3.zero;
        public readonly int TILE_COUNT_X = 8;
        public readonly int TILE_COUNT_Y = 8;

        public const string HOVER = "Hover";
        public const string TILE = "Tile";
        public const string HIGLIGHT = "Hightlight";
        public float tileSize = 10.0f;
        
        public void GenerateAllTiles(int tileCountX, int tileCountY, Transform transform)
        {
            yOffset += transform.position.y;
            bounds = new Vector3((tileCountX * 0.5f) * tileSize, 0, (tileCountX * 0.5f) * tileSize) + _boardCenter;
            tiles = new GameObject[tileCountX, tileCountY];
            for (var x = 0; x < tileCountX; x++)
            for (var y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y, transform);
        }


        private GameObject GenerateSingleTile(float tileSize, int x, int y, Transform transform)
        {
            GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
            tileObject.transform.parent = transform;
            var mesh = new Mesh();
            tileObject.AddComponent<MeshFilter>().mesh = mesh;
            tileObject.AddComponent<MeshRenderer>().material = tileMaterial;
            var vertices = new Vector3[4]
            {
                new Vector3(x * tileSize, yOffset, y * tileSize) - bounds,
                new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds,
                new Vector3((x + 1) * tileSize, yOffset, (y) * tileSize) - bounds,
                new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds
            };

            int[] tris = { 0, 1, 2, 1, 3, 2 };
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
            return new Vector3(pos.x * tileSize, yOffset + 0.2f, pos.y * tileSize) - bounds +
                   new Vector3(tileSize / 2, 0, tileSize / 2);
        }


        public void HighlightTiles(List<Vector2Int> availableMoves)
        {
            foreach (var movePos in availableMoves)
            {
                tiles[movePos.x, movePos.y].layer = LayerMask.NameToLayer(HIGLIGHT);
            }
        }

        public Vector2Int LockupTileIndex(GameObject hitInfo)
        {
            for (var x = 0; x < TILE_COUNT_X; x++)
            for (var y = 0; y < TILE_COUNT_Y; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);

            return -Vector2Int.one;
        }
    }
}
