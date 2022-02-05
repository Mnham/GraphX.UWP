using System;
using System.Threading.Tasks;

using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace GraphX.Controls
{
    public static class DispatcherHelper
    {
        public static async Task CheckBeginInvokeOnUi(Action action)
        {
            CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            if (dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
            }
        }
    }
}