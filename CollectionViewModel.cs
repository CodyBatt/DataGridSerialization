using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;
using ReactiveUI.Xaml;

namespace DataGridSerialization
{
    class CollectionViewModel : ReactiveObject
    {

        public CollectionViewModel()
        {
            Items.Add(new ItemViewModel{Age=20, Name="Bob", Guid=Guid.NewGuid()});
            Items.Add(new ItemViewModel { Age = 30, Name = "Tom", Guid = Guid.NewGuid() });
            Items.Add(new ItemViewModel { Age = 40, Name = "Joe", Guid = Guid.NewGuid() });
        }

        #region Property Items

        private ObservableCollection<ItemViewModel> _pItems = new ObservableCollection<ItemViewModel>();

        public ObservableCollection<ItemViewModel> Items
        {
            get { return _pItems; }
            set { this.RaiseAndSetIfChanged(ref _pItems, value); }
        }

        #endregion

        #region Property SelectedItem

        private ItemViewModel _pSelectedItem = new ItemViewModel();

        public ItemViewModel SelectedItem
        {
            get { return _pSelectedItem; }
            set { this.RaiseAndSetIfChanged(ref _pSelectedItem, value); }
        }

        #endregion

        #region Command: NewItemCommand

        private IReactiveCommand _pNewItemCommand = null;

        public IReactiveCommand NewItemCommand
        {
            get
            {
                if (_pNewItemCommand == null)
                {
                    _pNewItemCommand = new ReactiveCommand( /* Observable for CanExecute Here */);
                    _pNewItemCommand.Subscribe((param) => Items.Add(new ItemViewModel{Age=50, Name="Gert", Guid=Guid.NewGuid()}));
                }
                return _pNewItemCommand;
            }
        }

        #endregion

        #region Command: RemoveCommand

        private IReactiveCommand _pRemoveCommand = null;

        public IReactiveCommand RemoveCommand
        {
            get
            {
                if (_pRemoveCommand == null)
                {
                    _pRemoveCommand = new ReactiveCommand( this.WhenAny(x=>x.SelectedItem, x=> x.Value != null));
                    _pRemoveCommand.Subscribe((param) =>
                        {
                            if (SelectedItem == null) return;
                            Items.Remove(SelectedItem);
                            SelectedItem = null;
                        });
                }
                return _pRemoveCommand;
            }
        }

        #endregion

    }
}
