 /hobd
==============
  /hobd is a trip computer piece of software
  which utilize OBD-II link to fetch vehicle data in realtime.

  ![Screenshot](https://github.com/downloads/cail/hobd/s2_e.png)

  Runs both under desktop pc, or on any WinCE based platform
  (PDA, PNA, noname chinese 'gps navigator'-titled stuff)

  Beside all the stuff you may find in tons of OBDII scanner tools, here are some
  Unique features:

  - Fine graned touch-friendly fluid UI for use in carputer/navigator uninterruptly.
  - Precise MPG (fuel consumption) analysis and calculations.
  - Toyota owners will receive precise data via custom injector pulse width PID and some others.
  - Sensor data logging facilities for data review.
  - Full i18n and metric/imperial switching.
  - Visual themes.
  - Opensourced, extensible.

 Changelog
==============
 - v0.3
   - cusom visual themes support
   - UI settings for port, theme, language, vehicle
   - improved default layout
   - make fuel efficiency sensors run smoother
   - new trip time and idle time data
   - performance improvements in UI
   - sensor reading is done at maximum speed now

 - v0.2
   - Better PID naming
   - Reset trip, automatic trip suspend on disconnection functionality
   - Localization and units translation.
   - short term fuel consumption

 - v0.1
   - proof of concept

 Links
==============
  Downloads: http://github.com/cail/hobd/download

  Issues: http://github.com/cail/hobd/issues

  Fork me on github: http://github.com/cail/hobd

 Author
==============
  Igor Russkih (http://github.com/cail)

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
  http://fleux.codeplex.com/SourceControl/network/Forks/cail/fleuxdesktop

  /hobd uses InTheHand.Net.Personal framework (32feet.net) which is distributed under
  "In The Hand Community License".

 Configuration help
==============

 config.xml
--------------
  
  log-level: ERROR, WARN, INFO, TRACE, DUMP

  port:
    COM7 or any other serial port name
    btspp://000a95020c7b:1;pin=1234
      means bluetooth serial connection towards
      000a95020c7b address, with 1234 pin (optional), with '1' service id (optional)

  vehicle:
    name of the vehicle to use

  vehicles:
    (one or more entries)
    file names with vehicle configuration data

  dpi:
    DPI resolution. default is 92 (480x272). Less values will scale for the large resolutions
  language:
    Preferred language of all user interface elements
  theme:
    Class name for the visual theme layout.
    Default is 'hobd.HOBDTheme'
  layout:
    .layout file with required layout


 *.vehicles
--------------
  Vehicle specific parameters:

  <vehicle name="yourname">
    name to address this model

  obd:
    engine: engine class to use (default is 'hobd.OBD2Engine')
      sensors: Sensor hobd.OBD2Sensors
      sensors: more than one is possible to load

    liters: engine volume in liters (f.e. 1.8)
    cylinders: number of cylinders
    14.9: stoich air/fuel ratio

    custom sensor-specific parameters:
    injector-ccpm: injector performance in cubic centimeters per minute

 *.layout
--------------
    item:
      size: small, normal, large, huge, giant
      precision: number of digits in floating numbers


