using System.Threading.Tasks;
using DevToys.OutOfProcService.Core.OOP;
using DevToys.Shared.Api.Core;
using DevToys.Shared.Core;

namespace DevToys.Extension.PngJpegCompressor.FullTrust
{
    internal sealed class Program
    {
        private static MefComposer? _mefComposer;
        private static IMefProvider? _mefProvider;

        public static async Task Main(string[] args)
        {
            _mefComposer
                = new MefComposer(
                    typeof(Program).Assembly,
                    typeof(Shared.Constants).Assembly);

            _mefProvider = _mefComposer.ExportProvider.GetExport<IMefProvider>();

            await _mefProvider.Import<AppService>().WaitAppServiceConnectionCloseAsync();
        }

        //private static AppServiceConnection connection;
        //private static TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

        //public static async Task Main(string[] args)
        //{
        //    Debugger.Launch();
        //    connection = new AppServiceConnection();
        //    connection.AppServiceName = "com.devtoys.pngjpegcompressor.fulltrust";
        //    connection.PackageFamilyName = Package.Current.Id.FamilyName;
        //    connection.RequestReceived += Connection_RequestReceived;
        //    connection.ServiceClosed += Connection_ServiceClosed;

        //    AppServiceConnectionStatus status = await connection.OpenAsync();
        //    if (status != AppServiceConnectionStatus.Success)
        //    {
        //        await taskCompletionSource.Task;
        //    }
        //}

        //private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        //{
        //    taskCompletionSource.TrySetResult(false);
        //}

        //private static void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        //{
        //}
    }
}
