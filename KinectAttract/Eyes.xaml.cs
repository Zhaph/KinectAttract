﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KinectAttract
{
  /// <summary>
  /// Interaction logic for Eyes.xaml
  /// </summary>
  public partial class Eyes : UserControl
  {
    private readonly DispatcherTimer _blinkTimer = new DispatcherTimer();
    private readonly DispatcherTimer _lookAroundTimer = new DispatcherTimer();
    private readonly Random _interval = new Random();
    private Storyboard _blinkStoryboard;
    private Storyboard _lookAroundStoryboard;
    private bool _looking = false;

    public void HuntForPerson()
    {
      if (null != _lookAroundStoryboard)
      {
        _looking = true;

        _lookAroundTimer.Start();
      }
    }

    public void FoundPerson(Object sender, RoutedEventArgs e)
    {
      var gainFocus = this.FindResource("GainFocus") as Storyboard;
      if (gainFocus != null) BeginStoryboard(gainFocus);
    }


    public Eyes()
		{
			this.InitializeComponent();

      setupTimersForStoryBoards();

      if (null != _blinkStoryboard)
      {
        _blinkTimer.Start();
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
      return new TimeSpan(0, 0, _interval.Next(5, 12));
    }

    private void goHunting(object sender, EventArgs e)
    {
      if (_looking)
      {
        _lookAroundStoryboard.Begin();
        _lookAroundTimer.Interval = nextHunt();
      }
      else
      {
        _lookAroundTimer.Stop();
      }
    }

    private TimeSpan nextHunt()
    {
      return new TimeSpan(0, 0, _interval.Next(4, 8));
    }
  }
}