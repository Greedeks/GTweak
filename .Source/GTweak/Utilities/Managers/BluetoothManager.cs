using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GTweak.Utilities.Controls;
using Windows.Devices.Radios;
using Windows.Foundation;

namespace GTweak.Utilities.Managers
{
    internal class BluetoothManager
    {
        private static Radio _bluetooth;

        internal static bool IsAvailable { get; private set; }
        internal static bool IsEnabled => _bluetooth?.State == RadioState.On;

        static BluetoothManager()
        {
            AppDomain.CurrentDomain.ProcessExit += delegate
            {
                if (_bluetooth != null)
                {
                    Marshal.ReleaseComObject(_bluetooth);
                    _bluetooth = null;
                }
            };
        }

        internal static void Initialize()
        {
            try
            {
                if (Await(Radio.RequestAccessAsync()) != RadioAccessStatus.Allowed)
                {
                    IsAvailable = false;
                    return;
                }

                IReadOnlyList<Radio> radios = Await(Radio.GetRadiosAsync());
                _bluetooth = radios.FirstOrDefault(r => r.Kind == RadioKind.Bluetooth);
                IsAvailable = _bluetooth != null;
            }
            catch { IsAvailable = false; }
        }

        internal static void SetState(bool state)
        {
            Task.Run(delegate
            {
                try
                {
                    if (_bluetooth == null)
                    {
                        return;
                    }

                    RadioState targetState = state ? RadioState.On : RadioState.Off;
                    if (_bluetooth.State == targetState)
                    {
                        return;
                    }

                    Await(_bluetooth.SetStateAsync(targetState));
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            });
        }

        private static T Await<T>(IAsyncOperation<T> operation)
        {
            using ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);
            operation.Completed = delegate { resetEvent.Set(); };
            resetEvent.Wait();
            return operation.GetResults();
        }
    }
}
