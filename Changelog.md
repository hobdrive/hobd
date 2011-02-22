 Changelog
==============
 - v?.?
   - New OBD-II sensor definitions added,
   - ELM specific sensors: Voltage
   - Fix DTC reading on CAN
   - ELM reset detection
   - Better search for PID reply value
   - SensorTrack draft

 - v0.6
   - DTC reading & clearing sensors
   - DTC decoding helper

 - v0.5 21-Dec-2010
   - Fuel consumption uses infinity when vehicle is stopped
   - ELM custom initialization commands (included vehicle configs for explicit speed and JDM Toyotas)
   - Sensor names and descr are in translation resources now

 - v0.4 10-Dec-2010
   - MAP (Manifold absolute pressure) based fuel consumtion calculations
   - G (Acceleration) sensor
   - Ease of connection towards nonstandard china BT hardware
   - New vehicle parameter: correction coefficient on speed

 - v0.3 15-Nov-2010
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
