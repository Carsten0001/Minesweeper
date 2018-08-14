using Minesweeper.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using Minesweeper.ViewModels;

namespace Minesweeper.Model
{
    public sealed class MinesCore
    {
        private static readonly Lazy<MinesCore> _lazy = new Lazy<MinesCore>(() => new MinesCore());
        private int _numberOfTiles;
        private int _sizeX;
        private int _sizeY;
        private ObservableCollection<Tile> _tiles;

        public bool GameOver { get; set; } = true;

        public int NumberOfMines { get; private set; }

        public Dictionary<StateImages, BitmapSource> BitmapSources { get; private set; }

        public static MinesCore Instance
        {
            get
            {
                return _lazy.Value;
            }
        }

        private MinesCore()
        {
            BitmapSources = new Dictionary<StateImages, BitmapSource>();
            Dictionary<StateImages, Bitmap> images = Resources.ResourceManager
                                       .GetResourceSet(CultureInfo.CurrentCulture, true, true)
                                       .Cast<DictionaryEntry>()
                                       .Where(x => x.Value.GetType() == typeof(Bitmap))
                                       .ToDictionary(x => (StateImages)Enum.Parse(typeof(StateImages), x.Key.ToString()), x => x.Value as Bitmap);
            foreach (var image in images)
            {
                BitmapSources.Add(image.Key, (Imaging.CreateBitmapSourceFromHBitmap(image.Value.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(image.Value.Width, image.Value.Height))));
            }
        }

        public int StartGame(GameMode gameMode, Difficulty difficulty, int sizeX, int sizeY, int? numberOfMines, ref ObservableCollection<Tile> tiles)
        {
            _tiles = tiles;
            _sizeX = sizeX;
            _sizeY = sizeY;
            _numberOfTiles = sizeX * sizeY;
            if (gameMode == GameMode.Standard && GameOver)
            {
                switch (difficulty)
                {
                    case Difficulty.Easy:
                        NumberOfMines = (int)Math.Sqrt(_numberOfTiles);
                        break;
                    case Difficulty.Normal:
                        NumberOfMines = _numberOfTiles / 3;
                        break;
                    case Difficulty.Hard:
                        NumberOfMines = _numberOfTiles / 2;
                        break;
                }
                GameOver = false;
            }
            else if (gameMode == GameMode.Custom && GameOver && numberOfMines != null)
            {
                NumberOfMines = numberOfMines.Value;
            }
            return NumberOfMines;
        }

        public void GameLost()
        {
            GameOver = true;
            foreach ( Tile tile in _tiles)
            {
                if ((tile.DataContext as TileViewModel).HasMine && (tile.DataContext as TileViewModel).TileStateImage == BitmapSources[StateImages.None])
                    (tile.DataContext as TileViewModel).TileStateImage = BitmapSources[StateImages.Mine];
            }
        }

        public void OpenField(int id)
        {
            // Upper Left Corner
            if (id == 0)
            {
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(CheckUpperLeftCorner(id))];
            }
            else
            // Upper Middle Tiles
            if (id % _sizeX > 0 && id % _sizeX < _sizeX - 1 && id < _sizeX)
            {
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(CheckUpperMid(id))];
            }
            else
            // Upper Right Corner
            if (id == _sizeX - 1)
            {
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(CheckUpperRightCorner(id))];
            }
            else
            // Left Middle Tiles
            if (id % _sizeX == 0 && id >= _sizeX && id < _sizeX * _sizeY - _sizeX)
            {
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(CheckLeftMid(id))];
            }
            else
            // Right Middle Tiles
            if (id % _sizeX == _sizeX - 1 && id > _sizeX && id <= _sizeX * _sizeY - _sizeX)
            {
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(CheckRightMid(id))];
            }
            else
            // Bottom Left Corner
            if (id == _sizeX * _sizeY - _sizeX)
            {
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(CheckBottomLeftCorner(id))];
            }
            else
            // Bottom Middle Tiles
            if (id % _sizeX > 0 && id % _sizeX < _sizeX - 1 && id > _sizeX * _sizeY - _sizeX)
            {
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(CheckBottomMid(id))];
            }
            else
            // Bottom Right Corner
            if (id == _sizeX * _sizeY - 1)
            {
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(CheckBottomRightCorner(id))];
            }
            else
            // Center Tiles
            {
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(CheckCenter(id))];
            }
            CheckIfWon();

        }

        /// <summary>
        /// Returns Number of Mines surrounding Left Corner of Grid
        /// </summary>
        /// <param name="id">Id of the Tile</param>
        /// <returns>Number of Mines around</returns>
        private int CheckUpperLeftCorner(int id)
        {
            var counter = 0;
            if ((_tiles[id+1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id+_sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id+_sizeX+1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            return counter;
        }

        /// <summary>
        /// Returns Number of Mines surrounding Upper Middle Tiles of Grid
        /// </summary>
        /// <param name="id">Id of the Tile</param>
        /// <returns>Number of Mines around</returns>
        private int CheckUpperMid(int id)
        {
            var counter = 0;
            if ((_tiles[id - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + _sizeX + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            return counter;
        }

        /// <summary>
        /// Returns Number of Mines surrounding Upper Right Corner of Grid
        /// </summary>
        /// <param name="id">Id of the Tile</param>
        /// <returns>Number of Mines around</returns>
        private int CheckUpperRightCorner(int id)
        {
            var counter = 0;
            if ((_tiles[id - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            return counter;
        }

        /// <summary>
        /// Returns Number of Mines surrounding Left Mid of Grid
        /// </summary>
        /// <param name="id">Id of the Tile</param>
        /// <returns>Number of Mines around</returns>
        private int CheckLeftMid(int id)
        {
            var counter = 0;
            if ((_tiles[id - _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id - _sizeX + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + _sizeX + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            return counter;
        }

        /// <summary>
        /// Returns Number of Mines surrounding Left Mid of Grid
        /// </summary>
        /// <param name="id">Id of the Tile</param>
        /// <returns>Number of Mines around</returns>
        private int CheckRightMid(int id)
        {
            var counter = 0;
            if ((_tiles[id - _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id - _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            return counter;
        }

        /// <summary>
        /// Returns Number of Mines surrounding Left Bottom Corner of Grid
        /// </summary>
        /// <param name="id">Id of the Tile</param>
        /// <returns>Number of Mines around</returns>
        private int CheckBottomLeftCorner(int id)
        {
            var counter = 0;
            if ((_tiles[id - _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id - _sizeX + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            return counter;
        }

        /// <summary>
        /// Returns Number of Mines surrounding Bottom Mid Tiles of Grid
        /// </summary>
        /// <param name="id">Id of the Tile</param>
        /// <returns>Number of Mines around</returns>
        private int CheckBottomMid(int id)
        {
            var counter = 0;
            if ((_tiles[id - _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id - _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id - _sizeX + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            return counter;
        }

        /// <summary>
        /// Returns Number of Mines surrounding Bottom Right Corner of Grid
        /// </summary>
        /// <param name="id">Id of the Tile</param>
        /// <returns>Number of Mines around</returns>
        private int CheckBottomRightCorner(int id)
        {
            var counter = 0;
            if ((_tiles[id - _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id - _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            return counter;
        }

        /// <summary>
        /// Returns Number of Mines surrounding Center Tiles of Grid
        /// </summary>
        /// <param name="id">Id of the Tile</param>
        /// <returns>Number of Mines around</returns>
        private int CheckCenter(int id)
        {
            var counter = 0;
            if ((_tiles[id - _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id - _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id - _sizeX + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((_tiles[id + _sizeX + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            return counter;
        }

        private void CheckIfWon()
        {
            var counter = 0;
            foreach ( Tile tile in _tiles)
            {
                if ((tile.DataContext as TileViewModel).HasMine == false && (tile.DataContext as TileViewModel).TileStateImage != BitmapSources[StateImages.None])
                {
                    counter++;
                }
            }
            if (counter == _sizeX * _sizeY - NumberOfMines)
            {
                MessageBox.Show("You won!!!");
            }
            
        }

        private StateImages GetStateImage(int number)
        {
            switch (number)
            {
                case 0: return StateImages.Null;
                case 1: return StateImages.One;
                case 2: return StateImages.Two;
                case 3: return StateImages.Three;
                case 4: return StateImages.Four;
                case 5: return StateImages.Five;
                case 6: return StateImages.Six;
                case 7: return StateImages.Seven;
                case 8: return StateImages.Eight;
                default: return StateImages.None;

            }
        }
    }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    public enum GameMode
    {
        Standard,
        Custom
    }

    public enum StateImages
    {
        None,
        Null,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Flag,
        Questionmark,
        Mine,
        Explosion
    }
}
