# OTC
This project is to do Over the Counter trading (OTC).

# How to Test
1. Run Spook
1. Run the OTC project
3. Configure Poltergeist
4. Use OTC

## 1. Run Spook
To run Spook you need to have it and compile it. [Spook](https://github.com/phantasma-io/Spook)
> Go to Spook Debug folder ```%FolderStructure%\Spook\Spook\bin\Debug\netcoreapp3.1```
To run Spook.
1. Open a terminal window, and run the ```./spook-cli.exe```

## 2. Run the OTC project
To run OTC.
* First you need to compile the project by open it and go to Build>Build Solution
* If you have Visual Studio open the OTC.sln and run it.
* If not, Open a terminal window inside the Server Folder and execute this command ```dotnet run ./Backend/bin/Debug/netcoreapp3.1/OTC.dll --path=../Frontend/```
* If it doesn't run, try executing with this command ```dotnet run ./Backend/bin/Debug/netcoreapp3.1/OTC.dll --port=80 --env=prod --path=./Frontend/ --phantasma.rest=http://localhost:7078/api --api=http://localhost:7077/rpc -cache.path=Cache```

## 3. Configure Poltergeist
To Run poltergeist you need to have [Poltergeist](https://github.com/phantasma-io/Poltergeist/releases/tag/v2.5.1)
1. Download it.
2. Unzip the zip folder.
3. Run it.

### To configure
1. Go to settings
2. On nexus choose Local Net
3. Click on the Confirm Button
4. Import a wallet, in these case it should be the test wallet with the WIF ```L2LGgkZAdupN2ee8Rs6hpkc65zaGcLbxhbSDGq8oh6umUxxzeW25```

## 4. Use OTC
To use OTC you need to have Spook running, Poltergeist running and OTC running.
To Access it you need to open a web browser and go to http://localhost/ or http://127.0.0.1/
1. Choose the poltergeist button and allow the dapp on the poltergeist client.
2. Take a look, create your offer and have fun. :)

## Problems
After following these steps and it doesn't work, try this.
Clean all the projects and Compile them and try again from step one.
If it still doesn't work, contact @Tek or @Relfos


