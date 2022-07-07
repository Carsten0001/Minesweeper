using Minesweeper.Commands;
using Minesweeper.Enums;
using Minesweeper.Model;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Minesweeper.ViewModels
{
    /// <summary>
    /// ViewModel of a Tile
    /// </summary>
    public class TileViewModel : BindableBase
    {
        #region Fields

        private byte[]_tileStateImage;
        private bool _hasBomb;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The unique identifier of the tile
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The image displayed on the tile
        /// </summary>
        public byte[] TileStateImage
        {
            get => _tileStateImage;
            set => SetProperty(ref _tileStateImage, value);
        }

        /// <summary>
        /// Indicates if the Tile has a Mine
        /// </summary>
        public bool HasMine
        {
            get => _hasBomb;
            set => SetProperty(ref _hasBomb, value);
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Gets the Initial Image for the Tiles
        /// </summary>
        public TileViewModel()
        {
            TileStateImage = MinesCore.Instance.Images[StateImages.None];
        }

        #endregion Constructor

        #region Commanding

        private ICommand _toggleButtonStateCommand;
        private ICommand _markTile;

        /// <summary>
        /// Calls <see cref="DoToggleButtonState"/> when a tile is left clicked
        /// </summary>
        public ICommand ToggleButtonStateCommand
        {
            get
            {
                if (_toggleButtonStateCommand == null)
                {
                    _toggleButtonStateCommand = new RelayCommand(
                        p => CanToggleButtonState,
                        p => DoToggleButtonState());
                }
                return _toggleButtonStateCommand;
            }
        }

        /// <summary>
        /// Calls <see cref="DoMarkTile"/> if a Tile is right clicked
        /// </summary>
        public ICommand MarkTile
        {
            get
            {
                if (_markTile == null)
                {
                    _markTile = new RelayCommand(
                        p => CanMarkTile,
                        p => DoMarkTile());
                }
                return _markTile;
            }
        }

        #endregion Commanding

        #region Methods

        private static bool CanToggleButtonState => !MinesCore.Instance.GameOver;

        private void DoToggleButtonState()
        {
            if (HasMine)
            {
                MinesCore.Instance.Explosion();
                TileStateImage = MinesCore.Instance.Images[StateImages.Explosion];
                MinesCore.Instance.GameLost();
                
            }
            else
            {
                MinesCore.Instance.RevealFieldAndCheckForWin(Id);
            }
        }

        private static bool CanMarkTile => true;

        private void DoMarkTile()
        {
            if (TileStateImage == MinesCore.Instance.Images[StateImages.None])
            {
                TileStateImage = MinesCore.Instance.Images[StateImages.Flag];
                MinesCore.Instance.FlaggedMinesCounter++;
            }
            else if (TileStateImage == MinesCore.Instance.Images[StateImages.Flag])
            {
                TileStateImage = MinesCore.Instance.Images[StateImages.Questionmark];
                MinesCore.Instance.FlaggedMinesCounter--;
            }
            else if (TileStateImage == MinesCore.Instance.Images[StateImages.Questionmark])
            {
                TileStateImage = MinesCore.Instance.Images[StateImages.None];
            }
        }

        #endregion Methods
    }
}