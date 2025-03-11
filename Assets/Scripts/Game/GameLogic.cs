
using System;
using UnityEngine;

public abstract class BasePalyerSate
{
    public abstract void OnEnter(GameLogic gameLogic);
    public abstract void OnExit(GameLogic gameLogic);
    public abstract void HandleMove(GameLogic gameLogic, int row, int col);
    protected abstract void HandleNextTurn(GameLogic gameLogic);
    
    protected void ProcessMove(GameLogic gameLogic, Constants.PlayerType playerType,int row, int col)
    {
        if (gameLogic.SetNewBoardValue(playerType, row, col))
        {
            var gameResult = gameLogic.CheckGameResult();
            
            if (gameResult == GameLogic.GameResult.None)
            {
                HandleNextTurn(gameLogic);
            }
            else
            {
                gameLogic.EndGame(gameResult);
            }
        }
    }
}

// 직접 플레이 (싱글, 네트워크)
public class PlayerState : BasePalyerSate
{
    private Constants.PlayerType _playerType;
    private bool _isFirstPlayer;
    
    private MultiplayManager _multiplayManager;
    private string _roomId;
    private bool _isMultiplay;
    
    public PlayerState(bool isFirstPlayer)
    {
        _isFirstPlayer = isFirstPlayer;
        _playerType = _isFirstPlayer ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
        _isMultiplay = false;
    }

    public PlayerState(bool isFirstPlayer, MultiplayManager multiplayManager, string roomId) : this(isFirstPlayer)
    {
        _multiplayManager = multiplayManager;
        _roomId = roomId;
        _isMultiplay = true;
    }
    
    public override void OnEnter(GameLogic gameLogic)
    {
        gameLogic.blockController.OnBlockClickedDelegate = (row, col) =>
        {
            HandleMove(gameLogic, row, col);
        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        gameLogic.blockController.OnBlockClickedDelegate = null;
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        ProcessMove(gameLogic, _playerType, row, col);
        if (_isMultiplay)
        {
            _multiplayManager.SendPlayerMove(_roomId, row * 3 + col);
        }
    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        if (_isFirstPlayer)
        {
            gameLogic.SetState(gameLogic.secondPalyerSate);
        }
        else
        {
            gameLogic.SetState(gameLogic.firstPalyerSate);
        }
    }
}

// AI 플레이
public class AIState : BasePalyerSate
{
    public override void OnEnter(GameLogic gameLogic)
    {
        // AI 연산
        var result = MiniMaxAIController.GetBestMove(gameLogic.GetBoard());
        if (result.HasValue)
        {
            HandleMove(gameLogic, result.Value.row, result.Value.col);
        }
        else
        {
            gameLogic.EndGame(GameLogic.GameResult.Draw);
        }
    }

    public override void OnExit(GameLogic gameLogic)
    {
       
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        ProcessMove(gameLogic, Constants.PlayerType.PlayerB, row, col);
    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        gameLogic.SetState(gameLogic.firstPalyerSate);
    }
}

// 네트워크 플레이
public class MultiplayState : BasePalyerSate
{
    private Constants.PlayerType _playerType;
    private bool _isFirstPlayer;

    private MultiplayManager _multiplayManager;
    
    public MultiplayState(bool isFirstPlayer, MultiplayManager multiplayManager)
    {
        _isFirstPlayer = isFirstPlayer;
        _playerType = _isFirstPlayer ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
        _multiplayManager = multiplayManager;
    }
    
    public override void OnEnter(GameLogic gameLogic)
    {
        _multiplayManager.OnOpponentMove = moveData =>
        {
            var row = moveData.position / 3;
            var col = moveData.position % 3;
            UnityThread.executeInUpdate(() =>
            {
                HandleMove(gameLogic, row, col);
            });
        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        _multiplayManager.OnOpponentMove = null;
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        ProcessMove(gameLogic, _playerType, row, col);
    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        if (_isFirstPlayer)
        {
            gameLogic.SetState(gameLogic.secondPalyerSate);
        }
        else
        {
            gameLogic.SetState(gameLogic.firstPalyerSate);
        }
    }
}

public class GameLogic : IDisposable
{
    public BlockController blockController;
    private Constants.PlayerType[,] _board;
    
    public BasePalyerSate firstPalyerSate;        // 첫 번째 턴 상태 객체
    public BasePalyerSate secondPalyerSate;       // 두 번째 턴 상태 객체  
    private BasePalyerSate _currentPalyerSate;    // 현재 턴 상태 겍체
    
    private MultiplayManager _multiplayManager;
    private string _roomId;
    
    public enum GameResult
    {
        None,   // 게임 진행 중
        Win,    // 플레이어 승
        Lose,   // 플레이어 패
        Draw    // 비김
    }

    public GameLogic(BlockController blockController, 
        Constants.GameType gameType,
        MultiplayManager multiplayManager = null,
        string roomId = null,
        bool isFirstPlayer = true)
    {
        this.blockController = blockController;
        
        // _board 초기화
        _board = new Constants.PlayerType[3, 3];

        switch (gameType)
        {
            case Constants.GameType.SinglePlayer:
            {
                firstPalyerSate = new PlayerState(true);
                secondPalyerSate = new AIState();
                
                // 게임 시작
                SetState(firstPalyerSate);
                break;
            }
            case Constants.GameType.DualPlayer:
            {
                firstPalyerSate = new PlayerState(true);
                secondPalyerSate = new PlayerState(false);
                
                // 게임 시작
                SetState(firstPalyerSate);
                break;
            }
            case Constants.GameType.Multiplayer:
            {
                // Multiplay Manager 생성
                _multiplayManager = new MultiplayManager((state, roomId) =>
                {
                    _roomId = roomId;
                    switch (state)
                    {
                        case Constants.MultiplayManagerState.CreateRoom:
                            Debug.Log("## Create Room");
                            // TODO: 대기 화면 표시
                            // GameManager에게 대기 화면 표시 요청
                            break;
                        case Constants.MultiplayManagerState.JoinRoom:
                            Debug.Log("## Join Room");
                            firstPalyerSate = new MultiplayState(true, _multiplayManager);
                            secondPalyerSate = new PlayerState(false, _multiplayManager, roomId);
                            // 게임 시작
                            SetState(firstPalyerSate);
                            break;
                        case Constants.MultiplayManagerState.StartGame:
                            Debug.Log("## Start Game");
                            firstPalyerSate = new PlayerState(true, _multiplayManager, roomId);
                            secondPalyerSate = new MultiplayState(false, _multiplayManager);
                            // 게임 시작
                            SetState(firstPalyerSate);
                            break;
                        case Constants.MultiplayManagerState.ExitRoom:
                            Debug.Log("## Exit Room");
                            // TODO: Exit Room 처리
                            break;
                        case Constants.MultiplayManagerState.EndGame:
                            Debug.Log("## End Game");
                            // TODO: End Room 처리
                            break;
                    }
                });
                break;
            }
        }
    }

    public void SetState(BasePalyerSate state)
    {
        _currentPalyerSate?.OnExit(this);
        _currentPalyerSate = state;
        _currentPalyerSate?.OnEnter(this);
    }
    
    /// <summary>
    /// _board에 새로운 값을 할당하는 함수
    /// </summary>
    /// <param name="playerType">할당하고자 하는 플레이어 타입</param>
    /// <param name="row">Row</param>
    /// <param name="col">Col</param>
    /// <returns>False가 반환되면 할당할 수 없음, True는 할당이 완료됨</returns>
    public bool SetNewBoardValue(Constants.PlayerType playerType, int row, int col)
    {
        if (_board[row, col] != Constants.PlayerType.None) return false;
        
        if (playerType == Constants.PlayerType.PlayerA)
        {
            _board[row, col] = playerType;
            blockController.PlaceMarker(Block.MarkerType.O, row, col);
            return true;
        }
        else if (playerType == Constants.PlayerType.PlayerB)
        {
            _board[row, col] = playerType;
            blockController.PlaceMarker(Block.MarkerType.X, row, col);
            return true;
        }
        return false;   
    }
     
    /// <summary>
    /// 게임 결과 확인 함수
    /// </summary>
    /// <returns>플레이어 기준 게임 결과</returns>
    public GameResult CheckGameResult()
    {
        if (CheckGameWin(Constants.PlayerType.PlayerA)) { return GameResult.Win; }
        if (CheckGameWin(Constants.PlayerType.PlayerB)) { return GameResult.Lose; }
        if (MiniMaxAIController.IsAllBlocksPlaced(_board)) { return GameResult.Draw; }
        
        return GameResult.None;
    }
    
    //게임의 승패를 판단하는 함수
    private bool CheckGameWin(Constants.PlayerType playerType)
    {
        // 가로로 마커가 일치하는지 확인
        for (var row = 0; row < _board.GetLength(0); row++)
        {
            if (_board[row, 0] == playerType && _board[row, 1] == playerType && _board[row, 2] == playerType)
            {
                (int, int)[] blocks = { ( row, 0 ), ( row, 1 ), ( row, 2 ) };
                blockController.SetBlockColor(playerType, blocks);
                return true;
            }
        }
        
        // 세로로 마커가 일치하는지 확인
        for (var col = 0; col < _board.GetLength(1); col++)
        {
            if (_board[0, col] == playerType && _board[1, col] == playerType && _board[2, col] == playerType)
            {
                (int, int)[] blocks = { ( 0, col ), ( 1, col ), ( 2, col ) };
                blockController.SetBlockColor(playerType, blocks);
                return true;
            }
        }
        
        // 대각선 마커 일치하는지 확인
        if (_board[0, 0] == playerType && _board[1, 1] == playerType && _board[2, 2] == playerType)
        {
            (int, int)[] blocks = { ( 0, 0 ), ( 1, 1 ), ( 2, 2 ) };
            blockController.SetBlockColor(playerType, blocks);
            return true;
        }
        if (_board[0, 2] == playerType && _board[1, 1] == playerType && _board[2, 0] == playerType)
        {
            (int, int)[] blocks = { ( 0, 2 ), ( 1, 1 ), ( 2, 0 ) };
            blockController.SetBlockColor(playerType, blocks);
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// 게임 오버시 호출되는 함수
    /// gameResult에 따라 결과 출력
    /// </summary>
    /// <param name="gameResult">win, lose, draw</param>
    public void EndGame(GameResult gameResult)
    {
        SetState(null);
        
        firstPalyerSate = null;
        secondPalyerSate = null;
        
        GameManager.Instance.OpenGameOverPanel();
    }

    public Constants.PlayerType[,] GetBoard()
    {
        return _board;
    }

    public void Dispose()
    {
        _multiplayManager?.LeaveRoom(_roomId);
        _multiplayManager?.Dispose();
    }
}
