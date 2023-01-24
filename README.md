# Beanify
A personal music library

Packages Required:
  1. Entity Framework (from NuGet Package Manager)
    Tools > NuGet Package Manager > Package Manager Console > 
    Enter the command: install-package EntityFramework
    
  2. Avalonia UI (best to use the version for Virtual Studio 2022 - 2019/2017 version gave us trouble)
    Install link: https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.AvaloniaVS
    Instructions to start a working app (if needed): https://docs.avaloniaui.net/docs/getting-started

  3. Azure.Storage.Blobs (from NuGet Package Manager)
    Tools > NuGet Package Manager > Manage NuGet Packages for Solution
    Browse for the package and install for the project
    
References: 
  1. System.IO.Compression
  2. System.IO.Compression.FileSystem
    Both of these can be added by going to Solution Explorer, right-clicking on the References under the project, and then selecting Add Reference.
    Then, search for the two references and add them.
    
   3. System.Configuration
    
Items:
  1. Right click on the app in Solution Explorer > Add > New Item > Application Manifest File
    Change the requestedExecutionLevel tag's level attribute to level="requireAdministrator"

Adding a Database:
  1. Right click on the app in Solution Explorer
  2. Click Add > New Item
  3. Select Data > Service-based Database
  4. The database should appear on the service explorer. Click the "Connect to Database" icon
  5. Enter the database server and the admin credentials (secret!)
  6. The database should be accessible!
