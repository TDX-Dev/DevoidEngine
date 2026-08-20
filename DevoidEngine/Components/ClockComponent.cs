using DevoidEngine.Core;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.Components
{
    public class ClockComponent : Component
    {
        public override string Type => nameof(ClockComponent);

        // Assign these in the editor.
        public GameObject? HourHand;
        public GameObject? MinuteHand;
        public GameObject? SecondHand;

        public float DegreesPerMinute = 5.2f;

        private float hourInitialRotation;
        private float minuteInitialRotation;
        private float secondInitialRotation;

        public override void OnStart()
        {
            hourInitialRotation =
                HourHand?.Transform.EulerAngles.Z ?? 0;

            minuteInitialRotation =
                MinuteHand?.Transform.EulerAngles.Z ?? 0;

            secondInitialRotation =
                SecondHand?.Transform.EulerAngles.Z ?? 0;

            UpdateClock();
        }

        public override void OnUpdate(float dt)
        {
            UpdateClock();
        }

        private void UpdateClock()
        {
            DateTime now = DateTime.Now;

            float minute =
                now.Minute +
                now.Second / 60f;

            float second =
                now.Second +
                now.Millisecond / 1000f;

            // Minute hand
            float minuteRotation =
                minute * DegreesPerMinute;

            // Hour hand moves continuously with the minutes.
            float hourRotation =
                ((now.Hour % 12) + minute / 60f) *
                (DegreesPerMinute / 60f);

            // Second hand
            //float secondRotation = second * (DegreesPerMinute / 60f);

            SetZRotation(
                HourHand,
                hourInitialRotation - hourRotation);

            SetZRotation(
                MinuteHand,
                minuteInitialRotation - minuteRotation);

            SetZRotation(
                SecondHand,
                second * 10);
        }

        private static void SetZRotation(
            GameObject? hand,
            float rotation)
        {
            if (hand == null)
                return;

            Vector3 euler =
                hand.Transform.EulerAngles;

            euler.Z = rotation;

            hand.Transform.EulerAngles = euler;
        }
    }
}