using Minesweeper.Commands;
using Minesweeper.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minesweeper.ViewModels
{
    class GameViewModel : BindableBase
    {
        #region Fields
        private int _sizeX;
        private int _sizeY;
        private int _numberOfMines = 0;
        private bool _isNotRunning = true;
        private ObservableCollection<Tile> _tiles;
        private ObservableCollection<GameMode> _gameModes;
        private GameMode _selectedGameMode = GameMode.Standard;
        private Difficulty _selectedDifficulty = Difficulty.Normal;
        #endregion

        #region Properties
        public string StartButtonContent { get; set; } = "Start";
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

        public bool IsNotRunning
        {
            get => _isNotRunning;
            set => SetProperty(ref _isNotRunning, value);
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
            set => SetProperty(ref _selectedDifficulty, value);
        }
        #endregion

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
        #endregion

        #region Methods
        protected void DoStartGame()
        {
            Tiles = new ObservableCollection<Tile>();
            var totalSize = SizeX * SizeY;
            NumberOfMines = MinesCore.Instance.StartGame(SelectedGameMode, SelectedDifficulty, SizeX, SizeY, NumberOfMines, ref _tiles);
            var random = new Random();
            var MinesCounter = MinesCore.Instance.NumberOfMines;
            while(MinesCounter > 0)
            {
                for (int i = 0; i < totalSize; i++)
                {
                    if(Tiles.Count < totalSize)
                    {
                        var tile = new Tile();
                        (tile.DataContext as TileViewModel).Id = i;

                        if(random.Next(totalSize) == 1 && MinesCounter > 0)
                        {
                            (tile.DataContext as TileViewModel).HasMine = true;
                            Tiles.Add(tile);
                            MinesCounter--;
                        }
                        else
                        {
                            Tiles.Add(tile);
                        }
                    }
                    else
                    {
                        if(random.Next(2) == 1 && MinesCounter > 0 && ((Tiles[i] as Tile).DataContext as TileViewModel).HasMine == false)
                        {
                            ((Tiles[i] as Tile).DataContext as TileViewModel).HasMine = true;
                            MinesCounter--;
                        }
                    }
                }
            }
            IsNotRunning = false;
        }

        private Boolean CanStartGame => true;
        #endregion
    }
}
