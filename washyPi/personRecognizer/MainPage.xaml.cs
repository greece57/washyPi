using Microsoft.Maker.Devices.Media.UsbCamera;
using System;
using Windows.Media.Capture;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GrovePi.Sensors;
using GrovePi;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Net.Http;
using System.Collections.Generic;
using System.Net;
using RestSharp.Portable.HttpClient;
using RestSharp.Portable;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const int userRange = 100;
        const string accountName = "washydiag606";
        const string accountKey = "8QVb2d+vRjhuSr5DnNUix9Tii77//+ItK8kh4KDRDDJxy8cmW4XV+9oPhhkZ6ZSB96AO1SV/EPqHdbnvE5OvUg==";
        const string containerName = "picture-storage";
        const string backendAddress = "http://washybackend.azurewebsites.net";
        const string roomId = "1";

        private ILed infoLed;
        private ILed panicLed;
        private IUltrasonicRangerSensor ranger;
        private GrovePi.I2CDevices.IRgbLcdDisplay screen;

        private CloudBlobContainer container;

        private UsbCamera camera;

        private bool sendingPhoto;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private bool userInRange()
        {
            bool isInRange = false;
            for(int i = 0; i < 1; i++)
            {
                if (ranger.MeasureInCentimeters() <= userRange)
                    isInRange = true;
            }
            return isInRange;
        }


        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {


            sendingPhoto = false;
            panicLed = DeviceFactory.Build.Led(Pin.DigitalPin2);
            infoLed = DeviceFactory.Build.Led(Pin.DigitalPin4);
            ranger = DeviceFactory.Build.UltraSonicSensor(Pin.DigitalPin3);
            screen = DeviceFactory.Build.RgbLcdDisplay();
            screen.SetText("setting up...");
            screen.SetBacklightRgb(0, 0, 200);
            // init mode -> Both Led's are on
            panicLed.ChangeState(SensorStatus.On);
            infoLed.ChangeState(SensorStatus.On);


            // init camera
            camera = new UsbCamera();
            var initWorked  = await camera.InitializeAsync();

            // Something went wrong
            if (!initWorked || ranger.MeasureInCentimeters() == -1)
            {
                infoLed.ChangeState(SensorStatus.Off);
                screen.SetText("Camera or Sensor not connected!");
                screen.SetBacklightRgb(200, 0, 0);
                blink(panicLed);
                return;
            }

            // init photobackend

            Microsoft.WindowsAzure.Storage.Auth.StorageCredentials credentials = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(accountName, accountKey);
            //credentials.UpdateSASToken("?sv=2015-04-05&ss=b&srt=sco&sp=rwlac&se=2016-11-20T04:05:54Z&st=2016-11-12T20:05:54Z&spr=https,http&sig=B0zDabRXoO7LfWy5iACsn0sHOnWzvmmrDv8fAqITPgI%3D");
            CloudStorageAccount acc = new CloudStorageAccount(credentials, true);
            CloudBlobClient client = acc.CreateCloudBlobClient();
            container = client.GetContainerReference("picture-storage");

            previewElement.Source = camera.MediaCaptureInstance;

            await camera.StartCameraPreview();

            // init finished - turn off panic Led
            infoLed.ChangeState(SensorStatus.Off);
            panicLed.ChangeState(SensorStatus.Off);
            screen.SetText("");
            screen.SetBacklightRgb(0, 0, 0);

            DispatcherTimer mainThread = new DispatcherTimer();
            mainThread.Interval = TimeSpan.FromSeconds(0.5);
            mainThread.Tick += run;
            mainThread.Start();
        }

        private void run(object sender, object e)
        {
            if (userInRange())
            {
                if (infoLed.CurrentState == SensorStatus.Off && !sendingPhoto)
                {
                    infoLed.ChangeState(SensorStatus.On);
                    sendPhoto();
                }
            }
            else
            {
                infoLed.ChangeState(SensorStatus.Off);
            }
        }

        private async void sendPhoto()
        {
            sendingPhoto = true;
            panicLed.ChangeState(SensorStatus.On);

            Windows.Storage.StorageFile photo = await camera.CapturePhoto();

            // Retrieve reference to a blob named "myblob".
            string fileName = DateTime.Now.ToString("yyyy_MM_dd_h_mm_ss") + ".jpg";
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            // Create or overwrite the "myblob" blob with contents from a local file.
            await blockBlob.UploadFromFileAsync(photo);

            string httpAddressOfPhoto = "https://" + accountName + ".blob.core.windows.net/" + containerName + "/" + fileName;

            sendPost(httpAddressOfPhoto);

            sendingPhoto = false;
            panicLed.ChangeState(SensorStatus.Off);
        }

        private async void sendPost(string httpAddressOfPhoto)
        {
            var client = new RestClient(backendAddress);

            string address = "room/" + roomId + "/identify?" + "url=" + httpAddressOfPhoto;
            var request = new RestRequest(address, Method.POST);

            IRestResponse response = await client.Execute(request);
            var content = response.Content;
            screen.SetText(content);
            if (content == "true")
                screen.SetBacklightRgb(0, 200, 0);
            else
                screen.SetBacklightRgb(200, 0, 0);
        }

        private void blink(ILed led)
        {
            DispatcherTimer t = new DispatcherTimer();
            t.Interval = TimeSpan.FromSeconds(1);
            t.Tick += (s, e) => { led.ChangeState(led.CurrentState == SensorStatus.On ? SensorStatus.Off : SensorStatus.On); };
            t.Start();
        }
    }
}
