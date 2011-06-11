
Build
=========================

  Build env:
    windows sdk,
    .net cf 3.5
    monodevelop/sharpdevelop/visual studio express

  Checkout or download fleux mod:
    lib/fleuxdesktop2/README

  Checkout or download wmautoupdate:
    github.com/cail/wmautoupdate

  Checkout or download hobdcore into:
    ../hobdcore
  from
    github.com/cail/hobd/

  To build both win32 and wince builds:
    build.bat


Simulator installation
=========================

  com0com null modem should be installed (com0com.sourceforge.net)

  CNCB0 and COM7 (or other names) mapping should be created there

  Run:
      
      lib/obdsim.bat

  to start obd simulator.

  HOBD then will connect to COM7 (or your name) normally


WINCE simulator
=========================

  Device emulator (wince):
    http://www.microsoft.com/downloads/en/details.aspx?FamilyID=a6f6adaf-12e3-4b2f-a394-356e2c2fb114
  Device images:
    http://www.microsoft.com/downloads/en/details.aspx?FamilyID=38C46AA8-1DD7-426F-A913-4F370A65A582&displaylang=en#filelist

  Win mobile emulator:
    http://www.microsoft.com/downloads/en/details.aspx?FamilyID=83a52af2-f524-4ec5-9155-717cbe5d25ed

  Tests should be done both in vga and qvga modes (PDA and Windows Phone modes)!

