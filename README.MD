 /hobd v0.1
==============
  /hobd is a trip computer piece of software
  which utilize OBD-II link to fetch vehicle data in realtime.

  Runs both under desktop pc, or on any WinCE based platform
  (PDA, PNA, noname chinese 'gps navigator'-titled stuff)

  Beside all the stuff you may find in tons of OBDII scanner tools, here are some
  Unique features:
   - Fine graned touch-friendly UI for use in carputer/navigator uninterruptly.
   - Precise MPG (fuel consumption) analysis and calculations.
   - Toyota owners will receive precise data (much more precise than just MAF-based estimations) via custom injector pulse width PID.
   - Sensor data logging facilities for data review.
   - Opensourced, extensible

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
  but which I had to drop because of poor J2ME platform.

 License
==============
  MPL 2.0


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
      size: small, normal, large, huge
      precision: number of digits in floating numbers


