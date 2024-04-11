using UnityEngine;

namespace Board
{
    public class BoardSizeCalculator
    {
        private float _maxBoardHeightToScreenHeightRatio = 0.7f;
        private float _maxBoardWidthToScreenWidthRatio = 0.9f;
        
        //Set the board size according to screen size and constant boundaries.
        public Vector2 GetBoardSize(int columnCount, int rowCount, Camera camera)
        {
            float screenHeight = camera.orthographicSize * 2.0f;
            float screenWidth = screenHeight * Screen.width / Screen.height;
            float aspectRatio = (float) columnCount / rowCount;
            float maxBoardWidth = _maxBoardWidthToScreenWidthRatio * screenWidth;
            float maxBoardHeight = _maxBoardHeightToScreenHeightRatio * screenHeight;

            if (maxBoardWidth / maxBoardHeight > aspectRatio)
                return new Vector2(maxBoardHeight * aspectRatio, maxBoardHeight);
            else return new Vector2(maxBoardWidth, maxBoardWidth / aspectRatio);
        }
    }
}