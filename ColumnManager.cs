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
    public class ColumnDescriptor : INotifyPropertyChanged
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
            set
            {
                if (value != _pVisible)
                {
                    _pVisible = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this,new PropertyChangedEventArgs("Visible"));
                    }
                }
            }
        }
        #endregion

        public ColumnDescriptor()
        {
            int breakhere = 100;
        }

        public ColumnDescriptor(DataGridColumn col)
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

        public event PropertyChangedEventHandler PropertyChanged;
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
            Application.Current.Exit += OnApplicationExit;
        }

        

        void OnApplicationExit(object sender, ExitEventArgs e)
        {
            SaveSettings();
        }

        async void SaveSettings()
        {
            if (DataGrid == null) return;
            CustomSettings.Clear();
            foreach (var col in DataGrid.Columns)
            {
                CustomSettings.Add(new ColumnDescriptor(col));
            }
            await BlobCache.LocalMachine.InsertObject(SerializationId, CustomSettings);
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
            if( CustomSettings == null)
                CustomSettings = new ObservableCollection<ColumnDescriptor>();
            var cd = CustomSettings.FirstOrDefault(x => x.Id == columnId);
            if (cd != null)
            {
                cd.ApplyColumnSettings(e.Column);
            }
            else
            {
                cd = new ColumnDescriptor(e.Column);
                CustomSettings.Add(cd);
            }
            cd.SetColumnHeader(SerializationId, e.Column);
            
        }

        #region DependencyProperty: CustomSettings
        public static readonly DependencyProperty CustomSettingsProperty =
            DependencyProperty.Register("CustomSettings", typeof(ObservableCollection<ColumnDescriptor>), typeof(ColumnManager), new PropertyMetadata(default(ObservableCollection<ColumnDescriptor>)));

        public ObservableCollection<ColumnDescriptor> CustomSettings
        {
            get { return (ObservableCollection<ColumnDescriptor>)GetValue(CustomSettingsProperty); }
            set { SetValue(CustomSettingsProperty, value); }
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
            if (cm.DataGrid.AutoGenerateColumns)
            {
                cm.DataGrid.AutoGeneratingColumn += cm.DataGrid_AutoGeneratingColumns;
                cm.DataGrid.AutoGeneratedColumns += cm.DataGrid_AutoGeneratedColumns;
            }

            cm.DataGrid.Unloaded += cm.DataGrid_Unloaded;
        }

        void DataGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            foreach (var col in DataGrid.Columns)
            {
                var columnId = GetColumnId(col);
                if (columnId == null) continue;
                var cd = CustomSettings.FirstOrDefault(x => x.Id == columnId);
                if (cd == null) continue;
                cd.ApplyColumnOrdering(DataGrid.Columns.Count, col);
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
            
            foreach (var cd in CustomSettings)
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
                        var cd = param as ColumnDescriptor;
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

        #region DependencyProperty: SerializationId
        public static readonly DependencyProperty SerializationIdProperty =
            DependencyProperty.Register("SerializationId", typeof(string), typeof(ColumnManager), new PropertyMetadata(SerializationIdChanged));
        public string SerializationId
        {
            get { return (string) GetValue(SerializationIdProperty); }
            set { SetValue(SerializationIdProperty, value); }
        }
        private static void SerializationIdChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var cm = sender as ColumnManager;
            BlobCache.LocalMachine.GetObjectAsync<ObservableCollection<ColumnDescriptor>>(cm.SerializationId)
                     .Subscribe(settings => Application.Current.Dispatcher.Invoke(() =>
                         {
                             cm.CustomSettings = new ObservableCollection<ColumnDescriptor>();
                             foreach (var setting in settings)
                             {
                                 cm.CustomSettings.Add(setting);
                             }
                             cm._settingsLoaded = true;
                         }, DispatcherPriority.Normal), 
                        error =>
                        {
                           cm._settingsLoaded = true;
                        });
        }
        #endregion

        
    }

}
