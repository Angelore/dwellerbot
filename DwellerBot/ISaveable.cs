using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwellerBot
{
    public interface ISaveable
    {
        void SaveState();

        void LoadState();
    }
}
