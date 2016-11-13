using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using GrovePi.Sensors;
using GrovePi;

namespace washyPi
{
    public sealed class StartupTask : IBackgroundTask
    {
        static RegistryManager registryManager;
        static string connectionString = "HostName=washyHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=SDnUERyeBaCFmQIE/pPpspwD7z47u9nN2sOn8dwXrmU=";
        static string deviceKey;
        static DeviceClient deviceClient;

        static GrovePi.I2CDevices.IRgbLcdDisplay screen;

        public void Run(IBackgroundTaskInstance taskInstance)
        {

            screen = DeviceFactory.Build.RgbLcdDisplay();
            screen.SetBacklightRgb(200, 0, 0);
            screen.SetText("setting up...");

            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddDeviceAsync().Wait();

            deviceClient = DeviceClient.Create(connectionString, new DeviceAuthenticationWithRegistrySymmetricKey("myFirstDevice", deviceKey));

            main(taskInstance);


        }

        private async void main(IBackgroundTaskInstance taskInstance)
        {
            var def = taskInstance.GetDeferral();

            Microsoft.Azure.Devices.Client.Message message = await deviceClient.ReceiveAsync();

            string name = message.Properties["name"];

            screen.SetText(name);

            def.Complete();
        }

        private static async Task AddDeviceAsync()
        {
            string deviceId = "myWashmaschine";
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            deviceKey = device.Authentication.SymmetricKey.PrimaryKey;
        }

    }
}
