using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using ChevonChristieCode.Media;

namespace ChevonChristieCode.Samples
{
   public partial class MainPage : PhoneApplicationPage
   {
      // Constructor
      public MainPage()
      {
         InitializeComponent();
      }

      protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
      {
         base.OnNavigatedFrom(e);

         if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
         {
            //WP7ImageZoomer.SetIsZoomingEnabled(myImage, false);
            //OR
            WP7ImageZoomer.EndImageZooming(myImage); //Unhooks events and frees image reference to its memory can be released
         }
      }
   }
}