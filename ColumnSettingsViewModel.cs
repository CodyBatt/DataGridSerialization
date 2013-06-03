using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ReactiveUI;

namespace DataGridSerialization
{
    public class ColumnSettingsViewModel : ReactiveObject
    {
        public string Id;
        public double? Width;

        #region Property Required
        private bool _pRequired = default(bool);
        public bool Required
        {
            get { return _pRequired; }
            set { this.RaiseAndSetIfChanged(ref _pRequired, value); }
        }
        #endregion
        #region Property DisplayName

        private string _pDisplayName = default(string);

        public string DisplayName
        {
            get { return _pDisplayName; }
            set { this.RaiseAndSetIfChanged(ref _pDisplayName, value); }
        }

        #endregion
        #region Property Order
        private int _pOrder = default(int);
        public int Order
        {
            get { return _pOrder; }
            set { this.RaiseAndSetIfChanged(ref _pOrder, value); }
        }
        #endregion
        #region Property Visible
        private bool _pVisible;
        public bool Visible
        {
            get { return _pVisible; }
            set { this.RaiseAndSetIfChanged(ref _pVisible, value); }
        }
        #endregion

        private ColumnSettingsViewModel()
        {}

        public ColumnSettingsViewModel(ColumnSettings cs)
        {
            Id = cs.Id;
            DisplayName = cs.DisplayName;
            Order = cs.Order;
            Width = cs.Width;
            Visible = cs.Visible;
            Required = false;
        }

        public void ApplyColumnSettings(DataGridColumn col)
        {
            col.Width = ConvertToDataGridLength(Width);
            col.Visibility = Visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ApplyColumnOrdering(int colcount, DataGridColumn col)
        {
            if (Order >= 0 && Order < colcount)
                col.DisplayIndex = Order;
        }

        private static DataGridLength ConvertToDataGridLength(double? width)
        {
            return width == null ? DataGridLength.SizeToCells : new DataGridLength(width.Value);
        }

        public ColumnSettingsViewModel Copy()
        {
            return new ColumnSettingsViewModel
                {
                    Order = Order,
                    Visible = Visible,
                    DisplayName = DisplayName,
                    Id = Id,
                    Width = Width,
                    Required = Required
                };
        }
    }

    public class GridSettingsViewModel : ReactiveObject
    {
        public GridSettingsViewModel(ColumnManager cm)
        {
            Settings = new ReactiveCollection<ColumnSettingsViewModel>();
            SettingsView = (CollectionView)CollectionViewSource.GetDefaultView(Settings);
            SettingsView.SortDescriptions.Add(new SortDescription {PropertyName = "Order", Direction = ListSortDirection.Ascending});

            foreach (var csvm in cm.ColumnSettings)
                Settings.Add(csvm.Copy());
        }

        public void ApplySettings(ColumnManager cm)
        {
            foreach (var col in cm.ColumnSettings)
            {
                var settings = Settings.FirstOrDefault(x => x.Id == col.Id);
                if (settings == null) continue;
                col.Order = settings.Order;
                col.Visible = settings.Visible;
            }
            
        }

        #region Property Settings

        private ReactiveCollection<ColumnSettingsViewModel> _pSettings = default(ReactiveCollection<ColumnSettingsViewModel>);

        public ReactiveCollection<ColumnSettingsViewModel> Settings
        {
            get { return _pSettings; }
            set { this.RaiseAndSetIfChanged(ref _pSettings, value); }
        }

        #endregion
        #region Property SettingsView

        private CollectionView _pSettingsView = default(CollectionView);

        public CollectionView SettingsView
        {
            get { return _pSettingsView; }
            set { this.RaiseAndSetIfChanged(ref _pSettingsView, value); }
        }

        #endregion
        
        #region Command: MoveUp
        private ICommand _pMoveUp;
        public ICommand MoveUp
        {
            get
            {
                return _pMoveUp ??
                       (_pMoveUp = new RelayCommand(
                        (param) =>
                        {
                            // MoveUp Executed
                            var vm = param as ColumnSettingsViewModel;
                            if (vm == null) return;
                            SwapColumnOrders(vm, vm.Order - 1);
                        },
                        (param) =>
                        {
                            // MoveUp CanExecute?  
                            var vm = param as ColumnSettingsViewModel;
                            if (vm == null) return false;
                            return vm.Order > 0;
                            
                        }));
            }
        }
        #endregion
        #region Command: MoveDown
        private ICommand _pMoveDown;
        public ICommand MoveDown
        {
            get
            {
                return _pMoveDown ??
                       (_pMoveDown = new RelayCommand(
                        (param) =>
                        {
                            // MoveDown Executed
                            var vm = param as ColumnSettingsViewModel;
                            if (vm == null) return;
                            SwapColumnOrders(vm, vm.Order + 1);
                        },
                        (param) =>
                        {
                            // MoveDown CanExecute?  
                            var vm = param as ColumnSettingsViewModel;
                            if (vm == null) return false;
                            return vm.Order < Settings.Count - 1;
                        }));
            }
        }
        #endregion
        #region Command: ShowColumn
        private ICommand _pShowColumn;
        public ICommand ShowColumn
        {
            get
            {
                return _pShowColumn ??
                       (_pShowColumn = new RelayCommand(
                        (param) =>
                        {
                            // ShowColumn Executed
                            var vm = param as ColumnSettingsViewModel;
                            if (vm == null) return;
                            vm.Visible = true;
                        },
                        (param) =>
                        {
                            // ShowColumn CanExecute?  
                            var vm = param as ColumnSettingsViewModel;
                            if (vm == null) return false;
                            return !vm.Required && !vm.Visible;
                        }));
            }
        }
        #endregion
        #region Command: HideColumn
        private ICommand _pHideColumn;
        public ICommand HideColumn
        {
            get
            {
                return _pHideColumn ??
                       (_pHideColumn = new RelayCommand(
                        (param) =>
                        {
                            // HideColumn Executed
                            var vm = param as ColumnSettingsViewModel;
                            if (vm == null) return;
                            vm.Visible = false;
                        },
                        (param) =>
                        {
                            // HideColumn CanExecute?  
                            var vm = param as ColumnSettingsViewModel;
                            if (vm == null) return false;
                            return !vm.Required && vm.Visible;
                        }));
            }
        }
        #endregion

        private void SwapColumnOrders(ColumnSettingsViewModel vm, int targetOrder)
        {
            // Get the column with targetOrder
            var col = Settings.FirstOrDefault(x => x.Order == targetOrder);
            if (col == null) return;
            var tmp = col.Order;
            col.Order = vm.Order;
            vm.Order = tmp;
            SettingsView.Refresh();
        }
    }

}
