using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace KinectAttract
{
  /// <summary>
  ///   Interaction logic for Eyes.xaml
  /// </summary>
  public partial class Eyes : UserControl
  {
    private readonly DispatcherTimer _blinkTimer = new DispatcherTimer();
    private readonly Random _interval = new Random();
    private readonly DispatcherTimer _lookAroundTimer = new DispatcherTimer();
    private Storyboard _blinkStoryboard;
    private Storyboard _lookAroundStoryboard;
    private bool _looking;

    public Eyes()
    {
      InitializeComponent();

      setupTimersForStoryBoards();

      if (null != _blinkStoryboard)
      {
        _blinkTimer.Start();
      }
    }

    public void LookAround()
    {
      if (null != _lookAroundStoryboard)
      {
        _looking = true;

        _lookAroundTimer.Start();
      }
    }

    public void FoundPerson()
    {
      _looking = false;
      var gainFocus = FindResource("GainFocus") as Storyboard;
      if (gainFocus != null)
      {
        gainFocus.Begin();
      }
    }

    public void LostPerson()
    {
      _looking = false;
      var loseFocus = FindResource("LoseFocus") as Storyboard;

      if (loseFocus != null)
      {
        loseFocus.Begin();
        LookAround();
      }
    }

    private void setupTimersForStoryBoards()
    {
      _blinkStoryboard = FindResource("Blink") as Storyboard;

      _blinkTimer.Interval = nextBlink();
      _blinkTimer.Tick += causeBlink;

      _lookAroundStoryboard = FindResource("LookAround") as Storyboard;

      _lookAroundTimer.Interval = nextHunt();
      _lookAroundTimer.Tick += goHunting;
    }


    private void causeBlink(object sender, EventArgs e)
    {
      _blinkStoryboard.Begin();
      _blinkTimer.Interval = nextBlink();
    }

    private TimeSpan nextBlink()
    {
      return new TimeSpan(0, 0, _interval.Next(2, 8));
    }

    private void goHunting(object sender, EventArgs e)
    {
      if (_looking)
      {
        _lookAroundStoryboard.Begin();
      }
      else
      {
        _lookAroundTimer.Stop();
      }

      _lookAroundTimer.Interval = nextHunt();
    }

    private TimeSpan nextHunt()
    {
      return new TimeSpan(0, 0, _interval.Next(4, 8));
    }
  }
}