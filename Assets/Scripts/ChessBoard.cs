using System.Threading.Tasks;
using ChessPieces.ChessPiecesChose;
using UI;
using UnityEngine;
using Utils.ServiceLocator;
using static Net.NetUtility;


public class ChessBoard : ServiceMonoBehaviourGeneric<ChessBoardLogic>
{

    [SerializeField] private float deadScale = 0.8f;
    [SerializeField] private float deadSpacing = 10f;
    [SerializeField] private float dragOffset = 1.5f;
    [SerializeField] private ChessPiecesChoseSelector choseSelector;



    private void Start()
    {
        data.Init(deadScale, deadSpacing, dragOffset, choseSelector, transform);
    }

    private async void OnEnable()
    {
        await Task.Delay(1000);
        SWelcome += data.OnWelcomeServer;
        CWelcome += data.OnWelcomeClient;
        CStartgame += data.OnStartGameClient;
        SMakeMove += data.OnMakeMoveServer;
        CMakeMove += data.OnMakeMoveClient;
        CChosePieceOnChange += data.OnChosePieceClient;
        SChosePieceOnChange += data.OnChosePieceServer;
        SRematch += data.OnRematchServer;
        CRematch += data.OnRematchClient;
        ServiceL.Get<Buttons>().setLocaleGame += data.OnSetLocaleGame;
        ServiceL.Get<Buttons>().onPauseResumeButtonClick += data.InPauseButton;
        ServiceL.Get<Buttons>().onRestartButtonClick += data.OnRestartButtonClick;
        ServiceL.Get<Buttons>().onMenuButton += data.OnMenuButton;
        ServiceL.Get<RayCaster>().onRaycastLayer += data.OnRaycastLayer;
        ServiceL.Get<RayCaster>().onRaycastWithoutLayer += data.onRaycastWithoutLayer;
    }



    private void OnDisable()
    {
        SWelcome -= data.OnWelcomeServer;
        CWelcome -= data.OnWelcomeClient;
        CStartgame -= data.OnStartGameClient;
        SMakeMove -= data.OnMakeMoveServer;
        CMakeMove -= data.OnMakeMoveClient;
        CChosePieceOnChange += data.OnChosePieceClient;
        SChosePieceOnChange += data.OnChosePieceServer;
        SRematch -= data.OnRematchServer;
        CRematch -= data.OnRematchClient;
        ServiceL.Get<Buttons>().setLocaleGame -= data.OnSetLocaleGame;
        ServiceL.Get<Buttons>().onPauseResumeButtonClick -= data.InPauseButton;
        ServiceL.Get<Buttons>().onRestartButtonClick -= data.OnRestartButtonClick;
        ServiceL.Get<Buttons>().onMenuButton -= data.OnMenuButton;
        ServiceL.Get<RayCaster>().onRaycastLayer -= data.OnRaycastLayer;

    }

    public static void DestoyPiece(GameObject piece)
    {
        Destroy(piece.gameObject);
    }
}