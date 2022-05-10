using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class Chessboard : MonoBehaviour
{
    [FormerlySerializedAs("_tileMaterial")] [Header("Art stuff")] [SerializeField]
    private Material tileMaterial;

    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deadScale = 0.8f;
    [SerializeField] private float deadSpacing = 10f;
    [SerializeField] private float dragOffset = 1.5f;

    [Header("Prefabs & Material")] [SerializeField]
    private ChessPiece[] figurePrefabs;

    [SerializeField] private Material[] teammaterials;

    
    public static readonly int TILE_COUNT_X = 8;
    public static readonly  int TILE_COUNT_Y = 8;
    
    private const string HOVER = "Hover";
    private const string TILE = "Tile";
    private const string HIGLIGHT = "Hightlight";

    private List<ChessPiece> _deadWhite = new List<ChessPiece>();
    private List<ChessPiece> _deadBlack = new List<ChessPiece>();
    private List<Vector2Int> _availableMoves = new List<Vector2Int>();

    private ChessPiece[,] _chessPieces;
    private ChessPiece _currentlyDragging;
    private GameObject[,] _tiles;
    private Camera _currentCamera;
    private Vector2Int _currentHover;
    private Vector3 _bounds;
    private bool _isBlackTurn;


    private void Awake()
    {
        _isBlackTurn = false;
        _currentCamera = Camera.main;
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        SpawnAllPiaces();
        PositionAllPiaces();
    }

    private void Update()
    {
        Ray ray = _currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var info, 100, LayerMask.GetMask(TILE, HOVER, HIGLIGHT)))
        {
            Vector2Int hitPosition = LockupTileIndex(info.transform.gameObject);

            if (_currentHover == -Vector2Int.one)
            {
                _currentHover = hitPosition;
                _tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer(HOVER);
            }


            if (_currentHover != hitPosition)
            {
                _tiles[_currentHover.x, _currentHover.y].layer =
                    (_availableMoves.Contains(_currentHover))
                        ? LayerMask.NameToLayer(HIGLIGHT)
                        : LayerMask.NameToLayer(TILE);
                _currentHover = hitPosition;
                _tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer(HOVER);
            }

            if (Input.GetMouseButtonDown(0) && _chessPieces[hitPosition.x, hitPosition.y] != null && 
                Convert.ToBoolean(_chessPieces[hitPosition.x, hitPosition.y].team) == _isBlackTurn)
            {
                _currentlyDragging = _chessPieces[hitPosition.x, hitPosition.y];
                _availableMoves = _currentlyDragging.GetAvailableMoves(_chessPieces);
                HighlightTiles();
            }

            if (_currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                var lastPosition = _currentlyDragging.currentPos;
                var validMove = MoveTo(_currentlyDragging, hitPosition);
                if (!validMove)
                    _currentlyDragging.SetPosition(GetTileCenter(lastPosition));

                _currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }
        else
        {
            if (_currentHover != -Vector2Int.one)
            {
                _tiles[_currentHover.x, _currentHover.y].layer =
                    (_availableMoves.Contains(_currentHover))
                        ? LayerMask.NameToLayer(HIGLIGHT)
                        : LayerMask.NameToLayer(TILE);
                _currentHover = -Vector2Int.one;
            }

            if (_currentlyDragging && Input.GetMouseButtonUp(0))
            {
                _currentlyDragging.SetPosition(GetTileCenter(_currentlyDragging.currentPos));
                _currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }

        if (_currentlyDragging)
        {
            var horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            if (horizontalPlane.Raycast(ray, out var distance))
            {
                _currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
            }
        }
    }

    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        _bounds = new Vector3((tileCountX * 0.5f) * tileSize, 0, (tileCountX * 0.5f) * tileSize) + boardCenter;

        _tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        for (int y = 0; y < tileCountY; y++)
            _tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(x * tileSize, yOffset, y * tileSize) - _bounds,
            new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - _bounds,
            new Vector3((x + 1) * tileSize, yOffset, (y) * tileSize) - _bounds,
            new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - _bounds
        };

        int[] tris = {0, 1, 2, 1, 3, 2};

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer(TILE);
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    private void SpawnAllPiaces()
    {
        _chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
        int whiteTeam = 0, blackTeam = 1;
        _chessPieces[0, 0] = SpawnSinglePeace(ChessPieceType.Rook, whiteTeam);
        _chessPieces[1, 0] = SpawnSinglePeace(ChessPieceType.Knight, whiteTeam);
        _chessPieces[2, 0] = SpawnSinglePeace(ChessPieceType.Bishop, whiteTeam);
        _chessPieces[3, 0] = SpawnSinglePeace(ChessPieceType.Queen, whiteTeam);
        _chessPieces[4, 0] = SpawnSinglePeace(ChessPieceType.King, whiteTeam);       
        _chessPieces[5, 0] = SpawnSinglePeace(ChessPieceType.Bishop, whiteTeam);
        _chessPieces[6, 0] = SpawnSinglePeace(ChessPieceType.Knight, whiteTeam);
        _chessPieces[7, 0] = SpawnSinglePeace(ChessPieceType.Rook, whiteTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            _chessPieces[i, 1] = SpawnSinglePeace(ChessPieceType.Pawn, whiteTeam);
        _chessPieces[0, 7] = SpawnSinglePeace(ChessPieceType.Rook, blackTeam);
        _chessPieces[1, 7] = SpawnSinglePeace(ChessPieceType.Knight, blackTeam);
        _chessPieces[2, 7] = SpawnSinglePeace(ChessPieceType.Bishop, blackTeam);
        _chessPieces[4, 7] = SpawnSinglePeace(ChessPieceType.King, blackTeam);
        _chessPieces[3, 7] = SpawnSinglePeace(ChessPieceType.Queen, blackTeam);
        _chessPieces[5, 7] = SpawnSinglePeace(ChessPieceType.Bishop, blackTeam);
        _chessPieces[6, 7] = SpawnSinglePeace(ChessPieceType.Knight, blackTeam);
        _chessPieces[7, 7] = SpawnSinglePeace(ChessPieceType.Rook, blackTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
            _chessPieces[i, 6] = SpawnSinglePeace(ChessPieceType.Pawn, blackTeam);
    }

    private ChessPiece SpawnSinglePeace(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(figurePrefabs[(int) type - 1], transform).GetComponent<ChessPiece>();
        cp.type = type;
        cp.team = team;
        cp.meshRenderer.material = teammaterials[team];
        return cp;
    }


    private void PositionAllPiaces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        for (int y = 0; y < TILE_COUNT_Y; y++)
            if (_chessPieces[x, y] != null)
                PositionSinglePiaces(new Vector2Int(x, y), true);
    }

    private void PositionSinglePiaces(Vector2Int pos, bool force = false)
    {
        _chessPieces[pos.x, pos.y].currentPos = pos;
        _chessPieces[pos.x, pos.y].SetPosition(GetTileCenter(pos), force);
    }

    private Vector3 GetTileCenter(Vector2Int pos)
    {
        return new Vector3(pos.x * tileSize, yOffset + 0.2f, pos.y * tileSize) - _bounds +
               new Vector3(tileSize / 2, 0, tileSize / 2);
    }


    private void HighlightTiles()
    {
        foreach (var movePos in _availableMoves)
        {
            _tiles[movePos.x, movePos.y].layer = LayerMask.NameToLayer(HIGLIGHT);
        }
    }

    private void RemoveHighlightTiles()
    {
        foreach (var movePos in _availableMoves)
            _tiles[movePos.x, movePos.y].layer = LayerMask.NameToLayer(TILE);

        _availableMoves.Clear();
    }

    private Vector2Int LockupTileIndex(GameObject hitinfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        for (int y = 0; y < TILE_COUNT_Y; y++)
            if (_tiles[x, y] == hitinfo)
                return new Vector2Int(x, y);

        return -Vector2Int.one;
    }

    private bool MoveTo(ChessPiece currentlyDragging, in Vector2Int pos)
    {
        if (!_availableMoves.Contains(pos))
            return false;
        if (_chessPieces[pos.x, pos.y] != null)
        {
            ChessPiece otherChessPiaces = _chessPieces[pos.x, pos.y];
            if (otherChessPiaces.team == currentlyDragging.team)
            {
                return false;
            }

            var isFirstTeam = otherChessPiaces.team == 0;
            var chessPiecesDirection = isFirstTeam ? 1 : -1;
            var startPositionShiftX = new[]{ -0.7f, TILE_COUNT_X - 0.3f};
            var startPositionShiftZ = new[]{ 0, TILE_COUNT_X -1};
            var deadTeam = isFirstTeam ? _deadWhite : _deadBlack;

            var boardSize = new Vector3(tileSize * 0.5f, 0, tileSize * 0.5f) - _bounds;
            var shift = Vector3.forward * (chessPiecesDirection * deadSpacing * deadTeam.Count);
            otherChessPiaces.SetScale(Vector3.one * deadScale);
            
            var startPosition = new Vector3(
                startPositionShiftX[isFirstTeam ? 1 : 0] * tileSize,
                yOffset,
                startPositionShiftZ[isFirstTeam ? 0 : 1] * tileSize);
            
            deadTeam.Add(otherChessPiaces);
            
            otherChessPiaces.SetPosition(startPosition + boardSize + shift);
        }

        var lastPosition = _currentlyDragging.currentPos;
        _chessPieces[pos.x, pos.y] = currentlyDragging;
        _chessPieces[lastPosition.x, lastPosition.y] = null;

        PositionSinglePiaces(pos);

        _isBlackTurn = !_isBlackTurn;

        return true;
    }
}