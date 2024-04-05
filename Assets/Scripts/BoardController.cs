using UnityEngine;

namespace Board
{
    public class BoardController : IBoardController
    {
        private IDropItemDeterminer _dropItemDeterminer;
        private int _columnCount;
        private int _rowCount;
        private float _cellSize;
        private DropItemModel[,] _dropItemModelList;
        private BoardView _view;
        private Vector2 _originPosition;
        
        public BoardController(BoardView view)
        {
            _view = view;
            _originPosition = Vector2.zero;
        }
        
        public void InitializeBoard(int columnCount, int rowCount)
        {
            _dropItemDeterminer = new DropItemDeterminer();
            _columnCount = columnCount;
            _rowCount = rowCount;
            _dropItemModelList = new DropItemModel[_columnCount, _rowCount];
            Vector2 boardSize = new BoardSizeCalculator().GetBoardSize(_columnCount, _rowCount, _view.GetCamera());
            _cellSize = boardSize.y / _rowCount;
            SetDropItemModelList();
            Vector2 cameraPosition = new Vector2((columnCount - 1) * _cellSize / 2, (rowCount - 1) * _cellSize / 2);
            _view.SetCameraPosition(cameraPosition);
            _view.SetOnClick(GetIndexOfDropItem);
            _view.SetDropItemViews(_dropItemModelList);
        }

        private void SetDropItemModelList()
        {
            DropItemType[,] initialDropItemTypeList = _dropItemDeterminer.GetInitialDropItemTypes(_columnCount, _rowCount);
            Vector2 sizeOfSprite = _view.GetOriginalSizeOfSprites();
            Vector2 localScaleOfDropItem = new Vector2(1 / sizeOfSprite.x, 1 / sizeOfSprite.y) * _cellSize;
            for (int i = 0; i < _columnCount; i++)
            {
                for (int j = 0; j < _rowCount; j++)
                {
                    DropItemModel dropItemModel = new DropItemModel()
                    {
                        dropItemType = initialDropItemTypeList[i, j],
                        columnIndex = i,
                        rowIndex = j,
                        localScale = localScaleOfDropItem,
                        position = new Vector2(i, j) * _cellSize + _originPosition
                    };
                    _dropItemModelList[i, j] = dropItemModel;
                }
            }
        }
        
        private void GetIndexOfDropItem(in Vector2 worldPosition, out int columnIndex, out int rowIndex) {
            columnIndex = Mathf.FloorToInt(((worldPosition - _originPosition).x + _cellSize / 2) / _cellSize);
            rowIndex = Mathf.FloorToInt(((worldPosition - _originPosition).y + _cellSize / 2) / _cellSize);

            if (columnIndex < 0 || columnIndex >= _columnCount || rowIndex < 0 || rowIndex >= _rowCount)
            {
                columnIndex = -1;
                rowIndex = -1;
            }
        }    
    }

    public interface IBoardController
    {
        void InitializeBoard(int columnCount, int rowCount);
    }
}