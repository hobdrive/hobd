 /hobd
==============
  This code is a core part of HOBD. It responsible for working with OBD2 PIDs and sensors.

  /hobd is a trip computer piece of software
  which utilize OBD-II link (USB or Bluetooth) to fetch and analyze vehicle data in realtime.

  ![Screenshot](https://github.com/downloads/cail/hobd/s2_e.png)

  Runs both under desktop pc, or on any WinCE based platform
  (PDA, PNA, noname chinese 'gps navigator'-titled stuff)

  Beside all the stuff you may find in tons of OBDII scanner tools, here are some
  Unique features:

  - Concentration on trip data: MPG, distance run, total fuel consumption, run time, time spent in jams, etc.
  - Precise MPG (fuel consumption) analysis and calculations.
  - Toyota owners have precise MPG data via custom injector pulse width PID and other extra sensors.
  - Sensor data logging facilities for data review.
  - Fine graned touch-friendly fluid UI for use in carputer/navigator uninterruptly.
  - Full i18n and metric/imperial switching.
  - Visual themes.
  - HUD (screen projection) mode.
  - Opensource core, extensible.
  - Runs on any platform with .net installed (WinCE/PNA/PDA/PocketPC/WinMobile/Windows)

 Links
==============

  hobDrive: http://hobdrive.com

  Downloads: http://hobdrive.com/hobd/

  Issues: http://github.com/cail/hobd/issues

  Fork me on github: http://github.com/cail/hobd

 Author
==============
  http://github.com/cail

  My credits to OBDGauge opensource software which was a trigger for me
  to create /hobd. My credits to ECUTracker which inspired me in some areas
  but which I had to drop because of poor old J2ME platform.

 License
==============
  "MPL 1.1" (With fallback on your choice to GPL 2.0/LGPL 2.1)

  Mozilla Public License allows you to modify this project's source code
  and redistribute it for free (or for sale) so long as you follow the terms
  of the MPL. In particular, you must make your changes to this project's
  source code available back to it under the MPL (so this project can benefit
  from your changes).

  You can add external components to /hobd core, compile these, use library
  in external projects and redistribute them for free or for sale and you
  do not need to make such external files or changes to them available in
  source code form or binary form to this project.

  /hobd uses fleux ui framework (http://fleux.codeplex.com) which is distributed under
  "Microsoft Public License (Ms-PL)". /hobd uses a modified fleux version which could be found at
  http://fleux.codeplex.com/SourceControl/network/Forks/cail/fleuxdesktop2

  /hobd uses InTheHand.Net.Personal framework (32feet.net) which is distributed under
  "In The Hand Community License".
