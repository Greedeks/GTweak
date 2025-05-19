using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace GTweak.Utilities.Helpers.Animation
{
    internal sealed class TypewriterAnimation
    {
        internal TypewriterAnimation(in string textToAnimate, in TextBlock textBlock, in TimeSpan timeSpan)
        {
            if (!(textBlock.FindName(textBlock.Name) is TextBlock))
                return;

            Storyboard storyBoard = new Storyboard
            {
                FillBehavior = FillBehavior.HoldEnd
            };

            StringAnimationUsingKeyFrames stringAnimation = StringAnimation(textToAnimate, timeSpan);

            Storyboard.SetTargetName(stringAnimation, textBlock.Name);
            Storyboard.SetTargetProperty(stringAnimation, new PropertyPath(TextBlock.TextProperty));
            storyBoard.Children.Add(stringAnimation);

            DoubleAnimation opacityAnimation = FadeAnimation.FadeIn(1, timeSpan.TotalSeconds);
            Storyboard.SetTargetName(opacityAnimation, textBlock.Name);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
            storyBoard.Children.Add(opacityAnimation);

            textBlock.BeginStoryboard(storyBoard);
            storyBoard.Remove(textBlock);
        }

        private StringAnimationUsingKeyFrames StringAnimation(in string textToAnimate, in TimeSpan timeSpan)
        {
            StringAnimationUsingKeyFrames stringAnimation = new StringAnimationUsingKeyFrames
            {
                Duration = new Duration(timeSpan)
            };

            string temp = string.Empty;
            foreach (char data in textToAnimate)
            {
                temp += data;
                DiscreteStringKeyFrame keyFrame = new DiscreteStringKeyFrame
                {
                    KeyTime = KeyTime.Paced,
                    Value = temp
                };
                stringAnimation.KeyFrames.Add(keyFrame);
            }

            Timeline.SetDesiredFrameRate(stringAnimation, 240);
            return stringAnimation;
        }
    }
}
