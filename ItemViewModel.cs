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

        #region Property Custom1

        private string _pCustom1 = default(string);

        public string Custom1
        {
            get { return _pCustom1; }
            set { this.RaiseAndSetIfChanged(ref _pCustom1, value); }
        }

        #endregion

        #region Property Custom2

        private string _pCustom2 = default(string);

        public string Custom2
        {
            get { return _pCustom2; }
            set { this.RaiseAndSetIfChanged(ref _pCustom2, value); }
        }

        #endregion

        #region Property Custom3

        private string _pCustom3 = default(string);

        public string Custom3
        {
            get { return _pCustom3; }
            set { this.RaiseAndSetIfChanged(ref _pCustom3, value); }
        }

        #endregion

        #region Property Custom4

        private string _pCustom4 = default(string);

        public string Custom4
        {
            get { return _pCustom4; }
            set { this.RaiseAndSetIfChanged(ref _pCustom4, value); }
        }

        #endregion

        #region Property Custom5

        private string _pCustom5 = default(string);

        public string Custom5
        {
            get { return _pCustom5; }
            set { this.RaiseAndSetIfChanged(ref _pCustom5, value); }
        }

        #endregion

        #region Property Custom6

        private string _pCustom6 = default(string);

        public string Custom6
        {
            get { return _pCustom6; }
            set { this.RaiseAndSetIfChanged(ref _pCustom6, value); }
        }

        #endregion

        #region Property Custom7

        private string _pCustom7 = default(string);

        public string Custom7
        {
            get { return _pCustom7; }
            set { this.RaiseAndSetIfChanged(ref _pCustom7, value); }
        }

        #endregion

        #region Property Custom8

        private string _pCustom8 = default(string);

        public string Custom8
        {
            get { return _pCustom8; }
            set { this.RaiseAndSetIfChanged(ref _pCustom8, value); }
        }

        #endregion

        #region Property Custom9

        private string _pCustom9 = default(string);

        public string Custom9
        {
            get { return _pCustom9; }
            set { this.RaiseAndSetIfChanged(ref _pCustom9, value); }
        }

        #endregion

        #region Property Custom10

        private string _pCustom10 = default(string);

        public string Custom10
        {
            get { return _pCustom10; }
            set { this.RaiseAndSetIfChanged(ref _pCustom10, value); }
        }

        #endregion


    }

   
}
