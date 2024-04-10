using System.Collections.Generic;
using UnityEngine;

namespace Board
{
    public class GameCreator : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;
        [SerializeField] private int columnCount = 8;
        [SerializeField] private int rowCount = 8;
        [SerializeField] private List<int> nonSpawnableColumnIndices;
        public void Start()
        {
            Vector2 originPosition = Vector2.zero;
            IActiveCellModelsManager activeCellModelsManager = new ActiveCellModelsManager();
            ISwapManager swapManager = new SwapManager(boardView, activeCellModelsManager, originPosition);
            IBoardController boardController = new BoardController(boardView, activeCellModelsManager, originPosition, swapManager, nonSpawnableColumnIndices);
            
            boardController.InitializeBoard(rowCount,columnCount);
        }
    }
}