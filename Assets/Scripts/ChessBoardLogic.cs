using System;
using System.Collections.Generic;
using System.Linq;
using ChessPieces;
using ChessPieces.ChessPiecesChose;
using GameLogic;
using Net;
using Net.NetMassage;
using ServiceLocator;
using UI;
using UnityEngine;
using Utils.ServiceLocator;

public class ChessBoardLogic : IService
{
    public void Init(float deadScale, float deadSpacing, float dragOffset, ChessPiecesChoseSelector choseSelector, Transform transform)
    {
        _tileCountX = Tiles.TILE_COUNT_X;
        _tileCountY = Tiles.TILE_COUNT_Y;
        _deadScale = deadScale;
        _deadSpacing = deadSpacing;
        _dragOffset = dragOffset;
        this.choseSelector = choseSelector;
        _isBlackTurn = false;
        ServiceL.Get<Tiles>().GenerateAllTiles(_tileCountX, _tileCountY, transform);
        _chessPieces = new ChessPiece[_tileCountX, _tileCountY];
        ServiceL.Get<SpawnAndPosPieces>().SpawnAllPieces(_chessPieces);
        ServiceL.Get<SpawnAndPosPieces>().PositionAllPieces(_chessPieces);
    }

    private List<ChessPiece> _deadWhite = new List<ChessPiece>();
    private List<ChessPiece> _deadBlack = new List<ChessPiece>();
    private List<Vector2Int> _availableMoves = new List<Vector2Int>();

    private ChessPiece[,] _chessPieces;
    private ChessPiece _currentlyDragging;
    
    
    private bool _isBlackTurn;
    private int _playerCount = -1;
    private int _currentTeam = -1;
        
    private bool _localGame = false;
    private bool[] _playerRematch = new bool[2];
    private static readonly int InGameMenu = Animator.StringToHash("InGameMenu");
    private float _deadScale;
    private float _deadSpacing;
    private float _dragOffset;
    private ChessPiecesChoseSelector choseSelector;
    public event Action<int> OnCheck;
    public event Action<int> OnMate;
    private Vector2Int _currentHover;
    private int _tileCountX;
    private int _tileCountY;
    
    
    public event Action<Vector2Int, string> swapTileLayer;
    public Func<GameObject, Vector2Int> lockUpTile;
    public event Action<List<Vector2Int>> highlightTiles; 
    public Func <Vector2Int, Vector3> getTileCenter;
    public event Action<List<Vector2Int>> removeHighlightTiles;
   
    

    public void OnRematchServer(NetMessage msg)
    {
        Server.Instance.Broadcast(msg);
    }

    public void OnRematchClient(NetMessage msg)
    {
        var rematch = msg as NetRematch;
        _playerRematch[rematch.teamId] = rematch.wantRematch == 1;

        if ((rematch.teamId != _currentTeam) && (rematch.wantRematch == 0))
            ServiceL.Get<Buttons>().textQuit.gameObject.SetActive(true);
        else if (rematch.teamId != _currentTeam)
            ServiceL.Get<Buttons>().textRematch.gameObject.SetActive(true);

        if (_playerRematch[0] && _playerRematch[1])
        {
            ServiceL.Get<Buttons>().textRematch.gameObject.SetActive(false);
            OnRestartButtonClick();
        }
    }

    public void OnRestartButtonClick()
    {
        _currentlyDragging = null;
        _availableMoves.Clear();
        ServiceL.Get<Buttons>().textRematch.gameObject.SetActive(false);
        _playerRematch[0] = _playerRematch[1] = false;
        foreach (var item in _deadWhite)
        {
            ChessBoard.DestoyPiece(item.gameObject);
        }
        foreach (var item in _deadBlack)
        {
            ChessBoard.DestoyPiece(item.gameObject);
        }
        _deadBlack.Clear();
        _deadWhite.Clear();
        _availableMoves.Clear();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (_chessPieces[i, j] != null)
                    ChessBoard.DestoyPiece(_chessPieces[i, j].gameObject);
                _chessPieces[i, j] = null;
            }
        }

        ServiceL.Get<SpawnAndPosPieces>().SpawnAllPieces(_chessPieces);
        ServiceL.Get<SpawnAndPosPieces>().PositionAllPieces(_chessPieces);
        _isBlackTurn = false;
        ServiceL.Get<Cameras>().ChangeCamera(((_currentTeam == 0 && !_localGame) || _localGame) ? Cameras.CameraAngle.whiteTeam : Cameras.CameraAngle.blackTeam);
        ServiceL.Get<Buttons>().pauseMenu.gameObject.SetActive(false);
        if (_localGame)
            _currentTeam = 0;
    }


    public void InPauseButton(bool pause)
    {
        if (pause)
            ServiceL.Get<Cameras>().ChangeCamera(0);
        else
            ServiceL.Get<Cameras>().ChangeCamera(((_currentTeam == 0 && !_localGame) || _localGame) ? Cameras.CameraAngle.whiteTeam : Cameras.CameraAngle.blackTeam);
    }

    public void OnChosePieceServer(NetMessage msg)
    {
        var netChosePiece = msg as NetChosePiece;
        Server.Instance.Broadcast(netChosePiece);
    }

    public void OnChosePieceClient(NetMessage msg)
    {
        var netChosePiece = msg as NetChosePiece;
        ChessBoard.DestoyPiece(_chessPieces[netChosePiece.pos.x, netChosePiece.pos.y].gameObject);
        var piece = ServiceL.Get<SpawnAndPosPieces>().SpawnSinglePiece(netChosePiece.type, netChosePiece.teamId);
        _chessPieces[netChosePiece.pos.x, netChosePiece.pos.y] = piece;
        ServiceL.Get<SpawnAndPosPieces>().SetPiecePos(_chessPieces,netChosePiece.pos, true);
        piece.currentPos = netChosePiece.pos;
    }

    

    public void OnMakeMoveClient(NetMessage msg)
    {
        var makeMove = msg as NetMakeMove;
        if (makeMove.teamId != _currentTeam)
        {
            ChessPiece target = _chessPieces[makeMove.originalMove.x, makeMove.originalMove.y];
            _availableMoves = target.GetMoves(_chessPieces);
            MoveTo(makeMove.originalMove, makeMove.distanationMove);
        }
    }


    public void OnStartGameClient(NetMessage msg, int connectionNumber)
    {
        ServiceL.Get<Buttons>().backGroundIMG.gameObject.SetActive(false);
        ServiceL.Get<Cameras>().ChangeCamera((_currentTeam == 0) ? Cameras.CameraAngle.whiteTeam : Cameras.CameraAngle.blackTeam);
        
        ServiceL.Get<Buttons>().menuAnimator.SetTrigger(InGameMenu);
        ServiceL.Get<Buttons>().pauseButton.gameObject.SetActive(true);
    }

    public void OnWelcomeClient(NetMessage msg)
    {
        var netWelcome = msg as NetWelcome;
        _currentTeam = netWelcome.AssignedTeam;
        if (_localGame && _currentTeam == 0)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }


    public void OnMakeMoveServer(NetMessage msg)
    {
        var makeMove = msg as NetMakeMove;
        Server.Instance.Broadcast(makeMove);
    }

    public void OnWelcomeServer(NetMessage msg)
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
            ServiceL.Get<Client>().SendToServer(rematchwhite);

            var rematchBlack = new NetRematch();
            rematchBlack.teamId = 1;
            rematchBlack.wantRematch = 1;
            ServiceL.Get<Client>().SendToServer(rematchBlack);

        }
        else
        {
            var rematch = new NetRematch();
            rematch.teamId = _currentTeam;
            rematch.wantRematch = 1;
            ServiceL.Get<Client>().SendToServer(rematch);
        }
    }

    public void OnMenuButton()
    {
        var rematch = new NetRematch();
        rematch.teamId = _currentTeam;
        rematch.wantRematch = 0;
        ServiceL.Get<Client>().SendToServer(rematch);
        OnRestartButtonClick();
        ServiceL.Get<Buttons>().OnLeaveFromGameMenu();
        ServiceL.Get<Buttons>().backGroundIMG.gameObject.SetActive(true);
        ServiceL.Get<Buttons>().pauseMenu.gameObject.SetActive(false);
        ServiceL.Get<Buttons>().pauseButton.gameObject.SetActive(false);
        
        //Invoke("ShutdownRelay", 1.0f);

        _playerCount = -1;
        _currentTeam = -1;
    }

    private void ShutdownRelay()
    {
        ServiceL.Get<Client>().ShutDown();
        Server.Instance.ShutDown();
        ServiceL.Get<Buttons>().textQuit.gameObject.SetActive(false);
        ServiceL.Get<Buttons>().textRematch.gameObject.SetActive(false);
    }

    public void OnSetLocaleGame(bool value, bool isServer)
    {
        _currentTeam = isServer ? 1 : 0;
        _localGame = value;
    }

    public void OnRaycastLayer(Ray ray, RaycastHit info)
    {
        var hitPosition = (Vector2Int) lockUpTile?.Invoke(info.transform.gameObject);

            if (_currentHover == -Vector2Int.one)
            {
                _currentHover = hitPosition;
                swapTileLayer?.Invoke(hitPosition, Tiles.HOVER);
            }

            if (_currentHover != hitPosition)
            {
                swapTileLayer?.Invoke(_currentHover,
                    _availableMoves.Contains(_currentHover) ? Tiles.HIGLIGHT : Tiles.TILE);
                _currentHover = hitPosition;
                swapTileLayer?.Invoke(hitPosition, Tiles.HOVER);
            }

            if (Input.GetMouseButtonDown(0) && _chessPieces[hitPosition.x, hitPosition.y] != null &&
                Convert.ToBoolean(_chessPieces[hitPosition.x, hitPosition.y].team) == _isBlackTurn &&
                Convert.ToBoolean(_currentTeam) == _isBlackTurn)
            {
                _currentlyDragging = _chessPieces[hitPosition.x, hitPosition.y];
                _availableMoves = _currentlyDragging.GetAvailableMoves(_chessPieces);
                highlightTiles?.Invoke(_availableMoves);
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
                    ServiceL.Get<Client>().SendToServer(makeMove);
                }
                else
                {
                    _currentlyDragging.AnimateMove((Vector3) getTileCenter?.Invoke(lastPosition));
                    _currentlyDragging = null;
                    removeHighlightTiles?.Invoke(_availableMoves);
                    //Tiles.Instance.RemoveHighlightTiles(_availableMoves);
                }
            }

            HorizontalPlane(ray);

    }

    private void HorizontalPlane(Ray ray)
    {
        if (!_currentlyDragging) return;
        var horizontalPlane = new Plane(Vector3.up, Vector3.up * Tiles.YOffset);
        if (horizontalPlane.Raycast(ray, out var distance))
        {
            _currentlyDragging.AnimateMove(ray.GetPoint(distance) + Vector3.up * _dragOffset);
        }
    }
    public void onRaycastWithoutLayer(Ray ray, RaycastHit info)
    {
        if (_currentHover != -Vector2Int.one)
        {
            swapTileLayer?.Invoke(_currentHover,
                _availableMoves.Contains(_currentHover) ? Tiles.HIGLIGHT : Tiles.TILE);
            _currentHover = -Vector2Int.one;
        }

        if (_currentlyDragging && Input.GetMouseButtonUp(0))
        {
            _currentlyDragging.AnimateMove((Vector3) getTileCenter?.Invoke(_currentlyDragging.currentPos));
            _currentlyDragging = null;
            removeHighlightTiles?.Invoke(_availableMoves);
        }
        HorizontalPlane(ray);
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
            var startPositionShiftX = new[] { -0.7f, _tileCountX - 0.3f };
            var startPositionShiftZ = new[] { 0, _tileCountX - 1 };
            var deadTeam = isFirstTeam ? _deadWhite : _deadBlack;

            var boardSize = new Vector3(ServiceL.Get<Tiles>().tileSize * 0.5f, 0, ServiceL.Get<Tiles>().tileSize * 0.5f) - ServiceL.Get<Tiles>().bounds;
            var shift = Vector3.forward * (chessPiecesDirection * _deadSpacing * deadTeam.Count);
            otherChessPieces.SetScale(Vector3.one * _deadScale);

            var startPosition = new Vector3(
                startPositionShiftX[isFirstTeam ? 1 : 0] * ServiceL.Get<Tiles>().tileSize,
                Tiles.YOffset,
                startPositionShiftZ[isFirstTeam ? 0 : 1] * ServiceL.Get<Tiles>().tileSize);

            deadTeam.Add(otherChessPieces);

            otherChessPieces.AnimateMove(startPosition + boardSize + shift);
        }

        var lastPosition = currentlyDragging.currentPos;
        _chessPieces[pos.x, pos.y] = currentlyDragging;
        _chessPieces[lastPosition.x, lastPosition.y] = null;
        var thisTeam = _chessPieces[pos.x, pos.y].team;
        ServiceL.Get<SpawnAndPosPieces>().SetPiecePos(_chessPieces,pos);
        if (_chessPieces[pos.x, pos.y] is Pawn pawn && ((thisTeam == 0 && pos.y == 7) || (thisTeam == 1 && pos.y == 0)) &&
            Convert.ToBoolean(_currentTeam) == _isBlackTurn)
        {
            ServiceL.Get<Cameras>().ChangeCamera(Cameras.CameraAngle.menu);
            choseSelector.SpawnPiecesForChoose(ServiceL.Get<SpawnAndPosPieces>().teamMaterials[thisTeam], type =>
            {
                ServiceL.Get<Cameras>().ChangeCamera((_currentTeam == 0 || _localGame) ? Cameras.CameraAngle.whiteTeam : Cameras.CameraAngle.blackTeam);
                var piece = ServiceL.Get<SpawnAndPosPieces>().SpawnSinglePiece(type, _currentTeam);
                _chessPieces[pos.x, pos.y] = piece;
                ServiceL.Get<SpawnAndPosPieces>().SetPiecePos(_chessPieces, pos, true);
                piece.currentPos = pos;
                var chosePiece = new NetChosePiece { type = type, pos = pos, teamId = _currentTeam };
                ServiceL.Get<Client>().SendToServer(chosePiece);
                ChangeTurn();
            });
            ChessBoard.DestoyPiece(pawn.gameObject);
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
        ServiceL.Get<Tiles>().RemoveHighlightTiles(_availableMoves);

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
    
    public bool IsKingUnderAttack(ChessPiece[,] board, int team)
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