## Build

Hobdrive runs on multiple platform, with C# as a major development language.

### Windows/CE Build env:
  *  windows sdk
  * .net compact framework 3.5
  * .net power toys
  * windows sdk + visual studio express (heavy option)
  * Alternative light option: monodevelop 3.x, mono (on linux)

### Android Build env:
  * [closed source]

### Build
  
1. Checkout **github.com/cail/hobd** into **/hobdcore**

3. Checkout or download fleux **github.com/cail/fleux** into **/hobdcore/lib/fleuxdesktop2**

2.  Checkout or download wmautoupdate **github.com/cail/wmautoupdate** into **/hobdcore/lib/wmautoupdate**

4.  To build both win32 and wince builds:
    
    cd hobdcore
    build.bat

5. Use /hobdcore/hobd.csproj for the development under MSVC or MonoDevelop (recommended)

    
## OBD2 Data Simulator installation

1.  Run:
      
      lib/obdsim_tcp.bat

  to start obd simulator.

2.  To connect hobdrive to simulator, enter
      tcp://127.0.0.1:1234
  in connection port settings


## WINCE simulator

1.  Easy way: Prepared wince5/6 simulators, ready to run:

  http://hobdrive.com/download/DeviceEmulator.zip

  Use *.bat files to run, fix path to attach external storage

2. Hard way:
  2.1 Device emulator (wince):
    http://www.microsoft.com/downloads/en/details.aspx?FamilyID=a6f6adaf-12e3-4b2f-a394-356e2c2fb114
  
  2.1 Device images:
    http://www.microsoft.com/downloads/en/details.aspx?FamilyID=38C46AA8-1DD7-426F-A913-4F370A65A582&displaylang=en#filelist

  2.1 Win mobile emulator:
    http://www.microsoft.com/downloads/en/details.aspx?FamilyID=83a52af2-f524-4ec5-9155-717cbe5d25ed
    
3. Run    
  To run hobdrive on simulator without .net installation, use 'standalone' version files, or:
    1) copy content of /lib/.net/* into hobdrive's folder
    2) remove hobdrive.exe.config file
    
  Tests should be done both in vga and qvga modes (PDA and Windows Phone modes)!
