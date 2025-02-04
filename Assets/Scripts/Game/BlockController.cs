using UnityEngine;

public class BlockController : MonoBehaviour
{
    [SerializeField] private Block[] blocks;

    /// <summary>
    /// 특정 Block에 마커 표시하는 함수
    /// </summary>
    /// <param name="markerType">마커 타입</param>
    /// <param name="row">Row</param>
    /// <param name="col">Col</param>
    public void PlaceMarker(Block.MarkerType markerType, int row, int col)
    {
        // row, col을 index로 변환
        var markerIndex = row * 3 + col;
        
        // Block에게 마커 표시
        blocks[markerIndex].SetMarker(markerType);
    }
}