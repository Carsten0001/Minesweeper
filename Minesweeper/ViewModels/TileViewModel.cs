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
    public class TileViewModel: BindableBase
    {
        #region Fields
        private BitmapSource _tileStateImage;
        private bool _hasBomb = false; 

        #endregion

        #region Properties
        public int Id { get; set; }

        public BitmapSource TileStateImage
        {
            get => _tileStateImage;
            set => SetProperty(ref _tileStateImage, value);  
        }

        public bool HasMine
        {
            get => _hasBomb;
            set => SetProperty(ref _hasBomb, value);
        }

        #endregion

        #region Constructor
        public TileViewModel()
        {
            TileStateImage = MinesCore.Instance.BitmapSources[StateImages.None];
        }
        #endregion

        #region Commanding
        private ICommand _toggleButtonStateCommand;
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
        #endregion
        #region Methods
        private bool CanToggleButtonState => true;
        private void DoToggleButtonState()
        {
            Console.WriteLine(HasMine);
            if (HasMine)
            {
                TileStateImage = MinesCore.Instance.BitmapSources[StateImages.Explosion];
                MinesCore.Instance.GameLost();
            }
            else
            {
                MinesCore.Instance.OpenField(Id);
            }
        }
        #endregion
    }
}
