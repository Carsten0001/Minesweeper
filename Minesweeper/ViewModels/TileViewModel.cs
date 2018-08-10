using Minesweeper.Commands;
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

        #endregion

        #region Properties
        public BitmapSource TileStateImage
        {
            get => _tileStateImage;
            set => SetProperty(ref _tileStateImage, value);  
        }
        #endregion

        #region Constructor
        public TileViewModel()
        {
            TileStateImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Resources.None.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(Resources.None.Width, Resources.None.Height));
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
        protected void DoToggleButtonState()
        {
            
        }
        #endregion
    }
}
