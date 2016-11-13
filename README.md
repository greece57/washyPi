# washyPi

washyPi is the Part of our Washy Project.
The Idea of washy is that if you live in a studentdorm it's possible for everybody to reserve time for the washing machine. Instead of waiting for hours while walking down to see if a washer is free every 10 min with our solution it's possible that every student can schedule his washing time and reserve the washing machine for a specific time at a specific date.

User can register and reserve washers through our bot which is available for example for [Telegram](https://web.telegram.org/#/im?p=@washy_Bot) or [Facebook Messenger](https://www.facebook.com/messages/washyBot). On registration he has to upload a picture so we can later make a Face Recognition. Then he can reserve a washing machine at a specific time.

When going to the washroom our raspberry pi, which we will setup here, will take a picture when the user is near enaugh at the camera, upload this picture to our Azure BlubStorage and then send a POST-Request to the washyBackend with the URL to the picture.
The washyBackend sends either the name of the user in the picture back to the raspberry or an error code.

## What you need
  - Raspberry Pi 2 or 3 with Internet Connection
  - GrovePi+ Starter Kit (GrovePi+, Grove Red LED, Grove Green LED, Grove Ultrasonic Sensor, Grove - RGB Backlit LCD)
  - [Microsoft Azure Storage](https://azure.microsoft.com/de-de/services/storage/)
  - USB Kamera (not the PS3 Cam)
  - Visual Studio
  - Optinal: Monitor

## Setup
- I will try to bring you through the installation of the raspberry step by step

  ### Setup washyServices
  - Setup the [washyBackend](https://www.github.com/washyTUM/washyBackend)
  
  ### Setup the Raspberry
  - Mount the GrovePi+ on the Raspberry
  - Connect the Camera to the Raspberry per USB
  - Connect the Grove Red LED to Digital Port 2, the Grove Green LED to Digital Port 4 and the Grove Ultrasonic Sensor to any I2C port
  - Install Windows 10 IoT on your Raspberry with the [Windows IoT Core Dashboard](https://developer.microsoft.com/en-us/windows/iot/downloads) and power up your raspberry
  
  ### Setup Azure Storage
  - Setup Azure Blobstorage like discribed in this [tutorial](https://azure.microsoft.com/de-de/documentation/articles/storage-dotnet-how-to-use-files/)
  - Set the 4 constants (accountName = storageName, accountKey, containerName = blobContainerName, backendAddress = ipAddress of the washyBackend) at the Top of the MainPage.xaml.cs-File.
  
  ### Setup the Solution
  - Clone this Repository and open the Solution in Visual Studio
    - Select ARM as Solution Platform
    - Select the personRecognizer as Startup Project
    - Select "Remote Machine" as destination (select your Raspberry or fill in the ip-Address of the Pi)
  - After the App is diployed and started running on the Pi it should start with the initialization and after some seconds you should be able to see the Camera Preview on the Monitor
  
## Now what
If you now register yourself in a Bot and have a running Backend you should be able to see that the Raspberry Pi gives you green Light on the LCD RGB Display when you have a reservation at the moment.
After going near enaugh to the UltraSonic Sensor the green LED will light up and the camera will take a photo. While uploading the red light will be on. This takes a little time at the first time. Then the LCD RGB Screen should give you feedback
