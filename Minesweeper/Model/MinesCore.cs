using Minesweeper.Properties;
using Minesweeper.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Minesweeper.Model
{
    /// <summary>
    /// The Heart of the Game
    /// Mediator for the ViewModels
    /// Holds all the values, which are nessecary for the game
    /// </summary>
    public sealed class MinesCore : IObservable<MineData>
    {
        #region Fields
        private static List<IObserver<MineData>> _observers;
        private static MineData _mineData;
        private static readonly Lazy<MinesCore> _lazy = new Lazy<MinesCore>(() => new MinesCore());
        private int _sizeX;
        private int _sizeY;
        private ObservableCollection<Tile> _tiles;
        #endregion

        #region Properties 
        /// <summary>
        /// Indicates if the game is over
        /// Default is true
        /// </summary>
        public bool GameOver { get; set; } = true;
        /// <summary>
        /// Number of Mines
        /// </summary>
        public int NumberOfMines { get; private set; }

        /// <summary>
        /// Countes the Mines beeing flagged
        /// </summary>
        public int FlaggedMinesCounter { get; set; }

        /// <summary>
        /// A Collection of all the Images which can be displayed on a Tile
        /// </summary>
        public Dictionary<StateImages, BitmapSource> BitmapSources { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Returns the Instance of the MinesCore Singleton by lazy loading
        /// </summary>
        public static MinesCore Instance
        {
            get
            {
                if (_observers == null)
                {
                    _observers = new List<IObserver<MineData>>();
                }
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
        #endregion

        #region Methods
        /// <summary>
        /// Starts the Game an initializes the List of Observers. 
        /// Calculates the size depended on GameMode and Difficulty.
        /// Calls <see cref="UpdateObservers()"/>
        /// </summary>
        /// <param name="gameMode"></param>
        /// <param name="difficulty"></param>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        /// <param name="numberOfMines"></param>
        /// <param name="tiles"></param>
        public void StartGame(GameMode gameMode, Difficulty difficulty, int sizeX, int sizeY, int? numberOfMines, ref ObservableCollection<Tile> tiles)
        {
            if (_observers == null)
            {
                _observers = new List<IObserver<MineData>>();
            }
            if (gameMode == GameMode.Standard)
            {
                switch (difficulty)
                {
                    case Difficulty.Easy: _sizeX = _sizeY = 8; break;
                    case Difficulty.Normal: _sizeX = _sizeY = 16; break;
                    case Difficulty.Hard: _sizeX = 16; _sizeY = 30; break;
                }
            }
            else
            {
                _sizeX = sizeX;
                _sizeY = sizeY;
            }
            _tiles = tiles;

            if (gameMode == GameMode.Standard && GameOver)
            {
                switch (difficulty)
                {
                    case Difficulty.Easy:
                        NumberOfMines = 10;
                        break;
                    case Difficulty.Normal:
                        NumberOfMines = 40;
                        break;
                    case Difficulty.Hard:
                        NumberOfMines = 99;
                        break;
                }
                GameOver = false;
            }
            else if (gameMode == GameMode.Custom && GameOver && numberOfMines != null)
            {
                NumberOfMines = numberOfMines.Value;
                GameOver = false;
            }
            _mineData = new MineData(NumberOfMines, FlaggedMinesCounter, _sizeX, _sizeY, GameOver);
            UpdateObservers();
        }
        /// <summary>
        /// Informs the MineCore that the User has revealed a mine.
        /// </summary>
        public void GameLost()
        {
            GameOver = true;
            foreach (Tile tile in _tiles)
            {
                if ((tile.DataContext as TileViewModel).HasMine && (tile.DataContext as TileViewModel).TileStateImage == BitmapSources[StateImages.None])
                    (tile.DataContext as TileViewModel).TileStateImage = BitmapSources[StateImages.Mine];
            }
            var result = MessageBox.Show("You LOST!!! Wanna Restart?", "Fatal FAIL", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes: GameOver = false; UpdateObservers(); break;
                case MessageBoxResult.No:; break;
            }
        }
        /// <summary>
        /// Subscribs to Observer list
        /// </summary>
        /// <param name="observer">A Observer which wants to get informed</param>
        /// <returns><see cref="Unsubscriber{MineData}"/></returns>
        public IDisposable Subscribe(IObserver<MineData> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
                observer.OnNext(_mineData);
            }
            return new Unsubscriber<MineData>(_observers, observer);
        }

        /// <summary>
        /// Updates all the Observers
        /// </summary>
        private void UpdateObservers()
        {
            _mineData = new MineData(NumberOfMines, FlaggedMinesCounter, _sizeX, _sizeY, GameOver);
            foreach (var observer in _observers)
            {
                observer.OnNext(_mineData);
            }
        }
        /// <summary>
        /// Calls <see cref="OpenField(int)"/> and checks if the player has won by calling <see cref="CheckIfWon"/>
        /// </summary>
        /// <param name="id"></param>
        public void RevealFieldAndCheckForWin(int id)
        {
            OpenField(id);
            CheckIfWon();
        }

        /// <summary>
        /// Changes the Button Image of the button specified by id
        /// </summary>
        /// <param name="id">id of the button that should be revealed</param>
        private void OpenField(int id)
        {
            if ((_tiles[id].DataContext as TileViewModel).TileStateImage == BitmapSources[StateImages.Null])
            {
                return;
            }
            var newID = -1;
            // Upper Left Corner
            if (id == 0)
            {
                newID = CheckUpperLeftCorner(id);
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(newID)];
                if (newID == 0)
                {
                    OpenField(id + 1);
                    OpenField(id + _sizeX);
                    OpenField(id + _sizeX + 1);
                }
            }
            else
            // Upper Middle Tiles
            if (id % _sizeX > 0 && id % _sizeX < _sizeX - 1 && id < _sizeX)
            {
                newID = CheckUpperMid(id);
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(newID)];
                if (newID == 0)
                {
                    OpenField(id - 1);
                    OpenField(id + 1);
                    OpenField(id + _sizeX - 1);
                    OpenField(id + _sizeX);
                    OpenField(id + _sizeX + 1);
                }
            }
            else
            // Upper Right Corner
            if (id == _sizeX - 1)
            {
                newID = CheckUpperRightCorner(id);
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(newID)];
                if (newID == 0)
                {
                    OpenField(id - 1);
                    OpenField(id + _sizeX - 1);
                    OpenField(id + _sizeX);
                }
            }
            else
            // Left Middle Tiles
            if (id % _sizeX == 0 && id >= _sizeX && id < _sizeX * _sizeY - _sizeX)
            {
                newID = CheckLeftMid(id);
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(newID)];
                if (newID == 0)
                {
                    OpenField(id - _sizeX);
                    OpenField(id - _sizeX + 1);
                    OpenField(id + 1);
                    OpenField(id + _sizeX);
                    OpenField(id + _sizeX + 1);
                }
            }
            else
            // Right Middle Tiles
            if (id % _sizeX == _sizeX - 1 && id > _sizeX && id <= _sizeX * _sizeY - _sizeX)
            {
                newID = CheckRightMid(id);
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(newID)];
                if (newID == 0)
                {
                    OpenField(id - _sizeX);
                    OpenField(id - _sizeX - 1);
                    OpenField(id - 1);
                    OpenField(id + _sizeX);
                    OpenField(id + _sizeX - 1);
                }
            }
            else
            // Bottom Left Corner
            if (id == _sizeX * _sizeY - _sizeX)
            {
                newID = CheckBottomLeftCorner(id);
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(newID)];
                if (newID == 0)
                {
                    OpenField(id - _sizeX);
                    OpenField(id - _sizeX + 1);
                    OpenField(id + 1);
                }
            }
            else
            // Bottom Middle Tiles
            if (id % _sizeX > 0 && id % _sizeX < _sizeX - 1 && id > _sizeX * _sizeY - _sizeX)
            {
                newID = CheckBottomMid(id);
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(newID)];
                if (newID == 0)
                {
                    OpenField(id - _sizeX - 1);
                    OpenField(id - _sizeX);
                    OpenField(id - _sizeX + 1);
                    OpenField(id - 1);
                    OpenField(id + 1);
                }
            }
            else
            // Bottom Right Corner
            if (id == _sizeX * _sizeY - 1)
            {
                newID = CheckBottomRightCorner(id);
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(newID)];
                if (newID == 0)
                {
                    OpenField(id - _sizeX - 1);
                    OpenField(id - _sizeX);
                    OpenField(id - 1);
                }
            }
            else
            // Center Tiles
            {
                newID = CheckCenter(id);
                (_tiles[id].DataContext as TileViewModel).TileStateImage = BitmapSources[GetStateImage(newID)];
                if (newID == 0)
                {
                    OpenField(id - _sizeX - 1);
                    OpenField(id - _sizeX);
                    OpenField(id - _sizeX + 1);
                    OpenField(id - 1);
                    OpenField(id + 1);
                    OpenField(id + _sizeX - 1);
                    OpenField(id + _sizeX);
                    OpenField(id + _sizeX + 1);
                }
            }
        }

        /// <summary>
        /// Returns Number of Mines surrounding Left Corner of Grid
        /// </summary>
        /// <param name="id">Id of the Tile</param>
        /// <returns>Number of Mines around</returns>
        private int CheckUpperLeftCorner(int id)
        {
            var counter = 0;
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
            foreach (Tile tile in _tiles)
            {
                if ((tile.DataContext as TileViewModel).HasMine == false && (tile.DataContext as TileViewModel).TileStateImage != BitmapSources[StateImages.None])
                {
                    counter++;
                }
            }
            if (counter == _sizeX * _sizeY - NumberOfMines)
            {
                var result = MessageBox.Show("You Win!!! Wanna Restart?", "Awesome Job", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes: GameOver = false; _mineData = new MineData(NumberOfMines, FlaggedMinesCounter, _sizeX, _sizeY, GameOver); UpdateObservers(); break;
                    case MessageBoxResult.No:; break;
                    default:; break;
                }
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

        #endregion
    }

    /// <summary>
    /// Difficulties which can be choosen from
    /// </summary>
    public enum Difficulty
    {
        /// <summary>
        /// 8 X 8; 10 Mines
        /// </summary>
        Easy,
        /// <summary>
        /// 16 X 16; 40 Mines
        /// </summary>
        Normal,
        /// <summary>
        /// 16 X 30; 99 Mines
        /// </summary>
        Hard
    }

    /// <summary>
    /// The two GameModes
    /// </summary>
    public enum GameMode
    {
        /// <summary>
        /// Game will be built dependend on diffiulty choosen.
        /// </summary>
        Standard,
        /// <summary>
        /// Size and Number of Mines can be customized. Difficulty has no effect.
        /// </summary>
        Custom
    }

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

    internal class Unsubscriber<MineData> : IDisposable
    {
        private readonly List<IObserver<MineData>> _observers;
        private readonly IObserver<MineData> _observer;

        internal Unsubscriber(List<IObserver<MineData>> observers, IObserver<MineData> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}
