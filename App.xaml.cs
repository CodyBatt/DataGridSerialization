using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Akavache;
using ReactiveUI;

namespace DataGridSerialization
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            BlobCache.ApplicationName = "DataGridSerialization";
            RxApp.ConfigureServiceLocator((t, s) => null, (t, s) => null, (c, t, s) => { });

        }
    }
}
