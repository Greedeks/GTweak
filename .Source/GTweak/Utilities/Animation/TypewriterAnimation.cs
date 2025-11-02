using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace GTweak.Utilities.Animation
{
    internal sealed class TypewriterAnimation
    {
        private static EventHandler handler = null;

        internal static void Create(in string textToAnimate, in TextBlock textBlock, in TimeSpan timeSpan)
        {
            if (textBlock == null || string.IsNullOrEmpty(textToAnimate))
            {
                return;
            }

            Storyboard storyboard = new Storyboard { FillBehavior = FillBehavior.HoldEnd };

            StringAnimationUsingKeyFrames stringAnimation = CreateStringAnimation(textToAnimate, timeSpan);
            Storyboard.SetTarget(stringAnimation, textBlock);
            Storyboard.SetTargetProperty(stringAnimation, new PropertyPath(TextBlock.TextProperty));
            storyboard.Children.Add(stringAnimation);

            DoubleAnimation opacityAnimation = FactoryAnimation.CreateIn(0, 1, timeSpan.TotalSeconds);
            Storyboard.SetTarget(opacityAnimation, textBlock);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(opacityAnimation);

            handler = (s, e) =>
            {
                storyboard.Children.Clear();
                storyboard.Completed -= handler;
            };
            storyboard.Completed += handler;

            textBlock.BeginStoryboard(storyboard);
        }

        private static StringAnimationUsingKeyFrames CreateStringAnimation(in string textToAnimate, in TimeSpan timeSpan)
        {
            StringAnimationUsingKeyFrames stringAnimation = new StringAnimationUsingKeyFrames
            {
                Duration = new Duration(timeSpan)
            };

            StringBuilder temp = new StringBuilder();
            int totalChars = textToAnimate.Length;

            if (totalChars != 0)
            {
                double millisecondsPerChar = timeSpan.TotalMilliseconds / totalChars;

                for (int i = 0; i < totalChars; i++)
                {
                    temp.Append(textToAnimate[i]);
                    TimeSpan currentTime = TimeSpan.FromMilliseconds(millisecondsPerChar * (i + 1));

                    DiscreteStringKeyFrame keyFrame = new DiscreteStringKeyFrame
                    {
                        KeyTime = KeyTime.FromTimeSpan(currentTime),
                        Value = temp.ToString()
                    };

                    stringAnimation.KeyFrames.Add(keyFrame);
                }

                Timeline.SetDesiredFrameRate(stringAnimation, 60);
                return stringAnimation;
            }

            return stringAnimation;
        }
    }
}