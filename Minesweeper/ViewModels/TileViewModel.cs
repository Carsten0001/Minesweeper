using Minesweeper.Commands;
using Minesweeper.Model;
using Minesweeper.Properties;
using System;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Minesweeper.ViewModels
{
    /// <summary>
    /// ViewModel of a Tile
    /// </summary>
    public class TileViewModel: BindableBase
    {
        #region Fields
        private BitmapSource _tileStateImage;
        private bool _hasBomb = false; 

        #endregion

        #region Properties
        /// <summary>
        /// The unique identifier of the tile
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The image displayed on the tile
        /// </summary>
        public BitmapSource TileStateImage
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

        #endregion

        #region Constructor
        /// <summary>
        /// Gets the Initial Image for the Tiles
        /// </summary>
        public TileViewModel()
        {
            TileStateImage = MinesCore.Instance.BitmapSources[StateImages.None];
        }
        #endregion

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
                if(_markTile == null)
                {
                    _markTile = new RelayCommand(
                        p => CanMarkTile,
                        p => DoMarkTile());
                }
                return _markTile;
            }
        }
        #endregion

        #region Methods
        private bool CanToggleButtonState => true;
        private void DoToggleButtonState()
        {
            if (HasMine)
            {
                TileStateImage = MinesCore.Instance.BitmapSources[StateImages.Explosion];
                MinesCore.Instance.GameLost();
            }
            else
            {
                MinesCore.Instance.RevealFieldAndCheckForWin(Id);
            }
        }

        private bool CanMarkTile => true;
        private void DoMarkTile()
        {
            if (TileStateImage == MinesCore.Instance.BitmapSources[StateImages.None])
            {
                TileStateImage = MinesCore.Instance.BitmapSources[StateImages.Flag];
                MinesCore.Instance.FlaggedMinesCounter--;
            }
            else if (TileStateImage == MinesCore.Instance.BitmapSources[StateImages.Flag])
            {
                TileStateImage = MinesCore.Instance.BitmapSources[StateImages.Questionmark];
                MinesCore.Instance.FlaggedMinesCounter++;
            }
            else if (TileStateImage == MinesCore.Instance.BitmapSources[StateImages.Questionmark])
            {
                TileStateImage = MinesCore.Instance.BitmapSources[StateImages.None];
            }
        }
        #endregion
    }
}
