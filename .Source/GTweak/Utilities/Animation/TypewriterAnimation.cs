using System;
using System.Globalization;
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
            if (stringAnimation != null)
            {
                Storyboard.SetTarget(stringAnimation, textBlock);
                Storyboard.SetTargetProperty(stringAnimation, new PropertyPath(TextBlock.TextProperty));
                storyboard.Children.Add(stringAnimation);

                DoubleAnimation opacityAnimation = FactoryAnimation.CreateIn(0, 1, timeSpan.TotalSeconds);
                if (opacityAnimation != null)
                {
                    Storyboard.SetTarget(opacityAnimation, textBlock);
                    Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
                    storyboard.Children.Add(opacityAnimation);
                }

                handler = delegate
                {
                    storyboard.Children.Clear();
                    storyboard.Completed -= handler;
                };
                storyboard.Completed += handler;
                textBlock.BeginStoryboard(storyboard);
            }
        }



        private static StringAnimationUsingKeyFrames CreateStringAnimation(in string textToAnimate, in TimeSpan timeSpan)
        {
            StringAnimationUsingKeyFrames stringAnimation = new StringAnimationUsingKeyFrames
            {
                Duration = new Duration(timeSpan)
            };

            if (string.IsNullOrEmpty(textToAnimate))
            {
                return stringAnimation;
            }

            StringInfo stringInfo = new StringInfo(textToAnimate);
            int totalElements = stringInfo.LengthInTextElements;

            if (totalElements > 0)
            {
                double millisecondsPerElement = timeSpan.TotalMilliseconds / totalElements;
                StringBuilder temp = new StringBuilder();

                TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(textToAnimate);
                int index = 0;

                while (enumerator.MoveNext())
                {
                    temp.Append(enumerator.GetTextElement());
                    TimeSpan currentTime = TimeSpan.FromMilliseconds(millisecondsPerElement * (index + 1));

                    DiscreteStringKeyFrame keyFrame = new DiscreteStringKeyFrame
                    {
                        KeyTime = KeyTime.FromTimeSpan(currentTime),
                        Value = temp.ToString()
                    };

                    stringAnimation.KeyFrames.Add(keyFrame);
                    index++;
                }

                Timeline.SetDesiredFrameRate(stringAnimation, 60);
            }

            return stringAnimation;
        }
    }
}
