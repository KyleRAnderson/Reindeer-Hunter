# Reindeer-Hunter
A program that manages the Sacred Heart High School (Ottawa) annual reindeer hunt. 
Created by Kyle Anderson (eAUE) for the 2017 Reindeer Hunt at Sacred Heart High School.

&copy;2019 Kyle Anderson

## Development Environment
Development should be done with Windows 10 with the lastest .NET framework installed. It would probably work on older versions of Windows as long as the .NET libraries are up-to-date, however this hasn't been tester.

## Development Tools
- [Visual Studio 2017](https://visualstudio.microsoft.com/vs/)
- (Recommended) [Visual Studio Code](https://code.visualstudio.com/?wt.mc_id=vscom_downloads)

## Setup Instructions
1. Install Visual Studio 2017 with .Net Desktop Development Tools
1. Launch Visual Studio, Go To Tools -> Extensions and Updates and download and download the [*Microsoft Visual Studio 2017 installer projects extension*](https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects). 
   1. Quit Visual Studio to let the installation complete.
1. Download/Clone the repository, open the .sln file.
1. Make sure that all Nuget packages install properly. If they aren't installed automatically, go to `Tools -> Nuget Package Manager -> Manage Nuget Packages for solution` and it should prompt you to restore the packages. 
Below are the packages and their versions required for the program to function properly.
    1. FileHelpers by Marcos Meli and Contributors (V3.3.0)
    1. iTextSharp by Bruno Lowagie, Paulo Soares, et al. (V5.5.13)
    1. Newtonsoft.Json by James Newton-King (V11.0.2)
    1. ZkWeb.System.Drawing by ZKWeb.System (V4.0.1)
1. After opening the .snl file and assuring updates to Packages/Program (although you likely do not need to update), go to `Project -> Reindeer Hunter Properties`, then go to the "Signing" tab and uncheck the box that says "Sign the ClickOnce manifests". 
1. You're good to go!

## Development Conventions
Git branch names should be one of the following:
- cleanup
- fixes/*
- features/*
