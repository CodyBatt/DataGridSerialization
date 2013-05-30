using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace DataGridSerialization
{
    class ItemViewModel : ReactiveObject
    {
        #region Property Name

        private string _pName = default(string);

        public string Name
        {
            get { return _pName; }
            set { this.RaiseAndSetIfChanged(ref _pName, value); }
        }

        #endregion

        #region Property Age

        private int _pAge = default(int);

        public int Age
        {
            get { return _pAge; }
            set { this.RaiseAndSetIfChanged(ref _pAge, value); }
        }

        #endregion

        #region Property Guid

        private Guid _pGuid = default(Guid);

        public Guid Guid
        {
            get { return _pGuid; }
            set { this.RaiseAndSetIfChanged(ref _pGuid, value); }
        }

        #endregion
    }

   
}
