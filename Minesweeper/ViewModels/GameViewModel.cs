using Minesweeper.Commands;
using Minesweeper.Enums;
using Minesweeper.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Minesweeper.ViewModels
{
    /// <summary>
    /// The ViewModel auf the MainGameView
    /// </summary>
    internal class GameViewModel : BindableBase, IObserver<MineData>
    {
        #region Fields

        private double _mainViewMaxWidth = System.Windows.SystemParameters.MaximizedPrimaryScreenWidth;
        private double _mainViewMaxHeight = System.Windows.SystemParameters.MaximizedPrimaryScreenHeight;
        private int _sizeX;
        private int _sizeY;
        private int _numberOfMines = 0;
        private int _flaggedMinesCounter = 0;
        private bool _isNotRunning = true;
        private bool _isPlaygroundUnlocked = true;
        private ObservableCollection<Tile> _tiles;
        private ObservableCollection<GameMode> _gameModes;
        private GameMode _selectedGameMode = GameMode.Standard;
        private Difficulty _selectedDifficulty = Difficulty.Normal;
        private IDisposable _cancellation;

        #endregion Fields

        #region Properties

        public string StartButtonContent { get; set; } = "Start";

        public double MainViewMaxHeight
        {
            get => _mainViewMaxHeight;
            set => SetProperty(ref _mainViewMaxHeight, value);
        }

        public double MainViewMaxWidth
        {
            get => _mainViewMaxWidth;
            set => SetProperty(ref _mainViewMaxWidth, value);
        }

        public int SizeX
        {
            get => _sizeX;
            set => SetProperty(ref _sizeX, value);
        }

        public int SizeY
        {
            get => _sizeY;
            set => SetProperty(ref _sizeY, value);
        }

        public int NumberOfMines
        {
            get => _numberOfMines;
            set => SetProperty(ref _numberOfMines, value);
        }

        public int FlaggedMinesCounter
        {
            get => _flaggedMinesCounter;
            set => SetProperty(ref _flaggedMinesCounter, value);
        }

        public bool IsNotRunning
        {
            get => _isNotRunning;
            set => SetProperty(ref _isNotRunning, value);
        }

        public bool IsPlaygroundUnlocked
        {
            get => _isPlaygroundUnlocked;
            set => SetProperty(ref _isPlaygroundUnlocked, value);
        }

        public ObservableCollection<Tile> Tiles
        {
            get => _tiles;
            set => SetProperty(ref _tiles, value);
        }

        public ObservableCollection<GameMode> GameModes
        {
            get => _gameModes;
            set => SetProperty(ref _gameModes, value);
        }

        public GameMode SelectedGameMode
        {
            get => _selectedGameMode;
            set => SetProperty(ref _selectedGameMode, value);
        }

        public Difficulty SelectedDifficulty
        {
            get => _selectedDifficulty;
            set => SetProperty(ref _selectedDifficulty, value, () => MinesCore.Instance.ChangeSize(SelectedDifficulty));
        }

        #endregion Properties

        #region Commanding

        private ICommand _startGame;

        public ICommand StartGame
        {
            get
            {
                if (_startGame == null)
                {
                    _startGame = new RelayCommand(
                        p => CanStartGame,
                        p => DoStartGame());
                }
                return _startGame;
            }
        }

        #endregion Commanding

        #region Methods

        public GameViewModel()
        {
            _tiles = new ObservableCollection<Tile>();
            MinesCore.Instance.InitGame(ref _tiles);
            Subscribe(MinesCore.Instance);
        }

        /// <summary>
        /// Starts the Game. Calls <see cref="StartGame()"/>.
        /// Calls <see cref="Subscribe(MinesCore)"/>
        /// Calls <see cref="FillTilesCollection"/>
        /// </summary>
        protected void DoStartGame()
        {
            if (_cancellation != null)
            {
                Unsubscribe();
            }

            MinesCore.Instance.StartGame(SelectedGameMode, SelectedDifficulty, SizeX, SizeY, NumberOfMines, ref _tiles);
            Subscribe(MinesCore.Instance);
            FillTilesCollection();

            IsNotRunning = false;
            _isPlaygroundUnlocked = true;
        }

        /// <summary>
        /// Fills the TilesCollection random with bombs
        /// </summary>
        private void FillTilesCollection()
        {
            var random = new Random();
            var MinesCounter = NumberOfMines;
            var totalSize = SizeX * SizeY;

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
        /// Clears <see cref="Tiles"/>, sets <see cref="_isNotRunning"/> to true
        /// </summary>
        private void Reset()
        {
            Tiles.Clear();
            IsNotRunning = true;
        }

        /// <summary>
        /// Subscribes to <paramref name="minesCore"/>
        /// </summary>
        /// <param name="minesCore">The Minescore Instance to subscribe to</param>
        public virtual void Subscribe(MinesCore minesCore)
        {
            _cancellation = minesCore.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            _cancellation.Dispose();
        }

        /// <summary>
        /// Get <see cref="MineData"/> and update the Properties of GameViewModel
        /// If Minedata's GameOver Property is true the game will be resetted by calling <see cref="Reset"/>
        /// </summary>
        /// <param name="value">Newest MineData object</param>
        public virtual void OnNext(MineData value)
        {
            NumberOfMines = value.NumberOfMines;
            FlaggedMinesCounter = value.RemainingMines;
            IsNotRunning = value.GameOver;
            SizeX = value.SizeX;
            SizeY = value.SizeY;
            if (value.GameOver)
            {
                _isPlaygroundUnlocked = true;
                Reset();
            }
        }

        public virtual void OnError(Exception error)
        {
            // No implementation
        }

        public virtual void OnCompleted()
        {
            throw new NotImplementedException();
        }

        private bool CanStartGame => SizeX > 0 && SizeX <= 30 && SizeY > 0 && SizeY <= 28;

        #endregion Methods
    }
}