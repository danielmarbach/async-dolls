using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace configureawait
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            MessageBox.Show("Attach Debugger", "Attach Debugger", MessageBoxButton.OK);
            Debugger.Launch();

            Debug.WriteLine(SynchronizationContext.Current != null ? "Before AsyncMethod with context and ConfigureAwait(true)" : "Before AsyncMethod without context and ConfigureAwait(true)");

            await AsyncMethod();

            Debug.WriteLine(SynchronizationContext.Current != null ? "After AsyncMethod with context and ConfigureAwait(true)" : "After AsyncMethod without context and ConfigureAwait(true)");

            await AsyncMethod().ConfigureAwait(false);

            Debug.WriteLine(SynchronizationContext.Current != null ? "After AsyncMethod with context and ConfigureAwait(false)" : "After AsyncMethod without context and ConfigureAwait(false)");
        }

        static async Task AsyncMethod()
        {
            Debug.WriteLine(SynchronizationContext.Current != null ? "Before Task.Delay with context and ConfigureAwait(false)" : "Before Task.Delay without context and ConfigureAwait(false)");
            await Task.Delay(100).ConfigureAwait(false);
            Debug.WriteLine(SynchronizationContext.Current != null ? "Before Task.Delay with context and ConfigureAwait(true)" : "Before Task.Delay without context and ConfigureAwait(true)");
            await Task.Delay(100);
        }
    }
}
