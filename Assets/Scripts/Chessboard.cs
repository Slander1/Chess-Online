using System;
using System.Collections.Generic;
using System.Linq;
using ChessPiaces;
using GameLogic;
using Net;
using Net.NetMassage;
using Unity.Networking.Transport;
using UnityEngine;
using static Net.NetUtility;

public class Chessboard : MonoBehaviour
{

    [SerializeField] private float deadScale = 0.8f;
    [SerializeField] private float deadSpacing = 10f;
    [SerializeField] private float dragOffset = 1.5f;
    [SerializeField] private ChessPiecesChoseSelector choseSelector;
    

    private List<ChessPiece> _deadWhite = new List<ChessPiece>();
    private List<ChessPiece> _deadBlack = new List<ChessPiece>();
    private List<Vector2Int> _availableMoves = new List<Vector2Int>();

    private ChessPiece[,] _chessPieces;
    private ChessPiece _currentlyDragging;
    
    private Camera _currentCamera;
    private Vector2Int _currentHover;
    
    private bool _isBlackTurn;
    private int _playerCount = -1;
    private int _currentTeam = -1;
    public static event Action<int> OnCheck;
    public static event Action<int> OnMate;
    private bool _localGame = false;
    private bool[] _playerRematch = new bool[2];
    private static readonly int InGameMenu = Animator.StringToHash("InGameMenu");

    private void Start()
    {
        _isBlackTurn = false;
        _currentCamera = Camera.main;
        Tiles.Instance.GenerateAllTiles(Tiles.Instance.TILE_COUNT_X, Tiles.Instance.TILE_COUNT_Y, transform);
        _chessPieces = new ChessPiece[Tiles.Instance.TILE_COUNT_X, Tiles.Instance.TILE_COUNT_Y];
        SpawnAndPosPiaces.Instance.SpawnAllPieces(_chessPieces);
        SpawnAndPosPiaces.Instance.PositionAllPieces(_chessPieces);

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
        UI.Instance.setLocaleGame += OnSetLocaleGame;
        UI.Instance.onPauseResumeButtonClick += InPauseButton;
        UI.Instance.onRestartButtonClick += OnRestartButtonClick;
        UI.Instance.onMenuButton += OnMenuButton;

    }

    private void OnRematchServer(NetMessage msg)
    {
        Server.Instance.Broadcast(msg);
    }
    private void OnRematchClient(NetMessage msg)
    {
        var rematch = msg as NetRematch;
        _playerRematch[rematch.teamId] = rematch.wantRematch == 1;

        if ((rematch.teamId != _currentTeam) && (rematch.wantRematch == 0))
            UI.Instance.textQuit.gameObject.SetActive(true);
        else if (rematch.teamId != _currentTeam)
            UI.Instance.textRematch.gameObject.SetActive(true);

        if (_playerRematch[0] && _playerRematch[1])
        {
            UI.Instance.textRematch.gameObject.SetActive(false);
            OnRestartButtonClick();
        }
    }

    private void OnRestartButtonClick()
    {
        _currentlyDragging = null;
        _availableMoves.Clear();
        UI.Instance.textRematch.gameObject.SetActive(false);
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
                if (_chessPieces[i, j] != null)
                    Destroy(_chessPieces[i, j].gameObject);
                _chessPieces[i, j] = null;
            }
        }

        SpawnAndPosPiaces.Instance.SpawnAllPieces(_chessPieces);
        SpawnAndPosPiaces.Instance.PositionAllPieces(_chessPieces);
        _isBlackTurn = false;
        UI.Instance.ChangeCamera(((_currentTeam == 0 && !_localGame) || _localGame) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);
        UI.Instance.pauseMenu.gameObject.SetActive(false);
        if (_localGame)
            _currentTeam = 0;
    }


    private void InPauseButton(bool pause)
    {
        if (pause)
            UI.Instance.ChangeCamera(0);
        else
            UI.Instance.ChangeCamera(((_currentTeam == 0 && !_localGame) || _localGame) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);
    }

    private void OnChosePieceServer(NetMessage msg)
    {
        var netChosePiece = msg as NetChosePiece;
        Server.Instance.Broadcast(netChosePiece);
    }

    private void OnChosePieceClient(NetMessage msg)
    {
        var netChosePiece = msg as NetChosePiece;
        Destroy(_chessPieces[netChosePiece.pos.x, netChosePiece.pos.y].gameObject);
        var piece = SpawnAndPosPiaces.Instance.SpawnSinglePiece(netChosePiece.type, netChosePiece.teamId);
        _chessPieces[netChosePiece.pos.x, netChosePiece.pos.y] = piece;
        SpawnAndPosPiaces.Instance.SetPiecePos(_chessPieces,netChosePiece.pos, true);
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
        UI.Instance.setLocaleGame -= OnSetLocaleGame;
        UI.Instance.onPauseResumeButtonClick -= InPauseButton;
        UI.Instance.onMenuButton -= OnMenuButton;
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


    private void OnStartGameClient(NetMessage msg, int connectionNumber)
    {
        UI.Instance.backGroundIMG.gameObject.SetActive(false);
        UI.Instance.ChangeCamera((_currentTeam == 0) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);
        
        UI.Instance.menuAnimator.SetTrigger(InGameMenu);
        UI.Instance.pauseButton.gameObject.SetActive(true);
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


    private static void OnMakeMoveServer(NetMessage msg)
    {
        var makeMove = msg as NetMakeMove;
        Server.Instance.Broadcast(makeMove);
    }

    private void OnWelcomeServer(NetMessage msg)
    {
        if (_localGame)
            _currentTeam = 0;
        if (++_playerCount == 1 || _localGame)
            Server.Instance.Broadcast(new NetStartGame());
        //var netWelcome = msg as NetWelcome;
        //netWelcome.AssignedTeam = ++_playerCount;
        //Server.Instance.SendToClient(networkConnection, netWelcome);
        //if (_playerCount == 1)
        //    Server.Instance.Broadcast(new NetStartGame());
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
        UI.Instance.OnLeaveFromGameMenu();
        UI.Instance.backGroundIMG.gameObject.SetActive(true);
        UI.Instance.pauseMenu.gameObject.SetActive(false);
        UI.Instance.pauseButton.gameObject.SetActive(false);
        
        Invoke("ShutdownRelay", 1.0f);

        _playerCount = -1;
        _currentTeam = -1;
    }

    private void ShutdownRelay()
    {
        Client.Instance.ShutDown();
        Server.Instance.ShutDown();
        UI.Instance.textQuit.gameObject.SetActive(false);
        UI.Instance.textRematch.gameObject.SetActive(false);
    }
    #endregion

    private void OnSetLocaleGame(bool value, bool isServer)
    {
        _currentTeam = isServer ? 1 : 0;
        _localGame = value;
    }

    private void Update()
    {
        var ray = _currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var info, 100, LayerMask.GetMask(Tiles.TILE, Tiles.HOVER, Tiles.HIGLIGHT)))
        {
            Vector2Int hitPosition = Tiles.Instance.LockupTileIndex(info.transform.gameObject);

            if (_currentHover == -Vector2Int.one)
            {
                _currentHover = hitPosition;
                Tiles.Instance.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer(Tiles.HOVER);
            }


            if (_currentHover != hitPosition)
            {
                Tiles.Instance.tiles[_currentHover.x, _currentHover.y].layer =
                    (_availableMoves.Contains(_currentHover))
                        ? LayerMask.NameToLayer(Tiles.HIGLIGHT)
                        : LayerMask.NameToLayer(Tiles.TILE);
                _currentHover = hitPosition;
                Tiles.Instance.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer(Tiles.HOVER);
            }

            if (Input.GetMouseButtonDown(0) && _chessPieces[hitPosition.x, hitPosition.y] != null &&
                Convert.ToBoolean(_chessPieces[hitPosition.x, hitPosition.y].team) == _isBlackTurn &&
                Convert.ToBoolean(_currentTeam) == _isBlackTurn)
            {
                _currentlyDragging = _chessPieces[hitPosition.x, hitPosition.y];
                _availableMoves = _currentlyDragging.GetAvailableMoves(_chessPieces);
                Tiles.Instance.HighlightTiles(_availableMoves);
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
                    _currentlyDragging.AnimateMove(Tiles.Instance.GetTileCenter(lastPosition));
                    _currentlyDragging = null;
                    Tiles.Instance.RemoveHighlightTiles(_availableMoves);
                }
            }
        }
        else
        {
            if (_currentHover != -Vector2Int.one)
            {
                Tiles.Instance.tiles[_currentHover.x, _currentHover.y].layer =
                    (_availableMoves.Contains(_currentHover))
                        ? LayerMask.NameToLayer(Tiles.HIGLIGHT)
                        : LayerMask.NameToLayer(Tiles.TILE);
                _currentHover = -Vector2Int.one;
            }

            if (_currentlyDragging && Input.GetMouseButtonUp(0))
            {
                _currentlyDragging.AnimateMove(Tiles.Instance.GetTileCenter(_currentlyDragging.currentPos));
                _currentlyDragging = null;
                Tiles.Instance.RemoveHighlightTiles(_availableMoves);
            }
        }


        if (!_currentlyDragging) return;
        var horizontalPlane = new Plane(Vector3.up, Vector3.up * Tiles.Instance.yOffset);
        if (horizontalPlane.Raycast(ray, out var distance))
        {
            _currentlyDragging.AnimateMove(ray.GetPoint(distance) + Vector3.up * dragOffset);
        }
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
            var startPositionShiftX = new[] { -0.7f, Tiles.Instance.TILE_COUNT_X - 0.3f };
            var startPositionShiftZ = new[] { 0, Tiles.Instance.TILE_COUNT_X - 1 };
            var deadTeam = isFirstTeam ? _deadWhite : _deadBlack;

            var boardSize = new Vector3(Tiles.Instance.tileSize * 0.5f, 0, Tiles.Instance.tileSize * 0.5f) - Tiles.Instance.bounds;
            var shift = Vector3.forward * (chessPiecesDirection * deadSpacing * deadTeam.Count);
            otherChessPieces.SetScale(Vector3.one * deadScale);

            var startPosition = new Vector3(
                startPositionShiftX[isFirstTeam ? 1 : 0] * Tiles.Instance.tileSize,
                Tiles.Instance.yOffset,
                startPositionShiftZ[isFirstTeam ? 0 : 1] * Tiles.Instance.tileSize);

            deadTeam.Add(otherChessPieces);

            otherChessPieces.AnimateMove(startPosition + boardSize + shift);
        }

        var lastPosition = currentlyDragging.currentPos;
        _chessPieces[pos.x, pos.y] = currentlyDragging;
        _chessPieces[lastPosition.x, lastPosition.y] = null;
        var thisTeam = _chessPieces[pos.x, pos.y].team;
        SpawnAndPosPiaces.Instance.SetPiecePos(_chessPieces,pos);
        if (_chessPieces[pos.x, pos.y] is Pawn pawn && ((thisTeam == 0 && pos.y == 7) || (thisTeam == 1 && pos.y == 0)) &&
            Convert.ToBoolean(_currentTeam) == _isBlackTurn)
        {
            UI.Instance.ChangeCamera(CameraAngle.menu);
            choseSelector.SpawnPiecesForChoose(SpawnAndPosPiaces.Instance.teamMaterials[thisTeam], type =>
            {
                UI.Instance.ChangeCamera((_currentTeam == 0 || _localGame) ? CameraAngle.whiteTeam : CameraAngle.blackTeam);
                var piece = SpawnAndPosPiaces.Instance.SpawnSinglePiece(type, _currentTeam);
                _chessPieces[pos.x, pos.y] = piece;
                SpawnAndPosPiaces.Instance.SetPiecePos(_chessPieces, pos, true);
                piece.currentPos = pos;
                var chosePiece = new NetChosePiece { type = type, pos = pos, teamId = _currentTeam };
                Client.Instance.SendToServer(chosePiece);
                ChangeTurn();
            });
            Destroy(pawn.gameObject);
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
        Tiles.Instance.RemoveHighlightTiles(_availableMoves);

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