using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using Microsoft.Phone.Controls;

//NEED to add reference to the Silverlight Windows Phone Toolkit
//By no means complete, as I am still working on getting the "bounce" just right. The bounce is effect that occurs
//when an image is dragged past the end of the screen, and it snaps back
namespace ChevonChristieCode.Media
{
   /// <summary>
   /// Allows you to zoom images displayed in an image control
   /// </summary>
   public class WP7ImageZoomer : DependencyObject
   {
      private WP7ImageZoomer()
      {

      }

      static WP7ImageZoomer()
      {
         Images = new Dictionary<Image, ImageZoomerSettings>();
      }

      public static void EndImageZooming(Image image)
      {
          WP7ImageZoomer.SetIsZoomingEnabled(image, false);
      }

      private readonly static Dictionary<Image, ImageZoomerSettings> Images;

      private static void OnIsZoomingEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         if (d != null && d is Image)
         {
            var instance = d as Image;
            var listener = GestureService.GetGestureListener(instance);

            if ((bool)e.NewValue)
            {
               instance.RenderTransform = new CompositeTransform() { ScaleX = 1, ScaleY = 1, TranslateX = 0, TranslateY = 0 };
               instance.RenderTransformOrigin = ImageZoomerSettings.Origin;
               instance.CacheMode = new BitmapCache();
               listener.PinchStarted += OnPinchStarted;
               listener.PinchDelta += OnPinchDelta;
               listener.DragDelta += OnDragDelta;
               listener.DragCompleted += new EventHandler<DragCompletedGestureEventArgs>(listener_DragCompleted);
               instance.DoubleTap += OnDoubleTap;

               if (!Images.ContainsKey(instance))
                  Images.Add(instance, new ImageZoomerSettings());
            }
            else
            {
               listener.PinchStarted -= OnPinchStarted;
               listener.PinchDelta -= OnPinchDelta;
               listener.DragDelta -= OnDragDelta;
               instance.DoubleTap -= OnDoubleTap;

               Images.Remove(instance);
            }
         }
      }

      static void listener_DragCompleted(object sender, DragCompletedGestureEventArgs e)
      {
         
         ImageZoomerSettings settings;
         Image image = sender as Image;

         if (image == null)
            return;

         if (Images.TryGetValue(image, out settings))
         {
            if (settings.bounceOnNextComplete)
            {
               UpdateImagePosition(settings.bounceAmount, image, settings);
               settings.bounceOnNextComplete = false;
            }
         }
      }

      /// <summary>
      /// Initializes the zooming operation
      /// </summary>
      private static void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
      {
         ImageZoomerSettings settings;
         Image image = sender as Image;

         if (image == null)
            return;

         if (Images.TryGetValue(image, out settings))
         {
            settings.oldFinger1 = e.GetPosition(image, 0);
            settings.oldFinger2 = e.GetPosition(image, 1);
            settings.oldScaleFactor = 1;
         }
      }

      /// <summary>
      /// Computes the scaling and translation to correctly zoom around your fingers.
      /// </summary>
      private static void OnPinchDelta(object sender, PinchGestureEventArgs e)
      {
         ImageZoomerSettings settings;
         Image image = sender as Image;

         if (image == null)
            return;

         if (Images.TryGetValue(image, out settings))
         {
            var scaleFactor = e.DistanceRatio / settings.oldScaleFactor;
            if (!IsScaleValid(scaleFactor, settings))
               return;

            var currentFinger1 = e.GetPosition(image, 0);
            var currentFinger2 = e.GetPosition(image, 1);

            var translationDelta = GetTranslationDelta(
                currentFinger1,
                currentFinger2,
                settings.oldFinger1,
                settings.oldFinger2,
                settings.ImagePosition,
                scaleFactor);

            settings.oldFinger1 = currentFinger1;
            settings.oldFinger2 = currentFinger2;
            settings.oldScaleFactor = e.DistanceRatio;

            UpdateImageScale(scaleFactor, image, settings);
            UpdateImagePosition(translationDelta, image, settings);
         }
      }

      /// <summary>
      /// Moves the image around following your finger.
      /// </summary>
      private static void OnDragDelta(object sender, DragDeltaGestureEventArgs e)
      {
         ImageZoomerSettings settings;
         Image image = sender as Image;

         if (image == null)
            return;

         if (Images.TryGetValue(image, out settings))
         {
            var translationDelta = new Point(e.HorizontalChange, e.VerticalChange);
            bool bounced = false;

            if (IsDragValid(1, translationDelta, image, settings, out bounced))
               UpdateImagePosition(translationDelta, image, settings);

            if (bounced)
            {
               settings.bounceOnNextComplete = true;
               settings.bounceAmount = new Point(-e.HorizontalChange, -e.VerticalChange);
            }
         }
      }

      /// <summary>
      /// Resets the image scaling and position
      /// </summary>
      private static void OnDoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
      {
         ImageZoomerSettings settings;
         Image image = sender as Image;

         if (image == null)
            return;

         if (Images.TryGetValue(image, out settings))
         {
            ResetImagePosition(image, settings);
         }
      }


      /// <summary>
      /// Computes the translation needed to keep the image centered between your fingers.
      /// </summary>
      private static Point GetTranslationDelta(
          Point currentFinger1, Point currentFinger2,
          Point oldFinger1, Point oldFinger2,
          Point currentPosition, double scaleFactor)
      {
         var newPos1 = new Point(
          currentFinger1.X + (currentPosition.X - oldFinger1.X) * scaleFactor,
          currentFinger1.Y + (currentPosition.Y - oldFinger1.Y) * scaleFactor);

         var newPos2 = new Point(
          currentFinger2.X + (currentPosition.X - oldFinger2.X) * scaleFactor,
          currentFinger2.Y + (currentPosition.Y - oldFinger2.Y) * scaleFactor);

         var newPos = new Point(
             (newPos1.X + newPos2.X) / 2,
             (newPos1.Y + newPos2.Y) / 2);

         return new Point(
             newPos.X - currentPosition.X,
             newPos.Y - currentPosition.Y);
      }

      /// <summary>
      /// Updates the scaling factor by multiplying the delta.
      /// </summary>
      private static void UpdateImageScale(double scaleFactor, Image image, ImageZoomerSettings settings)
      {
         settings.TotalImageScale *= scaleFactor;
         ApplyScale(image, settings);
      }

      /// <summary>
      /// Applies the computed scale to the image control.
      /// </summary>
      private static void ApplyScale(Image image, ImageZoomerSettings settings)
      {
         ((CompositeTransform)image.RenderTransform).ScaleX = settings.TotalImageScale;
         ((CompositeTransform)image.RenderTransform).ScaleY = settings.TotalImageScale;
      }


      private static void ApplyPosition(Image image, ImageZoomerSettings settings)
      {
         ((CompositeTransform)image.RenderTransform).TranslateX = settings.ImagePosition.X;
         ((CompositeTransform)image.RenderTransform).TranslateY = settings.ImagePosition.Y;
      }

      private static void UpdateImagePosition(Point delta, Image image, ImageZoomerSettings settings)
      {
         var newPosition = new Point(settings.ImagePosition.X + delta.X, settings.ImagePosition.Y + delta.Y);

         if (newPosition.X > 0) newPosition.X = 0;
         if (newPosition.Y > 0) newPosition.Y = 0;

         if ((image.ActualWidth * settings.TotalImageScale) + newPosition.X < image.ActualWidth)
            newPosition.X = image.ActualWidth - (image.ActualWidth * settings.TotalImageScale);

         if ((image.ActualHeight * settings.TotalImageScale) + newPosition.Y < image.ActualHeight)
            newPosition.Y = image.ActualHeight - (image.ActualHeight * settings.TotalImageScale);

         settings.ImagePosition = newPosition;

         ApplyPosition(image, settings);
      }

      /// <summary>
      /// Resets the zoom to its original scale and position
      /// </summary>
      private static void ResetImagePosition(Image image, ImageZoomerSettings settings)
      {
         settings.TotalImageScale = 1;
         settings.ImagePosition = new Point(0, 0);
         ApplyScale(image, settings);
         ApplyPosition(image, settings);
      }

      /// <summary>
      /// Checks that dragging by the given amount won't result in empty space around the image
      /// </summary>
      private static bool IsDragValid(double scaleDelta, Point translateDelta, Image image, ImageZoomerSettings settings, out bool bounce)
      {
         if (settings.ImagePosition.X + translateDelta.X < image.ActualWidth * 0.33 || settings.ImagePosition.Y + translateDelta.Y < image.ActualHeight * 0.33)
         {
            bounce = true;
            return true;
         }
         if ((image.ActualWidth * settings.TotalImageScale * scaleDelta) + (settings.ImagePosition.X + translateDelta.X) < image.ActualWidth * 1.33)
         {
            bounce = true;
            return true;
         }
         if ((image.ActualHeight * settings.TotalImageScale * scaleDelta) + (settings.ImagePosition.Y + translateDelta.Y) < image.ActualHeight * 1.33)
         {
            bounce = true;
            return true;
         }

         if ((image.ActualWidth * settings.TotalImageScale * scaleDelta) + (settings.ImagePosition.X + translateDelta.X) < image.ActualWidth)
         {
            bounce = false;
            return false;
         }
         if ((image.ActualHeight * settings.TotalImageScale * scaleDelta) + (settings.ImagePosition.Y + translateDelta.Y) < image.ActualHeight)
         {
            bounce = false;
            return false;
         }

         bounce = false;
         return true;
      }

      /// <summary>
      /// Tells if the scaling is inside the desired range
      /// </summary>
      private static bool IsScaleValid(double scaleDelta, ImageZoomerSettings settings)
      {
         return (settings.TotalImageScale * scaleDelta >= 1) && (settings.TotalImageScale * scaleDelta <= settings.MAX_IMAGE_ZOOM);
      }

      public static bool GetIsZoomingEnabled(DependencyObject source) 
      {
         return (bool)source.GetValue(IsZoomingEnabledProperty); 
      }

      public static void SetIsZoomingEnabled(DependencyObject source, bool value)
      {
         source.SetValue(IsZoomingEnabledProperty, value);
      }

      public static readonly DependencyProperty IsZoomingEnabledProperty = DependencyProperty.RegisterAttached("IsZoomingEnabled", typeof(bool), typeof(WP7ImageZoomer), new PropertyMetadata(false, OnIsZoomingEnabledChanged));
   }
}
