KinectAttract
=============

Prototype Kinect app to attract people to come and interact with something

Built on top of the Kinect for Windows V2 API.

Based on the example "KinectFaceTrackingDemo" from [rarcher software](http://rarcher.azurewebsites.net/Post/PostContent/44)


Current Features
-------------

- Uses MutliSourceFrameReader to get synchronised tracking and colour frames
- Tracks the first user it finds, and starts facial tracking
- Cleans up
- Animates an on-screen face to indicate status:
  - Intermittent blinking (5-12 seconds) whilst running
  - Hunting for a person - eyes move left and right, pupils narrow
  - Found a person - stops hunting
  - Engaged with a person (they are looking at Kinect/screen) - pupils dilate
  - Disengaged with a person (they are no longer tracked/looking at the screen) - returns to hunting

Features to be implement
-------------

- Hook up mouth animation when found but not tracked
- Pick a user to track when multiple tracked
- Hands tracking - when picking things up, etc.