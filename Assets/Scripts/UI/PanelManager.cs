using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [SerializeField] private PanelController startPanelController;

    public enum PanelType { StartPanel, WinPanel, DrawPanel, LosePanel }
    private PanelController _currentPanelController;
    
    /// <summary>
    /// 표시할 패널 정보 전달하는 함수
    /// </summary>
    /// <param name="panelType"></param>
    public void ShowPanel(PanelType panelType)
    {
        switch (panelType)
        {
            case PanelType.StartPanel:
                ShowPanelController(startPanelController);
                break;
            case PanelType.WinPanel:
                
                break;
            case PanelType.DrawPanel:
                
                break;
            case PanelType.LosePanel:
                
                break;
        }
    }

    /// <summary>
    /// 패널을 표시하는 함수
    /// 기존 패널이 있으면 Hide하고 새로운 패널을 Show 함
    /// </summary>
    /// <param name="panelController">표시할 패널</param>
    private void ShowPanelController(PanelController panelController)
    {
        if (_currentPanelController != null)
        {
            _currentPanelController.Hide();
        }
        panelController.Show(() =>
        {
            _currentPanelController = null;
        });
        _currentPanelController = panelController;
    }
}
