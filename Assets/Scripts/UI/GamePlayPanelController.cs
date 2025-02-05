using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayPanelController : MonoBehaviour
{
    [SerializeField] private Image player1Image;
    [SerializeField] private Image player2Image;
    
    
    Color32 myTurnColor = new Color32(0, 0, 0, 255);
    Color32 notMyTurnColor = new Color32(173, 173, 173, 255);
    
    public void SetPlayerTurnColor(GameManager.TurnType turnType)
    {
        if (turnType == GameManager.TurnType.PlayerA)
        {
            if (player1Image.color == notMyTurnColor)
                player1Image.color = myTurnColor;

            if (player2Image.color == myTurnColor)
                player2Image.color = notMyTurnColor;
        }
        else if (turnType == GameManager.TurnType.PlayerB)
        {
            if (player1Image.color == myTurnColor)
                player1Image.color = notMyTurnColor;
        
            if (player2Image.color == notMyTurnColor)
                player2Image.color = myTurnColor;
        }
    }
}
