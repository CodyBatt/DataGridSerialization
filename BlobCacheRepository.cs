using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Akavache;

namespace DataGridSerialization
{
    class BlobCacheRepository : IGridSettingsRepository
    {
        public async Task<GridSettings> LoadAsync(string identifier)
        {
            var obs = BlobCache.LocalMachine.GetObjectAsync<GridSettings>(identifier);
            var result = obs.ToTask();
            try
            {
                return result.Result;
            }
            catch (AggregateException ex)
            {
                // TODO: Log this?
                string message = ex.InnerException.Message;
            }
            return null;
        }

        public async void Save(string identifier, GridSettings toSave)
        {
            var obs = BlobCache.LocalMachine.InsertObject(identifier, toSave);
            await obs;
        }

    }
}
