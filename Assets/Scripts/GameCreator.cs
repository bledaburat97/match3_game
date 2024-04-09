using UnityEngine;

namespace Board
{
    public class GameCreator : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;
        
        public void Start()
        {
            Vector2 originPosition = Vector2.zero;
            IActiveCellModelsManager activeCellModelsManager = new ActiveCellModelsManager();
            ISwapManager swapManager = new SwapManager(boardView, activeCellModelsManager, originPosition);
            IBoardController boardController = new BoardController(boardView, activeCellModelsManager, originPosition, swapManager);
            
            boardController.InitializeBoard(8,8);
        }
    }
}