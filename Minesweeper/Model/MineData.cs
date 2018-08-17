namespace Minesweeper.Model
{
    /// <summary>
    /// Model for Observable
    /// </summary>
    public class MineData
    {
        internal MineData(int numberOfMines, int remainingMines, int sizeX, int sizeY, bool gameOver)
        {
            NumberOfMines = numberOfMines;
            RemainingMines = remainingMines;
            SizeX = sizeX;
            SizeY = sizeY;
            GameOver = gameOver;
        }
        /// <summary>
        /// Number of Mines
        /// </summary>
        public int NumberOfMines { get; }
        /// <summary>
        /// The Mines which are not flagged
        /// </summary>
        public int RemainingMines { get; }
        /// <summary>
        /// Width of the GameBoard
        /// </summary>
        public int SizeX { get; }
        /// <summary>
        /// Height of the GameBoard
        /// </summary>
        public int SizeY { get; }
        /// <summary>
        /// Indicates if the game is over
        /// </summary>
        public bool GameOver { get; }
    }
}
