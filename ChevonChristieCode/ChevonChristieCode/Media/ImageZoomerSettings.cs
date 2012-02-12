using System;
using System.Windows;
using System.Collections.Generic;

namespace ChevonChristieCode.Media
{
   public class ImageZoomerSettings
   {
      public static Point Origin = new Point(0, 0);
      public static ImageZoomerSettings DefaultSettings = new ImageZoomerSettings();
      public ImageZoomerSettings(Point imagePosition, double totalImageScale = 1, double maxImageZoom = 5)
      {
         TotalImageScale = totalImageScale;
         ImagePosition = imagePosition;
         MAX_IMAGE_ZOOM = maxImageZoom;
      }
      public ImageZoomerSettings(double totalImageScale = 1, double maxImageZoom = 5)
      {
         TotalImageScale = totalImageScale;
         ImagePosition = ImageZoomerSettings.Origin;
         MAX_IMAGE_ZOOM = maxImageZoom;
      }

      internal double TotalImageScale = 1d;
      internal Point ImagePosition = new Point(0, 0);
      internal readonly double MAX_IMAGE_ZOOM = 5;

      internal bool bounceOnNextComplete = false;
      internal Point bounceAmount;
      internal Point oldFinger1;
      internal Point oldFinger2;
      internal double oldScaleFactor;
   }
}
