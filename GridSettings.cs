using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSerialization
{
    public class ColumnSettings
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public int Order { get; set; }
        public double? Width { get; set; }
        public bool Visible { get; set; }
    }

    public class ColumnSort
    {
        public int Index { get; set; }
        public ListSortDirection Direction { get; set; }
    }

    public class GridSettings
    {
        public GridSettings()
        {
            ColumnSettings = new List<ColumnSettings>();
        }
        public List<ColumnSettings> ColumnSettings { get; set; }
        public ColumnSort ColumnSort { get; set; }
    }

    public interface IGridSettingsRepository
    {
        Task<GridSettings> LoadAsync(string identifier);
        void Save(string identifier, GridSettings toSave);
    }
}
