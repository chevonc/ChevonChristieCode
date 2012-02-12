using System;
using System.Threading;

namespace ChevonChristieCode.Misc
{

   /// <summary>
   /// A timer modeled off silverlight's DispatcherTimer. It behaves the same and can be setup just as easily!
   /// </summary>
   public class CustomTimer
   {
      private readonly static TimeSpan PreventStart = TimeSpan.FromMilliseconds(-1);
      private readonly static TimeSpan StartNow = TimeSpan.FromMilliseconds(0);
      
      private TimeSpan m_Interval;
      private readonly object m_State;
      private bool isRunning;
      Timer m_Timer;
      public event Action<object> Tick;

      /// <summary>
      /// Gets or sets the interval.
      /// </summary>
      /// <value>
      /// The interval.
      /// </value>
      public TimeSpan Interval
      {
         get
         {
            return m_Interval;
         }
         set
         {
            if (value == m_Interval && m_Timer != null)
               return;

            m_Interval = value;

            if (m_Timer == null)
               InitializeTimer(m_Interval, null);
            else
               ChangeInterval(m_Interval);
         }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="CustomTimer"/> class.
      /// </summary>
      public CustomTimer()
      {

      }

      /// <summary>
      /// Initializes a new instance of the <see cref="CustomTimer"/> class.
      /// </summary>
      /// <param name="time">The time.</param>
      /// <param name="state">The state.</param>
      public CustomTimer(TimeSpan time, object state)
      {
         Interval = time;
         m_State = state;
         InitializeTimer(time, state);
      }

      /// <summary>
      /// Initializes the timer.
      /// </summary>
      /// <param name="time">The time.</param>
      /// <param name="state">The state.</param>
      private void InitializeTimer(TimeSpan time, object state)
      {
         m_Timer = new Timer(InternalTimerTicked, state, CustomTimer.PreventStart, time);
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="CustomTimer"/> class.
      /// </summary>
      /// <param name="milliseconds">The milliseconds.</param>
      /// <param name="state">The state.</param>
      public CustomTimer(int milliseconds, object state)
         : this(TimeSpan.FromMilliseconds(milliseconds), state)
      {

      }

      /// <summary>
      /// Changes the interval.
      /// </summary>
      /// <param name="newInterval">The new interval.</param>
      public void ChangeInterval(TimeSpan newInterval)
      {
         m_Interval = newInterval;
         m_Timer.Change(isRunning ? CustomTimer.StartNow : CustomTimer.PreventStart, m_Interval);
      }

      /// <summary>
      /// Internals the timer ticked.
      /// </summary>
      /// <param name="state">The state.</param>
      private void InternalTimerTicked(object state)
      {
         var shadow = Tick;

         if (shadow != null)
            shadow(state);
      }


      /// <summary>
      /// Starts this instance.
      /// </summary>
      public void Start()
      {
         if (!isRunning)
         {
            m_Timer.Change(CustomTimer.StartNow, Interval);
            isRunning = true;
         }
      }

      /// <summary>
      /// Stops this instance.
      /// </summary>
      public void Stop()
      {
         if (isRunning)
         {
            m_Timer.Change(CustomTimer.PreventStart, Interval);
            isRunning = false;
         }
      }

      /// <summary>
      /// Starts the or stop.
      /// </summary>
      /// <param name="shouldStart">if set to <c>true</c> [should start].</param>
      public void StartOrStop(bool shouldStart)
      {
         if((isRunning && shouldStart) || (!isRunning && !shouldStart))
            return;

         if(!shouldStart && isRunning)
            Stop();

         if (shouldStart && !isRunning)
            Start();
      }
   }
}
