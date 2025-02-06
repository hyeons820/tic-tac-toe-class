using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmPanel;
    
    private BlockController _blockController;
    private GameUIController _gameUIController;
    private Canvas _canvas;
    
    public enum PlayerType { None, PlayerA, PlayerB }
    private PlayerType[,] _board;

    private enum TurnType { None, PlayerA, PlayerB }

    private enum GameResult
    {
        None,   // 게임 진행 중
        Win,    // 플레이어 승
        Lose,   // 플레이어 패
        Draw    // 비김
    }

    public enum GameType { SinglePlayer, DualPlayer }
    
    public void ChangeToGameScene(GameType gameType)
    {
        SceneManager.LoadScene("Game");
    }

    public void ChangeToMainScene()
    {
        SceneManager.LoadScene("Main");
    }

    public void OpenSettingsPanel()
    {
        if (_canvas != null)
        {
            var settingsPanelObject = Instantiate(settingsPanel, _canvas.transform);
            settingsPanelObject.GetComponent<PanelController>().Show();
        }
    }

    public void OpenConfirmPanel(string message, ConfirmPanelController.OnConfirmButtonClick onConfirmButtonClick)
    {
        if (_canvas != null)
        {
            var confirmPanelObject = Instantiate(confirmPanel, _canvas.transform);
            confirmPanelObject.GetComponent<ConfirmPanelController>()
                .Show(message, onConfirmButtonClick);
        }
    }

    /// <summary>
    /// 게임 시작
    /// </summary>
    private void StartGame()
    {
        // _board 초기화
        _board = new PlayerType[3, 3];
        
        // Block 초기화
        _blockController.InitBlocks();
        
        // Game UI 초기화
        _gameUIController.SetGameUIMode(GameUIController.GameUIMode.Init);

        // 턴 시작
        SetTurn(TurnType.PlayerA);
    }
    
    /// <summary>
    /// 게임 오버시 호출되는 함수
    /// gameResult에 따라 결과 출력
    /// </summary>
    /// <param name="gameResult">win, lose, draw</param>
    private void EndGame(GameResult gameResult)
    {
        // 게임오버 표시
        _gameUIController.SetGameUIMode(GameUIController.GameUIMode.GameOver);
        
        // TODO: 나중에 구현!!
        switch (gameResult)
        {
            case GameResult.Win:
                Debug.Log("Player win!");
                break;
            case GameResult.Lose:
                Debug.Log("Player lost!");
                break;
            case GameResult.Draw:
                Debug.Log("Draw!");
                break;
        }
    }

    /// <summary>
    /// _board에 새로운 값을 할당하는 함수
    /// </summary>
    /// <param name="playerType">할당하고자 하는 플레이어 타입</param>
    /// <param name="row">Row</param>
    /// <param name="col">Col</param>
    /// <returns>False가 반환되면 할당할 수 없음, True는 할당이 완료됨</returns>
    private bool SetNewBoardValue(PlayerType playerType, int row, int col)
    {
        if (playerType == PlayerType.PlayerA)
        {
            _board[row, col] = playerType;
            _blockController.PlaceMarker(Block.MarkerType.O, row, col);
            return true;
        }
        else if (playerType == PlayerType.PlayerB)
        {
            _board[row, col] = playerType;
            _blockController.PlaceMarker(Block.MarkerType.X, row, col);
            return true;
        }
        return false;   
    }
    
    private void SetTurn(TurnType turnType)
    {
        switch (turnType)
        {
            case TurnType.PlayerA:
                _gameUIController.SetGameUIMode(GameUIController.GameUIMode.TurnA);
                _blockController.onBlockClickedDelegate = (row, col) =>
                {
                    if (_board[row, col] == PlayerType.None)
                    {
                        if (SetNewBoardValue(PlayerType.PlayerA, row, col))
                        {
                            var gameResult = CheckGameResult();
                            if (gameResult == GameResult.None)
                                SetTurn(TurnType.PlayerB);
                            else
                            {
                                SetTurn(TurnType.None);
                                EndGame(gameResult);
                            }
                        }
                    }
                    else
                    {
                        _blockController.onBlockClickedDelegate = null;
                        SetTurn(TurnType.PlayerA);
                    }
                    
                };
                break;
            case TurnType.PlayerB:
                _gameUIController.SetGameUIMode(GameUIController.GameUIMode.TurnB);
                _blockController.onBlockClickedDelegate = (row, col) =>
                {
                    if (_board[row, col] == PlayerType.None)
                    {
                        if (SetNewBoardValue(PlayerType.PlayerB, row, col))
                        {
                            var gameResult = CheckGameResult();
                            if (gameResult == GameResult.None)
                                SetTurn(TurnType.PlayerA);
                            else
                            {
                                SetTurn(TurnType.None);
                                EndGame(gameResult);
                            }
                        }
                    }
                    else
                    {
                        _blockController.onBlockClickedDelegate = null;
                        SetTurn(TurnType.PlayerB);
                    }
                };
                break;
            case TurnType.None:
                _blockController.onBlockClickedDelegate = (row, col) =>
                {
                    _gameUIController.SetGameUIMode(GameUIController.GameUIMode.GameOver);
                };
            break;
        }
        
        // 게임 결과 확인
        /*switch (CheckGameResult())
        {
            case GameResult.Win:
                // TODO: 승리 결과창 표시
                break;
            case GameResult.Lose:
                // TODO: 패배 결과창 표시
                break;
            case GameResult.Draw:
                // TODO: 비김 결과창 표시
                break;
            case GameResult.None:
                // 게임이 종료되지 않았다면, 턴 변경
                var nextTurn = turnType == TurnType.PlayerA ? TurnType.PlayerB : TurnType.PlayerA;
                SetTurn(nextTurn);
                break;
        }*/
        
    }
    
        
    /// <summary>
    /// 게임 결과 확인 함수
    /// </summary>
    /// <returns>플레이어 기준 게임 결과</returns>
    private GameResult CheckGameResult()
    {
        if (CheckGameWin(PlayerType.PlayerA)) { return GameResult.Win; }
        if (CheckGameWin(PlayerType.PlayerB)) { return GameResult.Lose; }
        if (IsAllBlocksPlaced()) { return GameResult.Draw; }
        
        return GameResult.None;
    }
    
    // 모든 마커가 보드에 배치 되었는지 확인하는 함수
    private bool IsAllBlocksPlaced()
    {
        for (var row = 0; row < _board.GetLength(0); row++)
        {
            for (var col = 0; col < _board.GetLength(1); col++)
            {
                if (_board[row, col] == PlayerType.None)
                    return false;
            }
        }
        return true;
    }
    
    // 게임의 승패를 판단하는 함수
    private bool CheckGameWin(PlayerType playerType)
    {
        // 가로로 마커가 일치하는지 확인
        for (var row = 0; row < _board.GetLength(0); row++)
        {
            if (_board[row, 0] == playerType && _board[row, 1] == playerType && _board[row, 2] == playerType)
            {
                (int, int)[] blocks = { ( row, 0 ), ( row, 1 ), ( row, 2 ) };
                _blockController.SetBlockColor(playerType, blocks);
                return true;
            }
        }
        
        // 세로로 마커가 일치하는지 확인
        for (var col = 0; col < _board.GetLength(1); col++)
        {
            if (_board[0, col] == playerType && _board[1, col] == playerType && _board[2, col] == playerType)
            {
                (int, int)[] blocks = { ( 0, col ), ( 1, col ), ( 2, col ) };
                _blockController.SetBlockColor(playerType, blocks);
                return true;
            }
        }
        
        // 대각선 마커 일치하는지 확인
        if (_board[0, 0] == playerType && _board[1, 1] == playerType && _board[2, 2] == playerType)
        {
            (int, int)[] blocks = { ( 0, 0 ), ( 1, 1 ), ( 2, 2 ) };
            _blockController.SetBlockColor(playerType, blocks);
            return true;
        }
        if (_board[0, 2] == playerType && _board[1, 1] == playerType && _board[2, 0] == playerType)
        {
            (int, int)[] blocks = { ( 0, 2 ), ( 1, 1 ), ( 2, 0 ) };
            _blockController.SetBlockColor(playerType, blocks);
            return true;
        }
        
        return false;
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            _blockController = GameObject.FindObjectOfType<BlockController>();
            _gameUIController = GameObject.FindObjectOfType<GameUIController>();
            
            // 게임 시작
            StartGame();
        }
        
        _canvas = GameObject.FindObjectOfType<Canvas>();
    }
}