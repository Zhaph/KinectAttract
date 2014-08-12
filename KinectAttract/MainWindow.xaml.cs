using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KinectAttract.Annotations;
using KinectAttract.Helpers;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;

namespace KinectAttract
{
  /// <summary>Enum to flag the current state of the face we're tracking</summary>
  public enum TrackedFaceState
  {
    NoFace,
    LookingAway,
    MouthOpen,
    Neutral,
    Happy,
    Sad
  };

  /// <summary>Enum to flag the state of the left and right hands for the tracked body</summary>
  public enum TrackedHandState
  {
    NoHand,
    Open,
    Closed
  };

  /// <summary>
  ///   Interaction logic for MainWindow.xaml
  /// </summary>
  /// <remarks>
  ///   Based on the example "KinectFaceTrackingDeom" from rarcher software
  ///   (http://rarcher.azurewebsites.net/Post/PostContent/44)
  /// </remarks>
  public partial class MainWindow : Window, INotifyPropertyChanged
  {
    private const FaceFrameFeatures FaceFrameFeatures =
      Microsoft.Kinect.Face.FaceFrameFeatures.BoundingBoxInInfraredSpace |
      Microsoft.Kinect.Face.FaceFrameFeatures.PointsInInfraredSpace |
      Microsoft.Kinect.Face.FaceFrameFeatures.MouthOpen |
      Microsoft.Kinect.Face.FaceFrameFeatures.LookingAway |
      Microsoft.Kinect.Face.FaceFrameFeatures.Happy |
      Microsoft.Kinect.Face.FaceFrameFeatures.FaceEngagement |
      Microsoft.Kinect.Face.FaceFrameFeatures.Glasses |
      Microsoft.Kinect.Face.FaceFrameFeatures.LeftEyeClosed |
      Microsoft.Kinect.Face.FaceFrameFeatures.MouthMoved |
      Microsoft.Kinect.Face.FaceFrameFeatures.RightEyeClosed;

    private Body[] _bodies;
    private FaceFrameReader _faceFrameReader;
    private FaceFrameSource _faceFrameSource;
    private KinectSensor _kinectSensor;
    private MultiSourceFrameReader _multiFrameReader;

    private TrackedFaceState _trackedFaceState;
    private TrackedHandState _trackedLeftHandState;
    private TrackedHandState _trackedRightHandState;
    private bool _trackingBody;
    private WriteableBitmap _videoBitmap;

    private int _height = 1080;
    private int _width = 1920;

    public MainWindow()
    {
      InitializeComponent();

      // Subscribe to the unloaded event so we can clean up resources
      Unloaded += onUnloaded;

      initKinect();
    }


    /// <summary>Status of the Kinect device</summary>
    public string KinectStatus
    {
      get
      {
        if (_kinectSensor == null) return "Off";
        return _kinectSensor.IsAvailable ? "Available" : "Not available";
      }
    }

    /// <summary>Flags if we're actively tracking a body or not</summary>
    public bool TrackingBody
    {
      get { return _trackingBody; }
      set
      {
        if (value == _trackingBody) return;

        _trackingBody = value;
        OnPropertyChanged();
        OnPropertyChanged("LeftHandImage");
        OnPropertyChanged("RightHandImage");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>Setup the Kinect device</summary>
    private async void initKinect()
    {
      try
      {
        // Only one Kinect device can be connected currently - get a reference to it
        _kinectSensor = KinectSensor.GetDefault();

        // Subscribe to the Kinect's status change event
        _kinectSensor.IsAvailableChanged += (s, args) => OnPropertyChanged("KinectStatus");

        // Set the initial states of the hands and face
        _trackedLeftHandState = TrackedHandState.NoHand;
        _trackedRightHandState = TrackedHandState.NoHand;
        _trackedFaceState = TrackedFaceState.NoFace;

        _videoBitmap = new WriteableBitmap(_width, _height, 96, 96, PixelFormats.Bgr32, null);

        // Start the Kinect device
        _kinectSensor.Open();

        _multiFrameReader = _kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body);
        _multiFrameReader.MultiSourceFrameArrived += onFrameArrived;
      }
      catch (Exception ex)
      {
        Debug.WriteLine("Error in InitKinect(). {0}", ex.Message);
      }
    }

    private void onFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
    {
      try
      {
        MultiSourceFrame frame = e.FrameReference.AcquireFrame();

        if (frame == null) return;

        using (ColorFrame colourFrame = frame.ColorFrameReference.AcquireFrame())
        {
          if (colourFrame == null) return;

          // Get Colour Data
          FrameDescription colourFrameDescripton =
            colourFrame.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
          uint bytesPerPixel = colourFrameDescripton.BytesPerPixel;
          long frameSize = colourFrameDescripton.Width*colourFrameDescripton.Height*bytesPerPixel;
          var colourData = new byte[frameSize];

          if (colourFrame.RawColorImageFormat == ColorImageFormat.Bgra)
          {
            colourFrame.CopyRawFrameDataToArray(colourData);
          }
          else
          {
            colourFrame.CopyConvertedFrameDataToArray(colourData, ColorImageFormat.Bgra);
          }

          var rectangle = new Int32Rect(0, 0, _width, _height);
          _videoBitmap.WritePixels(rectangle, colourData, _width*4, 0);
          KinectColourImage.Source = _videoBitmap;
        }

        using (BodyFrame bodyFrame = frame.BodyFrameReference.AcquireFrame())
        {
          if (bodyFrame == null) return;

          if (_bodies == null)
          {
            _bodies = new Body[bodyFrame.BodyCount];
          }

          bodyFrame.GetAndRefreshBodyData(_bodies);

          int i = 0;

          TrackedBodyCount.Content = _bodies.Count(b => b.IsTracked);
          var bodyIds = _bodies.Where(b=> b.IsTracked).Select(b => new {b.TrackingId}).ToList();
          TrackedBodyIds.Content = string.Join(", ", bodyIds);
          foreach (Body body in _bodies)
          {
            if (body == null) continue;

            // Break out of the loop if the body isn't correctly tracked at the moment.
            if (!body.IsTracked) continue;

            TrackingBody = true;
            
            // For the body being tracked, show hand states. Note that we only
            // make changes to the current state if the device has high confidence
            // in the tracking of the particular hand (avoids the hand state images
            // flickering)
            if (body.HandLeftConfidence == TrackingConfidence.High)
            {
              TrackedHandState previousLeftHandState = _trackedLeftHandState;

              // Set our left hand state property (which controls the image path)
              switch (body.HandLeftState)
              {
                case HandState.Open:
                  _trackedLeftHandState = TrackedHandState.Open;
                  break;
                case HandState.Closed:
                  _trackedLeftHandState = TrackedHandState.Closed;
                  break;
                default:
                  _trackedLeftHandState = TrackedHandState.NoHand;
                  break;
              }

              // Need to change the image?
              if (previousLeftHandState != _trackedLeftHandState)
              {
                OnPropertyChanged("LeftHandImage");
              }
            }

            if (body.HandRightConfidence == TrackingConfidence.High)
            {
              TrackedHandState previousRightHandImage = _trackedRightHandState;

              // Set our right hand state property (which controls the image path)
              switch (body.HandRightState)
              {
                case HandState.Open:
                  _trackedRightHandState = TrackedHandState.Open;
                  break;
                case HandState.Closed:
                  _trackedRightHandState = TrackedHandState.Closed;
                  break;
                default:
                  _trackedRightHandState = TrackedHandState.NoHand;
                  break;
              }

              // Need to change the image?
              if (previousRightHandImage != _trackedRightHandState)
              {
                OnPropertyChanged("RightHandImage");
              }
            }

            {
              var drawingGroup = new DrawingGroup();
              using (DrawingContext drawingContext = drawingGroup.Open())
              {
                BodyCanvas.Children.Clear();

                BodyCanvas.DrawPoint(body.Joints[JointType.Head]);
                BodyCanvas.DrawPoint(body.Joints[JointType.Neck]);
              }
            }

            FoundStatus.Fill = new SolidColorBrush(Colors.DarkCyan);

            // Are we going to track a new face?
            if (_faceFrameSource == null || _faceFrameSource.TrackingId != body.TrackingId)
            {
              // We're tracking a new body/face. Create a face frame source and reader
              // The TrackingId is used to link the body and face
              _faceFrameSource = new FaceFrameSource(_kinectSensor)
              {
                TrackingId = body.TrackingId,
                FaceFrameFeatures = FaceFrameFeatures
              };
              //_faceFrameSource = new FaceFrameSource(_kinectSensor, body.TrackingId, _faceFrameFeatures);

              // Subscribe to the TrackingIdLost event (raised when the face we're tracing is lost)
              _faceFrameSource.TrackingIdLost += onFaceTrackingIdLost;

              // Subscribe to the face FrameArrived event
              _faceFrameReader = _faceFrameSource.OpenReader();
              _faceFrameReader.FrameArrived += onFaceFrameArrived;
            }

            // We've found a body, so break out of the loop.
            return;
          }


          // We didn't find a body, so revert to hunt...
          BodyCanvas.Children.Clear();
          TrackedStatus.Content = "Not tracking";
          TrackingBody = false;
          Hunt_Click(null, null);
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine("Error processing frame: {0}", ex.Message);
      }
    }

    private void onFaceFrameArrived(object sender, FaceFrameArrivedEventArgs e)
    {
      if (e.FrameReference == null) return;

      FaceFrame faceFrame = e.FrameReference.AcquireFrame();

      if (faceFrame == null) return;

      using (faceFrame)
      {
        TrackedFaceState previousTrackedFaceState = _trackedFaceState;

        _trackedFaceState = TrackedFaceState.Neutral;

        // Flag the new face state:
        if (faceFrame.FaceFrameResult.FaceProperties[FaceProperty.LookingAway] == DetectionResult.Yes)
        {
          // The user is looking away...
          _trackedFaceState = TrackedFaceState.LookingAway;
        }
        else if (faceFrame.FaceFrameResult.FaceProperties[FaceProperty.Happy] == DetectionResult.Yes)
        {
          // The user is looking at us, and is happy
          _trackedFaceState = TrackedFaceState.Happy;
        }
        else if (faceFrame.FaceFrameResult.FaceProperties[FaceProperty.MouthOpen] == DetectionResult.Yes)
        {
          // The user is looking at us, and has their mouth open
          _trackedFaceState = TrackedFaceState.MouthOpen;
        }

        // Only change the image if the face state is not the same as the previous frame
        if (previousTrackedFaceState != _trackedFaceState)
        {
          updateFace(sender);
        }
      }
    }

    private void onFaceTrackingIdLost(object sender, TrackingIdLostEventArgs e)
    {
      Debug.WriteLine("Face tracking lost!");

      _faceFrameReader.FrameArrived -= onFaceFrameArrived;
      _faceFrameSource.TrackingIdLost -= onFaceTrackingIdLost;

      _faceFrameReader.Dispose();
      _faceFrameReader = null;
      _faceFrameSource = null;

      _trackedFaceState = TrackedFaceState.NoFace;

      updateFace(sender);
    }

    private void updateFace(object sender)
    {
      EngageStatus.Fill =
        DisengageStatus.Fill = HuntStatus.Fill = FoundStatus.Fill = new SolidColorBrush(Colors.DarkGray);

      switch (_trackedFaceState)
      {
        case TrackedFaceState.NoFace:
          if (TrackingBody)
          {
            TrackedStatus.Content = "Tracking Body; No Face";
            Found_Click(sender, null);
          }
          else
          {
            TrackedStatus.Content = "Not Tracking Body; No Face";
            Disengage_Click(sender, null);
          }
          break;
        case TrackedFaceState.LookingAway:
          if (TrackingBody)
          {
            TrackedStatus.Content = "Tracking Body; Face Looking Away";
            Found_Click(sender, null);
          }
          else
          {
            TrackedStatus.Content = "Not Tracking Body; Face Looking Away";
            Disengage_Click(sender, null);
          }
          break;
        case TrackedFaceState.MouthOpen:
        case TrackedFaceState.Neutral:
        case TrackedFaceState.Happy:
        case TrackedFaceState.Sad:
        default:
          TrackedStatus.Content = "Tracking Body; Engaged Face: " + _trackedFaceState;
          Engage_Click(sender, null);
          break;
      }
    }

    private void onUnloaded(object sender, RoutedEventArgs e)
    {
      if (_multiFrameReader != null)
      {
        _multiFrameReader.Dispose();
        _multiFrameReader = null;
      }

      if (_faceFrameReader != null)
      {
        _faceFrameReader.Dispose();
        _faceFrameReader = null;
      }

      _faceFrameSource = null;

      if (_bodies != null)
      {
        foreach (Body body in _bodies.Where(body => body != null))
        {
//          body.Dispose();
        }
      }

      _videoBitmap = null;

      if (_kinectSensor == null) return;
      _kinectSensor.Close();
      _kinectSensor = null;
    }

    private void Hunt_Click(object sender, RoutedEventArgs e)
    {
      HuntStatus.Fill = new SolidColorBrush(Colors.DarkGreen);
      Attractor.LookForUser();
    }

    private void Found_Click(object sender, RoutedEventArgs e)
    {
      FoundStatus.Fill = new SolidColorBrush(Colors.DarkGreen);
      Attractor.FoundUser();
    }

    private void Engage_Click(object sender, RoutedEventArgs e)
    {
      EngageStatus.Fill = new SolidColorBrush(Colors.DarkGreen);
      Attractor.GainInterest();
    }

    private void Disengage_Click(object sender, RoutedEventArgs e)
    {
      DisengageStatus.Fill = new SolidColorBrush(Colors.DarkGreen);
      Attractor.LooseInterest();
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}