
# XGD.PackageBuilder Project Description

PackageBuilder is a .NET library to write Linux installation package files, as well as build an ISO from them.

PackageBuilder leverages a subset of [DiscUtils](ttps://github.com/DiscUtils/DiscUtils/) to write archives and ISO9660 formats.

## Wiki

See more up to date documentation at the [Wiki](https://github.com/XGD/PakcageBuilder/wiki)

## How to use the Library

Here's a few really simple examples.

### How to create a new OPkg/IPkg:

```csharp
``` 

You can add files as streams (shown above), or as files from the filesystem. By using a different form of Build, you can get a Stream to the file, rather than writing it to the filesystem.

