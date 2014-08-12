using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace KinectAttract.Helpers
{
  internal static class WpfExtensions
  {
    internal static void DrawPoint(this Canvas canvas, Joint joint)
    {
      // 1) Check whether the joint is tracked.
      if (joint.TrackingState == TrackingState.NotTracked) return;

      // 2) Map the real-world coordinates to screen pixels.
      joint = joint.ScaleTo((float) canvas.ActualWidth, (float) canvas.ActualHeight);

      // 3) Create a WPF ellipse.
      var ellipse = new Ellipse
      {
        Width = 20,
        Height = 20,
        Fill = new SolidColorBrush(Colors.LightBlue)
      };

      // 4) Position the ellipse according to the joint's coordinates.
      Canvas.SetLeft(ellipse, joint.Position.X - ellipse.Width/2);
      Canvas.SetTop(ellipse, joint.Position.Y - ellipse.Height/2);

      // 5) Add the ellipse to the canvas.
      canvas.Children.Add(ellipse);
    }
  }
}