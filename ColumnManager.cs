﻿using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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

        private static DataGridLength ConvertToDataGridLength(double? width)
        {
            return width == null ? DataGridLength.SizeToCells : new DataGridLength(width.Value);
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

        void LoadSettings()
        {
            var settings = SettingsRepository.Load(SerializationId);
            if (settings == null)
            {
                //_settingsLoaded = true;
                return;
            }

            ColumnSettings = new ReactiveCollection<ColumnSettingsViewModel>();
            foreach (var columnSettings in settings.ColumnSettings)
            {
                ColumnSettings.Add(new ColumnSettingsViewModel(columnSettings));
            }
            ColumnSort = settings.ColumnSort;
            //_settingsLoaded = true;
        }

        private bool _settingsLoaded;
        void DataGrid_AutoGeneratingColumns(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            //while (!_settingsLoaded)
           //     Thread.Sleep(50);

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
            ApplyColumnSettings(e.Column, columnId);
        }

        private void HandleCustomColumns()
        {
            foreach (var col in DataGrid.Columns)
            {
                var boundColumn = col as DataGridBoundColumn;
                if (boundColumn == null) continue;
                var binding = boundColumn.Binding as Binding;
                if (binding == null) continue;
                var columnId = binding.Path.Path;
                SetColumnId(col, columnId);
                ApplyColumnSettings(col, columnId);
            }
            ApplyGridSettings();
        }

        private bool ApplyColumnSettings(DataGridColumn col, string columnId)
        {
            if (ColumnSettings == null) return false;
            var cd = ColumnSettings.FirstOrDefault(x => x.Id == columnId);
            if (cd != null)
            {
                cd.ApplyColumnSettings(col);
                return false;
            }
            return true;
        }

        private void ApplyGridSettings()
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
            else
            {
                cm.HandleCustomColumns();
            }

            cm.DataGrid.Unloaded += cm.DataGrid_Unloaded;
        }

        void DataGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            ApplyGridSettings();
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
        private ICommand _pToggleColumnVisibility;
        public ICommand ToggleColumnVisibility
        {
            get
            {
                return _pToggleColumnVisibility ??
                    (_pToggleColumnVisibility = new RelayCommand<ColumnSettingsViewModel>(
                        (vm) =>
                            {
                                if (vm == null) return;
                                vm.Visible = !vm.Visible;
                                var col = DataGrid.Columns.FirstOrDefault(x => GetColumnId(x) == vm.Id);
                                if (col == null) return;
                                col.Visibility = vm.Visible ? Visibility.Visible : Visibility.Collapsed;
                            },
                        (vm) =>
                            {
                                if (vm == null) return false;
                                var visibleColumns = ColumnSettings.Count(x => x.Visible);
                                return visibleColumns != 1 || !vm.Visible;
                            }));
            }
        }
        #endregion
        #endregion
        #endregion

        private string SerializationId { get; set; }
        private ColumnSort ColumnSort { get; set; }
    }

}
