using System;
using System.Collections.Generic;
using System.Linq;
using ChessPiaces;
using Net;
using Net.NetMassage;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static Net.NetUtility;

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
    [SerializeField] private ChessPiecesChoseSelector choseSelector; 

    [Header("Prefabs & Material")] [SerializeField]
    private ChessPiece[] figurePrefabs;

    [SerializeField] private Material[] teamMaterials;


    public static readonly int TILE_COUNT_X = 8;
    public static readonly int TILE_COUNT_Y = 8;

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
    private int _playerCount = -1;
    private int _currentTeam = -1;
    private Vector2Int _swapPawn = new Vector2Int(-1, -1);
    public static event Action<int> OnCheck;
    public static event Action<int> OnMate;
    private bool _localGame = false;
    private bool[] _playerRematch = new bool[2];
    private static readonly int InGameMenu = Animator.StringToHash("InGameMenu");

    private void Start()
    {
        _isBlackTurn = false;
        _currentCamera = Camera.main;
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        SpawnAllPieces();
        PositionAllPieces();

        RegisterEvents();
    }

    #region Reg

    private void RegisterEvents()
    {
        SWelcome += OnWelcomeServer;
        CWelcome += OnWelcomeClient;
        CStartgame += OnStartGameClient;
        SMakeMove += OnMakeMoveServer;
        CMakeMove += OnMakeMoveClient;
        CChosePieceOnChange += OnChosePieceClient;
        SChosePieceOnChange += OnChosePieceServer;
        SRematch += OnRematchServer;
        CRematch += OnRematchClient;
        Buttons.Instance.setLocaleGame += OnSetLocaleGame;
        Buttons.Instance.onPauseResumeButtonClick += InPauseButton;
        Buttons.Instance.onRestartButtonClick += OnRestartButtonClick;
        
    }
    
    private void OnRematchServer(NetMessage msg, NetworkConnection networkConnection)
    {
        Server.Instance.Broadcast(msg);
    }
    private void OnRematchClient(NetMessage msg)
    {
        var rematch = msg as NetRematch;
        _playerRematch[rematch.teamId] = rematch.wantRematch == 1;
        
        if (rematch.teamId != _currentTeam)
            Buttons.Instance.textRemach.gameObject.SetActive(true);

        if (_playerRematch[0] && _playerRematch[1])
            OnRestartButtonClick();
    }

    private void OnRestartButtonClick()
    {
        _currentlyDragging = null;
        _availableMoves.Clear();
        Buttons.Instance.textRemach.gameObject.SetActive(false);
        _playerRematch[0] = _playerRematch[1] = false; 
        foreach (var item in _deadWhite)
        {
            Destroy(item.gameObject);
        }
        foreach (var item in _deadBlack)
        {
            Destroy(item.gameObject);
        }
        _deadBlack.Clear();
        _deadWhite.Clear();
        _availableMoves.Clear();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (_chessPieces[i,j] != null)
                    Destroy(_chessPieces[i,j].gameObject);
                _chessPieces[i, j] = null;
            }
        }

        SpawnAllPieces();
        PositionAllPieces();
        _isBlackTurn = false;
        Buttons.Instance.ChangeCamera(((_currentTeam == 0 && !_localGame)|| _localGame) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);
        Buttons.Instance.pauseMenu.gameObject.SetActive(false);
    }

    private void RestartGame()
    {
       
    }
    private void InPauseButton(bool pause)
    {
        if (pause)
            Buttons.Instance.ChangeCamera(0);
        else
            Buttons.Instance.ChangeCamera(((_currentTeam == 0 && !_localGame)|| _localGame) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);
    }

    private void OnChosePieceServer(NetMessage msg, NetworkConnection networkConnection)
    {
        var netChosePiece = msg as NetChosePiece;
        Server.Instance.Broadcast(netChosePiece);
    }

    private void OnChosePieceClient(NetMessage msg)
    {
        var netChosePiece = msg as NetChosePiece;
        Destroy(_chessPieces[netChosePiece.pos.x, netChosePiece.pos.y].gameObject);
        var piece = SpawnSinglePiece(netChosePiece.type, netChosePiece.teamId);
        _chessPieces[netChosePiece.pos.x, netChosePiece.pos.y] = piece;
        SetPiecePos(netChosePiece.pos, true);
        piece.currentPos = netChosePiece.pos;
    }

    private void UnRegisterEvents()
    {
        SWelcome -= OnWelcomeServer;
        CWelcome -= OnWelcomeClient;
        CStartgame -= OnStartGameClient;
        SMakeMove -= OnMakeMoveServer;
        CMakeMove -= OnMakeMoveClient;
        CChosePieceOnChange += OnChosePieceClient;
        SChosePieceOnChange += OnChosePieceServer;
        SRematch -= OnRematchServer;
        CRematch -= OnRematchClient;
        Buttons.Instance.setLocaleGame -= OnSetLocaleGame;
        Buttons.Instance.onPauseResumeButtonClick -= InPauseButton;
    }

    private void OnMakeMoveClient(NetMessage msg)
    {
        var makeMove = msg as NetMakeMove;
        if (makeMove.teamId != _currentTeam)
        {
            ChessPiece target = _chessPieces[makeMove.originalMove.x, makeMove.originalMove.y];
            _availableMoves = target.GetMoves(_chessPieces);
            MoveTo(makeMove.originalMove, makeMove.distanationMove);
        }
    }


    private void OnStartGameClient(NetMessage msg)
    {
        Buttons.Instance.backGroundIMG.gameObject.SetActive(false);
        Buttons.Instance.ChangeCamera((_currentTeam == 0) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);
        Buttons.Instance.MenuAnimator.SetTrigger(InGameMenu);
        Buttons.Instance.pauseButton.gameObject.SetActive(true);
    }
    
    private void OnWelcomeClient(NetMessage msg)
    {
        var netWelcome = msg as NetWelcome;
        _currentTeam = netWelcome.AssignedTeam;
        if (_localGame && _currentTeam == 0)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }


    private void OnMakeMoveServer(NetMessage msg, NetworkConnection networkConnection)
    {
        var makeMove = msg as NetMakeMove;
        Server.Instance.Broadcast(makeMove);
    }

    private void OnWelcomeServer(NetMessage msg, NetworkConnection networkConnection)
    {
        var netWelcome = msg as NetWelcome;
        netWelcome.AssignedTeam = ++_playerCount;
        Server.Instance.SendToClient(networkConnection, netWelcome);
        if (_playerCount == 1)
            Server.Instance.Broadcast(new NetStartGame());
    }
    public void OnRematchButton()
    {
        if (_localGame)
        {
            var rematchwhite = new NetRematch();
            rematchwhite.teamId = 0;
            rematchwhite.wantRematch = 1;
            Client.Instance.SendToServer(rematchwhite);
            
            var rematchBlack = new NetRematch();
            rematchBlack.teamId = 1;
            rematchBlack.wantRematch = 1;
            Client.Instance.SendToServer(rematchBlack);
            
        }
        else
        {
            var rematch = new NetRematch();
            rematch.teamId = _currentTeam;
            rematch.wantRematch = 1;
            Client.Instance.SendToServer(rematch);
        }
    }

    public void OnMenuButton()
    {
        var rematch = new NetRematch();
        rematch.teamId = _currentTeam;
        rematch.wantRematch = 0;
        Client.Instance.SendToServer(rematch);
        OnRestartButtonClick();
        Buttons.Instance.OnLeaveFromGameMenu();
        Invoke("ShutdownRelay", 1.0f);
        _playerCount = -1;
        _currentTeam = -1;
    }

    private void ShutdownRelay()
    {
        Client.Instance.ShutDown();
        Server.Instance.ShutDown();
    }
    #endregion

    private void OnSetLocaleGame(bool value)
    {

        _localGame = value;
    }

    private void Update()
    {
        var ray = _currentCamera.ScreenPointToRay(Input.mousePosition);
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
                Convert.ToBoolean(_chessPieces[hitPosition.x, hitPosition.y].team) == _isBlackTurn &&
                Convert.ToBoolean(_currentTeam) == _isBlackTurn)
            {
                _currentlyDragging = _chessPieces[hitPosition.x, hitPosition.y];
                _availableMoves = _currentlyDragging.GetAvailableMoves(_chessPieces);
                HighlightTiles();
            }

            if (_currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                var lastPosition = _currentlyDragging.currentPos;

                if (_availableMoves.Contains(hitPosition))
                {
                    MoveTo(lastPosition, hitPosition);

                    NetMakeMove makeMove = new NetMakeMove();
                    makeMove.originalMove = lastPosition;
                    makeMove.distanationMove = hitPosition;
                    makeMove.teamId = _currentTeam;
                    Client.Instance.SendToServer(makeMove);
                }
                else
                {
                    _currentlyDragging.AnimateMove(GetTileCenter(lastPosition));
                    _currentlyDragging = null;
                    RemoveHighlightTiles();
                }
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
                _currentlyDragging.AnimateMove(GetTileCenter(_currentlyDragging.currentPos));
                _currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }


        if (!_currentlyDragging) return;
        var horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
        if (horizontalPlane.Raycast(ray, out var distance))
        {
            _currentlyDragging.AnimateMove(ray.GetPoint(distance) + Vector3.up * dragOffset);
        }
    }

    


    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        _bounds = new Vector3((tileCountX * 0.5f) * tileSize, 0, (tileCountX * 0.5f) * tileSize) + boardCenter;
        _tiles = new GameObject[tileCountX, tileCountY];
        for (var x = 0; x < tileCountX; x++)
        for (var y = 0; y < tileCountY; y++)
            _tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }
    

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;
        var mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;
        var vertices = new Vector3[4]
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

    private void SpawnAllPieces()
    {
        _chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
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
            _chessPieces[i, 0] = SpawnSinglePiece(typesOfPiecesWhite[i], whiteTeam);
            _chessPieces[i, 7] = SpawnSinglePiece(typesOfPiecesBlack[i], blackTeam);
        }

        for (var i = 0; i < 8; i++)
        {
            _chessPieces[i, 1] = SpawnSinglePiece(ChessPiece.Type.Pawn, whiteTeam);
            _chessPieces[i, 6] = SpawnSinglePiece(ChessPiece.Type.Pawn, blackTeam);
        }
       
    }

    private ChessPiece SpawnSinglePiece(ChessPiece.Type type, int team)
    {
        var cp = Instantiate(figurePrefabs[(int) type - 1], transform);
        cp.type = type;
        cp.team = team;
        cp.meshRenderer.material = teamMaterials[team];
        return cp;
    }


    private void PositionAllPieces()
    {
        for (var x = 0; x < TILE_COUNT_X; x++)
        for (var y = 0; y < TILE_COUNT_Y; y++)
            if (_chessPieces[x, y] != null)
                SetPiecePos(new Vector2Int(x, y), true);
    }

    private void SetPiecePos(Vector2Int pos, bool force = false)
    {
        _chessPieces[pos.x, pos.y].currentPos = pos;
        _chessPieces[pos.x, pos.y].AnimateMove(GetTileCenter(pos), force);
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

    private Vector2Int LockupTileIndex(GameObject hitInfo)
    {
        for (var x = 0; x < TILE_COUNT_X; x++)
        for (var y = 0; y < TILE_COUNT_Y; y++)
            if (_tiles[x, y] == hitInfo)
                return new Vector2Int(x, y);

        return -Vector2Int.one;
    }

    

    private void MoveTo(Vector2Int originalCoord, Vector2Int pos)
    {
        ChessPiece currentlyDragging = _chessPieces[originalCoord.x, originalCoord.y];
        if (_chessPieces[pos.x, pos.y] != null)
        {
            ChessPiece otherChessPieces = _chessPieces[pos.x, pos.y];
            if (otherChessPieces.team == currentlyDragging.team)
                return;
            var isFirstTeam = otherChessPieces.team == 0;
            var chessPiecesDirection = isFirstTeam ? 1 : -1;
            var startPositionShiftX = new[] {-0.7f, TILE_COUNT_X - 0.3f};
            var startPositionShiftZ = new[] {0, TILE_COUNT_X - 1};
            var deadTeam = isFirstTeam ? _deadWhite : _deadBlack;

            var boardSize = new Vector3(tileSize * 0.5f, 0, tileSize * 0.5f) - _bounds;
            var shift = Vector3.forward * (chessPiecesDirection * deadSpacing * deadTeam.Count);
            otherChessPieces.SetScale(Vector3.one * deadScale);

            var startPosition = new Vector3(
                startPositionShiftX[isFirstTeam ? 1 : 0] * tileSize,
                yOffset,
                startPositionShiftZ[isFirstTeam ? 0 : 1] * tileSize);

            deadTeam.Add(otherChessPieces);

            otherChessPieces.AnimateMove(startPosition + boardSize + shift);
        }

        var lastPosition = currentlyDragging.currentPos;
        _chessPieces[pos.x, pos.y] = currentlyDragging;
        _chessPieces[lastPosition.x, lastPosition.y] = null;
        var thisTeam = _chessPieces[pos.x, pos.y].team;
        SetPiecePos(pos);
        if (_chessPieces[pos.x, pos.y] is Pawn pawn && ((thisTeam == 0 && pos.y == 7) || (thisTeam == 1 && pos.y == 0)) &&
            Convert.ToBoolean(_currentTeam) == _isBlackTurn)
        {
            Buttons.Instance.ChangeCamera(CameraAngle.menu);
            choseSelector.SpawnPiecesForChoose(teamMaterials[thisTeam], type =>
            {
                Buttons.Instance.ChangeCamera((_currentTeam == 0 || _localGame) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);
                var piece = SpawnSinglePiece(type, _currentTeam);
                _chessPieces[pos.x, pos.y] = piece;
                SetPiecePos(pos, true);
                piece.currentPos = pos;
                var chosePiece = new NetChosePiece {type = type, pos = pos, teamId = _currentTeam};
                Client.Instance.SendToServer(chosePiece);
                ChangeTurn();
            });
            Destroy(pawn.gameObject);
            _swapPawn = pawn.currentPos;
            return;
        }
        ChangeTurn();
    }

    private void ChangeTurn()
    {
        _isBlackTurn = !_isBlackTurn;
        if (_localGame)
            _currentTeam = (_currentTeam == 0) ? 1 : 0;
        var turn = _isBlackTurn ? 0 : 1;

        if (_currentlyDragging)
            _currentlyDragging = null;
        RemoveHighlightTiles();

        if (!IsMate(_chessPieces, turn))
        {
            OnMate?.Invoke(turn);
            return;
        }

        if (IsKingUnderAttack(_chessPieces, turn == 0 ? 1 : 0))
        {
            OnCheck?.Invoke(turn);
        }
    }
    

    public static bool IsKingUnderAttack(ChessPiece[,] board, int team)
    {
        King ourKing = null;

        foreach (var chessPiece in board)
            if (chessPiece != null && chessPiece.team == team && chessPiece is King thereKing)
            {
                ourKing = thereKing;
                break;
            }

        foreach (var chessPiece in board)
        {
            if (chessPiece == null || chessPiece.team == team)
                continue;

            if (chessPiece.GetMoves(board).Contains(ourKing.currentPos))
                return true;
        }

        return false;
    }

    private static bool IsMate(ChessPiece[,] board, int team)
    {
        return (board.Cast<ChessPiece>().Where(chessPiece => chessPiece != null && chessPiece.team != team)
            .Any(chessPiece => chessPiece.GetAvailableMoves(board).Count > 0));
    }
}