using UnityEngine;

namespace Board
{
    public class GameCreator : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;
        
        public void Start()
        {
            IBoardController boardController = new BoardController(boardView);
            boardController.InitializeBoard(8,8);
        }
    }
}