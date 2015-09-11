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

            Console.WriteLine(SynchronizationContext.Current != null ? "Before AsyncMethod with context and ConfigureAwait(true)" : "Before AsyncMethod without context and ConfigureAwait(true)");

            await AsyncMethod();

            Console.WriteLine(SynchronizationContext.Current != null ? "After AsyncMethod with context and ConfigureAwait(true)" : "After AsyncMethod without context and ConfigureAwait(true)");

            await AsyncMethod().ConfigureAwait(false);

            Console.WriteLine(SynchronizationContext.Current != null ? "After AsyncMethod with context and ConfigureAwait(false)" : "After AsyncMethod without context and ConfigureAwait(false)");
        }

        static async Task AsyncMethod()
        {
            Console.WriteLine(SynchronizationContext.Current != null ? "Before WriteLineAsync with context" : "Before WriteLineAsync without context");
            await Task.Delay(100).ConfigureAwait(false);
            Console.WriteLine(SynchronizationContext.Current != null ? "Before FlushAsync with context" : "Before FlushAsync without context");
            await Task.Delay(100);
        }
    }
}
