using Microsoft.Kinect;

namespace KinectAttract.Helpers
{
  internal static class SkeletalExtensions
  {
    public static Joint ScaleTo(this Joint joint, float width, float height, float skeletonMaxX, float skeletonMaxY)
    {
      var pos = new CameraSpacePoint
      {
        X = scale(width, skeletonMaxX, joint.Position.X),
        Y = scale(height, skeletonMaxY, -joint.Position.Y),
        Z = joint.Position.Z
      };

      joint.Position = NormalisePosition(pos);

      return joint;
    }

    public static Joint ScaleTo(this Joint joint, float width, float height)
    {
      return ScaleTo(joint, width, height, 1.0f, 1.0f);
    }

    private static float scale(float maxPixel, float maxSkeleton, float position)
    {
      float value = ((((maxPixel/maxSkeleton)/2)*position) + (maxPixel/2));
      if (value > maxPixel)
        return maxPixel;
      if (value < 0)
        return 0;
      return value;
    }

    internal static CameraSpacePoint NormalisePosition(CameraSpacePoint position)
    {
      if (position.Z < 0)
      {
        position.Z = 0.1f;
      }

      return position;
    }
  }
}