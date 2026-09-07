using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Components.PickUpBehavior
{
    public interface IPickupBehavior
    {
        void OnPickup(GameObject player);
        void OnDrop();
        void OnUse();
        void OnPrimary();
        void OnSecondary();
    }
}
