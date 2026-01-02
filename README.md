# Virtual remote web app with FLIRC

A simple web application to turn my physical remote controls into virtual ones using a FLIRC 2.0 USB stick.
The FLIRC 2.0 can receive IR signals from many remote controls and send them to a device via USB. It can also send IR signals, allowing it to control various devices.

This project features an easy-to-use web interface that allows users to manage and use virtual remotes. 
It integrates with the FLIRC 2.0 USB stick to both receive and send IR signals. 
Users can customize the controls by mapping physical remote buttons to virtual buttons within the app. 

## Design & Usage view

Simple end-user usage view, with mobile usage in mind (not perfect, but works well on most devices):

<img width="558" height="800" alt="image" src="https://github.com/user-attachments/assets/42428721-0381-472a-a9b0-8cb11e3940de" />

Designing view of the remote (built to work well on Desktop, touch was not taken into account but works to some extent):

<img width="555" height="1108" alt="image" src="https://github.com/user-attachments/assets/a5c60f33-33d7-4b16-a8af-6f85888359df" />

## Supported & Verified OS
- [x] MacOS (ARM64)
- [x] Windows (x64)
- [ ] Linux (x64)
- [x] Linux (ARM64)

## Getting Started

The application is built using Blazor Server (.NET), whilst wrapping the SDKs (C) of FLIRC to communicate with the hardware.

### Dependencies

* .NET 10.0
* LINUX: `libhidapi-hidraw0 libhidapi-dev libc6 libusb-1.0-0-dev libudev-dev` libs installed
* A [FLIRC USB](https://flirc.tv/products/flirc-usb-receiver?variant=43513067569384) stick (to run/test it)

### Features

* Simple web interface for easy interactions
* Automatic loading and saving of virtual remotes and their IR mappings
* Send and receive IR signals through FLIRC

## License

This project is licensed under [MIT LICENSE](LICENSE)

## External resources

* [flirc/sdk](https://github.com/flirc/sdk) - Repository of the FLIRC SDK which this application utilizes
