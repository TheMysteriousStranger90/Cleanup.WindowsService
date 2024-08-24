# Cleanup.WindowsService
![Image 1](Screenshots/Screen8.png)

Cleanup.WindowsService is a Windows service designed to perform various system cleanup tasks, such as emptying the recycle bin, cleaning temporary folders, and removing old log files. This service helps maintain system performance and free up disk space by regularly cleaning up unnecessary files.

## Features

- Empty the recycle bin
- Clean temporary folders
- Clean the Downloads folder
- Clean the Prefetch folder
- Clean Windows temporary folder
- Clean log files
- Clean event logs
- Clean old files (`*.old`, `*.bak`, `*.tmp`)
- Clean trace files (`*.trace`)
- Clean cookies
- Clean remnant driver files
- Reset DNS resolver cache

## Installation

1. Install the service using the provided installer:
   - Run `setup.exe` as an administrator to install the service.

![Image 2](Screenshots/Screen1.png)
![Image 3](Screenshots/Screen2.png)
![Image 4](Screenshots/Screen3.png)
![Image 4](Screenshots/Screen4.png)

2. After installation, open the Services management console:
   - Press `Win + R`, type `services.msc`, and press `Enter`.

3. In the Services management, find `Cleanup Windows Service`.
![Image 4](Screenshots/Screen5.png)

4. Right-click on the service and select Properties.

5. Go to the Log On tab. Select This account. Enter the username and password of the account you want the service to run under. Click OK.
![Image 4](Screenshots/Screen6.png)
![Image 4](Screenshots/Screen7.png)

6. Go back to the General tab. Click Start to start the service.
![Image 4](Screenshots/Screen8.png)

## Usage

The service runs as a background service and performs cleanup tasks at regular intervals (once a day). The main cleanup tasks include:

- Emptying the recycle bin
- Cleaning various system folders
- Removing old and temporary files
- Resetting DNS cache
- Running System File Checker
- Logs are saved in the CleanupWindowsService folder in MyDocuments.

## Contributions

We welcome contributions from the community. If you have a command you'd like to add, feel free to open a pull request.

## Author

Bohdan Harabadzhyu

## License

[MIT](https://choosealicense.com/licenses/mit/)
