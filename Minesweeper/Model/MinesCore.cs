using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Model
{
    class MinesCore
    {
        private int _numberOfTiles;
        private int _numberOfMines;

        public MinesCore(int numberOfTiles, ref List<bool> minesList)
        {
            _numberOfTiles = numberOfTiles;
            _numberOfMines = numberOfTiles / 2;
            populateMinesField(ref minesList);
        }
        public MinesCore(int numberOfTiles, int numberOfMines)
        {
            _numberOfTiles = numberOfTiles;
            _numberOfMines = numberOfMines;
        }

        private void populateMinesField(ref List<bool> minesList)
        {
            if ( minesList == null )
            {
                minesList = new List<bool>();
            }
            for ( int  i = 0; i < _numberOfTiles; i++ )
            {

            }
        }
    }
}
