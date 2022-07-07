using Minesweeper.Enums;
using Minesweeper.Manager;
using Minesweeper.Properties;
using Minesweeper.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace Minesweeper.Model
{
    /// <summary>
    /// The Heart of the Game
    /// Mediator for the ViewModels
    /// Holds all the values, which are nessecary for the game
    /// </summary>
    public sealed class MinesCore : BindableBase, IObservable<MineData>
    {
        #region Fields

        private static List<IObserver<MineData>> _observers;
        private static MineData _mineData;
        private static readonly Lazy<MinesCore> _lazy = new(() => new MinesCore());
        private readonly SoundManager _soundManager;
        private ObservableCollection<Tile> _tiles;
        private int _sizeX;
        private int _sizeY;
        private int _flaggedMinesCounter;

        #endregion Fields

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
        public int FlaggedMinesCounter
        {
            get
            {
                return _flaggedMinesCounter;
            }
            set
            {
                _flaggedMinesCounter = value;
                UpdateObservers();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<Tile> Tiles
        {
            get => _tiles ??= new ObservableCollection<Tile>();
            set => SetProperty(ref _tiles, value);
        }

        /// <summary>
        /// A Collection of all the Images which can be displayed on a Tile
        /// </summary>
        public Dictionary<StateImages, byte[]> Images { get; private set; }

        #endregion Properties

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
            Images = Resources.ResourceManager
                                       .GetResourceSet(CultureInfo.InvariantCulture, true, true)
                                       .Cast<DictionaryEntry>()
                                       .Where(x => x.Value.GetType() == typeof(byte[]))
                                       .ToDictionary(x => (StateImages)Enum.Parse(typeof(StateImages), x.Key.ToString()), x => x.Value as byte[]);

            _soundManager = new SoundManager();
        }


        #endregion Constructors

        #region Methods

        /// <summary>
        /// Inits the Game with default properties.
        /// </summary>
        internal void InitGame()
        {
            SetStandardGameProperties(Difficulty.Normal);
            UpdateObservers();
        }

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
        internal void StartGame(GameMode gameMode, Difficulty difficulty, int sizeX, int sizeY, int numberOfMines)
        {
            if (gameMode == GameMode.Standard)
            {
                SetStandardGameProperties(difficulty);
            }
            else
            {
                _sizeX = sizeX;
                _sizeY = sizeY;
                NumberOfMines = numberOfMines;
            }

            GameOver = false;
            UpdateObservers();
        }


        /// <summary>
        /// Fills the TilesCollection random with bombs
        /// </summary>
        public void FillTilesCollection()
        {
            var random = new Random();
            var MinesCounter = NumberOfMines;
            var totalSize = _sizeX * _sizeY;

            for (int i = 0; i < totalSize; i++)
            {
                var tile = new Tile();
                (tile.DataContext as TileViewModel).Id = i;
                Tiles.Add(tile);
            }
            while (MinesCounter > 0)
            {
                var randomNumber = random.Next(totalSize);
                if (!(Tiles[randomNumber].DataContext as TileViewModel).HasMine)
                {
                    (Tiles[randomNumber].DataContext as TileViewModel).HasMine = true;
                    MinesCounter--;
                }
            }
        }

        /// <summary>
        /// Sets the size of the game field and the number of mines based on difficulty level.
        /// </summary>
        /// <param name="difficulty">The defined difficulty</param>
        private void SetStandardGameProperties(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy: _sizeX = _sizeY = 8; NumberOfMines = 10; break;
                case Difficulty.Normal: _sizeX = _sizeY = 16; NumberOfMines = 40; break;
                case Difficulty.Hard: _sizeX = 30; _sizeY = 16; NumberOfMines = 99; break;
            }
        }

        /// <summary>
        /// Informs the MineCore that the User has revealed a mine.
        /// </summary>
        internal void GameLost()
        {
            _soundManager.PlayExplosionSound();
            GameOver = true;
            foreach (Tile tile in Tiles)
            {
                if ((tile.DataContext as TileViewModel).HasMine && (tile.DataContext as TileViewModel).TileStateImage == Images[StateImages.None])
                    (tile.DataContext as TileViewModel).TileStateImage = Images[StateImages.Mine];
            }
            var result = MessageBox.Show(Resources.Fail_Dialog_Text, Resources.Fail_Dialog_Title, MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes: 
                    Tiles.Clear();
                    UpdateObservers(); 
                    break;
                case MessageBoxResult.No: /* Do nothing */; break;
            }
        }

        internal void ChangeSize(Difficulty selectedDifficulty)
        {
            SetStandardGameProperties(selectedDifficulty);
            UpdateObservers();
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
        internal void RevealFieldAndCheckForWin(int id)
        {
            _soundManager.PlayRevealTileSound();
            OpenField(id);
            CheckIfWon();
        }

        /// <summary>
        /// Changes the Button Image of the button specified by id
        /// </summary>
        /// <param name="id">id of the button that should be revealed</param>
        private void OpenField(int id)
        {
            if ((Tiles[id].DataContext as TileViewModel).TileStateImage == Images[StateImages.Null])
            {
                return;
            }

            int newID;
            // Upper Left Corner
            if (id == 0)
            {
                newID = CheckUpperLeftCorner(id);
                (Tiles[id].DataContext as TileViewModel).TileStateImage = Images[GetStateImage(newID)];
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
                (Tiles[id].DataContext as TileViewModel).TileStateImage = Images[GetStateImage(newID)];
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
                (Tiles[id].DataContext as TileViewModel).TileStateImage = Images[GetStateImage(newID)];
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
                (Tiles[id].DataContext as TileViewModel).TileStateImage = Images[GetStateImage(newID)];
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
                (Tiles[id].DataContext as TileViewModel).TileStateImage = Images[GetStateImage(newID)];
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
                (Tiles[id].DataContext as TileViewModel).TileStateImage = Images[GetStateImage(newID)];
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
                (Tiles[id].DataContext as TileViewModel).TileStateImage = Images[GetStateImage(newID)];
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
                (Tiles[id].DataContext as TileViewModel).TileStateImage = Images[GetStateImage(newID)];
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
                (Tiles[id].DataContext as TileViewModel).TileStateImage = Images[GetStateImage(newID)];
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
            if ((Tiles[id + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + _sizeX + 1].DataContext as TileViewModel).HasMine)
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
            if ((Tiles[id - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + _sizeX + 1].DataContext as TileViewModel).HasMine)
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
            if ((Tiles[id - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + _sizeX].DataContext as TileViewModel).HasMine)
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
            if ((Tiles[id - _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id - _sizeX + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + _sizeX + 1].DataContext as TileViewModel).HasMine)
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
            if ((Tiles[id - _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id - _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + _sizeX - 1].DataContext as TileViewModel).HasMine)
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
            if ((Tiles[id - _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id - _sizeX + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + 1].DataContext as TileViewModel).HasMine)
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
            if ((Tiles[id - _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id - _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id - _sizeX + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + 1].DataContext as TileViewModel).HasMine)
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
            if ((Tiles[id - _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id - _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id - 1].DataContext as TileViewModel).HasMine)
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
            if ((Tiles[id - _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id - _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id - _sizeX + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + _sizeX - 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + _sizeX].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            if ((Tiles[id + _sizeX + 1].DataContext as TileViewModel).HasMine)
            {
                counter++;
            }
            return counter;
        }

        private void CheckIfWon()
        {
            var counter = 0;
            foreach (Tile tile in Tiles)
            {
                if ((tile.DataContext as TileViewModel).HasMine == false && (tile.DataContext as TileViewModel).TileStateImage != Images[StateImages.None])
                {
                    counter++;
                }
            }
            if (counter == _sizeX * _sizeY - NumberOfMines)
            {
                var result = MessageBox.Show(Resources.Win_Dialog_Text, Resources.Win_Dialog_Title, MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes: GameOver = true; UpdateObservers(); break;
                    case MessageBoxResult.No:; break;
                    default:; break;
                }
            }
        }

        private static StateImages GetStateImage(int number)
        {
            return number switch
            {
                0 => StateImages.Null,
                1 => StateImages.One,
                2 => StateImages.Two,
                3 => StateImages.Three,
                4 => StateImages.Four,
                5 => StateImages.Five,
                6 => StateImages.Six,
                7 => StateImages.Seven,
                8 => StateImages.Eight,
                _ => StateImages.None,
            };
        }

        #endregion Methods
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