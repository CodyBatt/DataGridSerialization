using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Akavache;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Xaml;

namespace DataGridSerialization
{
    public class ColumnSettingsViewModel : ReactiveObject
    {
        public string Id;
        public string DisplayName;
        public int Order;
        public double? Width;
        #region Property Visible
        private bool _pVisible;
        public bool Visible
        {
            get { return _pVisible; }
            set { this.RaiseAndSetIfChanged(ref _pVisible, value); }
        }
        #endregion

        public ColumnSettingsViewModel(ColumnSettings cs)
        {
            Id = cs.Id;
            DisplayName = cs.DisplayName;
            Order = cs.Order;
            Width = cs.Width;
            Visible = cs.Visible;
        }

        public ColumnSettingsViewModel(DataGridColumn col)
        {
            Assign(col);
        }

        public void Assign(DataGridColumn col)
        {
            Id = ColumnManager.GetColumnId(col);
            Order = col.DisplayIndex;
            Visible = col.Visibility == Visibility.Visible;
            Width = col.ActualWidth;
            DisplayName = col.Header as string;
        }

        public void ApplyColumnSettings(DataGridColumn col)
        {
            col.Width = ConvertToDataGridLength(Width);
            col.Visibility = Visible ? Visibility.Visible : Visibility.Collapsed;            
        }

        public void ApplyColumnOrdering(int colcount, DataGridColumn col)
        {
            if( Order >= 0 && Order < colcount)
                col.DisplayIndex = Order;
        }

        public void SetColumnHeader(string serializationId, DataGridColumn col)
        {
            DisplayName = PropertyNameToDisplayName(serializationId, ColumnManager.GetColumnId(col));
            col.Header = DisplayName;
        }

        private static DataGridLength ConvertToDataGridLength(double? width)
        {
            return width == null ? DataGridLength.SizeToCells : new DataGridLength(width.Value);
        }

        public static string PropertyNameToDisplayName(string serializationId, string propertyName)
        {
            var displayName = serializationId + "_" + propertyName;
            // TODO: Return the localized string
            return displayName;
        }
    }

    public class ColumnManager : Control
    {
        #region AttachedProperty: ColumnId
        public static readonly DependencyProperty ColumnIdProperty =
            DependencyProperty.RegisterAttached("ColumnId", typeof (string), typeof (ColumnManager), new PropertyMetadata(default(string)));
        public static void SetColumnId(DataGridColumn element, string value)
        {
            element.SetValue(ColumnIdProperty, value);
        }
        public static string GetColumnId(DataGridColumn element)
        {
            return (string) element.GetValue(ColumnIdProperty);
        }
        #endregion

        static ColumnManager()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColumnManager), new FrameworkPropertyMetadata(typeof(ColumnManager)));
        }
        
        public ColumnManager()
        {
            SettingsRepository = new BlobCacheRepository();
            Loaded += ColumnManager_Loaded;
        }

        void ColumnManager_Loaded(object sender, RoutedEventArgs e)
        {
            // Have grid-columns already been auto-generated?
            Application.Current.Exit += OnApplicationExit;
            
        }

        #region DependencyProperty: SettingsRepository
        public static readonly DependencyProperty SettingsRepositoryProperty =
            DependencyProperty.Register("SettingsRepository", typeof (IGridSettingsRepository), typeof (ColumnManager), new PropertyMetadata(default(IGridSettingsRepository)));

        public IGridSettingsRepository SettingsRepository
        {
            get { return (IGridSettingsRepository) GetValue(SettingsRepositoryProperty); }
            set { SetValue(SettingsRepositoryProperty, value); }
        }
        #endregion

        void OnApplicationExit(object sender, ExitEventArgs e)
        {
            SaveSettings();
        }

        void SaveSettings()
        {
            if (DataGrid == null) return;
            var settings = new GridSettings();
            foreach (var col in DataGrid.Columns)
            {
                var columnSettings = new ColumnSettings()
                    {
                        DisplayName = col.Header as string,
                        Id = GetColumnId(col),
                        Order = col.DisplayIndex,
                        Visible = col.Visibility == Visibility.Visible,
                        Width = col.ActualWidth
                    };
                settings.ColumnSettings.Add(columnSettings);
                if (col.SortDirection == null) continue;

                settings.ColumnSort = new ColumnSort
                    {
                        Direction = col.SortDirection.Value,
                        Index = col.DisplayIndex
                    };
            }
            SettingsRepository.Save(SerializationId, settings);
        }

        async void LoadSettings()
        {
            var settings = await SettingsRepository.LoadAsync(SerializationId);
            if (settings == null)
            {
                _settingsLoaded = true;
                return;
            }

            ColumnSettings = new ReactiveCollection<ColumnSettingsViewModel>();
            foreach (var columnSettings in settings.ColumnSettings)
            {
                ColumnSettings.Add(new ColumnSettingsViewModel(columnSettings));
            }
            ColumnSort = settings.ColumnSort;
            _settingsLoaded = true;
        }

        private bool _settingsLoaded = false;
        void DataGrid_AutoGeneratingColumns(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            while (!_settingsLoaded)
                Thread.Sleep(50);

            // Skip IObservable properties "Changing" "Changed"
            var pt = e.PropertyType;
            if (pt.IsGenericType )
            {
                if (pt.GetGenericTypeDefinition() == typeof (IObservable<>))
                {
                    e.Cancel = true;
                    return;
                }
            }

            var columnId = e.PropertyName;
            SetColumnId(e.Column, columnId);
            e.Column.Header = GetLocalizedHeader(columnId);

            if (ColumnSettings == null) return;
            var cd = ColumnSettings.FirstOrDefault(x => x.Id == columnId);
            if (cd != null)
            {
                cd.ApplyColumnSettings(e.Column);                
            }
            else
            {
                e.Cancel = true;
            }
        }

        string GetLocalizedHeader(string columnId)
        {
            // TODO: Localize this
            return SerializationId + "_" + columnId;
        }

        #region DependencyProperty: ColumnSettings
        public static readonly DependencyProperty ColumnSettingsProperty =
            DependencyProperty.Register("ColumnSettings", typeof(ReactiveCollection<ColumnSettingsViewModel>), typeof(ColumnManager), new PropertyMetadata(default(ReactiveCollection<ColumnSettingsViewModel>)));

        public ReactiveCollection<ColumnSettingsViewModel> ColumnSettings
        {
            get { return (ReactiveCollection<ColumnSettingsViewModel>)GetValue(ColumnSettingsProperty); }
            set { SetValue(ColumnSettingsProperty, value); }
        }
        #endregion
        #region DependencyProperty: DataGrid
        public static readonly DependencyProperty DataGridProperty =
            DependencyProperty.Register("DataGrid", typeof (DataGrid), typeof (ColumnManager), new PropertyMetadata(DataGridChanged));
        public DataGrid DataGrid
        {
            get { return (DataGrid) GetValue(DataGridProperty); }
            set { SetValue(DataGridProperty, value); }
        }
        private static void DataGridChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var cm = sender as ColumnManager;
            if (cm == null || cm.DataGrid == null || string.IsNullOrEmpty(cm.DataGrid.Name)) return;
            cm.SerializationId = cm.DataGrid.Name;
            cm.LoadSettings();
            if (cm.DataGrid.AutoGenerateColumns)
            {
                cm.DataGrid.AutoGeneratingColumn += cm.DataGrid_AutoGeneratingColumns;
                cm.DataGrid.AutoGeneratedColumns += cm.DataGrid_AutoGeneratedColumns;
            }

            cm.DataGrid.Unloaded += cm.DataGrid_Unloaded;
        }

        void DataGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            if (ColumnSettings == null) return;
            foreach (var col in DataGrid.Columns)
            {
                var columnId = GetColumnId(col);
                if (columnId == null) continue;
                var cd = ColumnSettings.FirstOrDefault(x => x.Id == columnId);
                if (cd == null) continue;
                cd.ApplyColumnOrdering(DataGrid.Columns.Count, col);
            }
            if (ColumnSort != null)
            {
                DataGrid.Columns[ColumnSort.Index].SortDirection = ColumnSort.Direction;
            }
            DataGrid.ContextMenu = CreateContextMenu();
        }

        void DataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
        #region Context Menu
        ContextMenu CreateContextMenu()
        {
            var retval = new ContextMenu();
            
            foreach (var cd in ColumnSettings)
            {
                var item = new MenuItem();
                item.Header = cd.DisplayName;
                item.DataContext = cd;
                var checkedBinding = new Binding("Visible");
                item.SetBinding(MenuItem.IsCheckedProperty, checkedBinding);
                item.Command = ToggleColumnVisibility;
                item.CommandParameter = cd;                
                retval.Items.Add(item);
            }
            return retval;
        }

        #region Command: ToggleColumnVisibility
        private IReactiveCommand _pToggleColumnVisibility = null;
        public IReactiveCommand ToggleColumnVisibility
        {
            get
            {
                if (_pToggleColumnVisibility == null)
                {
                    
                    _pToggleColumnVisibility = new ReactiveCommand( /* Observable for CanExecute Here */);
                    _pToggleColumnVisibility.Subscribe((param) =>
                    {
                        var cd = param as ColumnSettingsViewModel;
                        if (cd == null) return;
                        cd.Visible = !cd.Visible;
                        var col = DataGrid.Columns.FirstOrDefault(x => GetColumnId(x) == cd.Id);
                        if (col == null) return;
                        col.Visibility = cd.Visible ? Visibility.Visible : Visibility.Collapsed;
                    });
                }
                return _pToggleColumnVisibility;
            }
        }
        #endregion
        #endregion
        #endregion

        private string SerializationId { get; set; }
        private ColumnSort ColumnSort { get; set; }
    }

}
