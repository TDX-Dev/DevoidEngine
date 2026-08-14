using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elemental
{
    public class SelectionContext
    {
        public GameObject? SelectedGameObject { get; set; }
        public object? SelectedAsset { get; set; }

        public void ClearSelection()
        {
            SelectedGameObject = null;
            SelectedAsset = null;
        }
    }
}
