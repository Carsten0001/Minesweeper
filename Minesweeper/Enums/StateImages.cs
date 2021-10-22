namespace Minesweeper.Enums
{
    /// <summary>
    /// The Names of the Images which could be displayed on the Tiles
    /// </summary>
    public enum StateImages
    {
        /// <summary>
        /// The Tile is revealed and has no Mine
        /// </summary>
        None,

        /// <summary>
        /// Initial State of Mine
        /// </summary>
        Null,

        /// <summary>
        /// The Tile is revealed and has one Mine
        /// </summary>
        One,

        /// <summary>
        /// The Tile is revealed and has two Mines
        /// </summary>
        Two,

        /// <summary>
        /// The Tile is revealed and has three Mines
        /// </summary>
        Three,

        /// <summary>
        /// The Tile is revealed and has four Mines
        /// </summary>
        Four,

        /// <summary>
        /// The Tile is revealed and has five Mines
        /// </summary>
        Five,

        /// <summary>
        /// The Tile is revealed and has six Mines
        /// </summary>
        Six,

        /// <summary>
        /// The Tile is revealed and has seven Mines
        /// </summary>
        Seven,

        /// <summary>
        /// The Tile is revealed and has eight Mines
        /// </summary>
        Eight,

        /// <summary>
        /// Tile right clicked once
        /// </summary>
        Flag,

        /// <summary>
        /// Tile right clicked twice
        /// </summary>
        Questionmark,

        /// <summary>
        /// The player has lost the game and Mine has been reveled automatically
        /// </summary>
        Mine,

        /// <summary>
        /// The player has clicked a Tile which has bomb
        /// </summary>
        Explosion
    }
}