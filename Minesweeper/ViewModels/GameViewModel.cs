using Minesweeper.Commands;
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
        private Boolean _isNotRunning = true;
        private ObservableCollection<UserControl> _tiles;
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

        public Boolean IsNotRunning
        {
            get => _isNotRunning;
            set => SetProperty(ref _isNotRunning, value);
        }

        public ObservableCollection<UserControl> Tiles
        {
            get => _tiles;
            set => SetProperty(ref _tiles, value);
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
            Tiles = new ObservableCollection<UserControl>();
            var totalSize = SizeX * SizeY;
            for (int i = 0; i < totalSize; i++)
            {
                Tiles.Add(new Tile());
            }
        }

        private Boolean CanStartGame => true;
        #endregion
    }
}
