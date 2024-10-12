using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace GTweak.Utilities.Helpers
{
    internal sealed class TypewriterAnimation
    {
        internal TypewriterAnimation(string textToAnimate, TextBlock textBlock, TimeSpan timeSpan)
        {
            if (!(textBlock.FindName(textBlock.Name) is TextBlock)) return;
            Storyboard storyBoard = new Storyboard()
            {
                FillBehavior = FillBehavior.HoldEnd
            };

            StringAnimationUsingKeyFrames stringAnimationUsingKeyFrames = new StringAnimationUsingKeyFrames()
            {
                Duration = new Duration(timeSpan)
            };

            string temp = string.Empty;
            foreach (char data in textToAnimate)
            {
                DiscreteStringKeyFrame discreteStringKeyFrame = new DiscreteStringKeyFrame
                {
                    KeyTime = KeyTime.Paced
                };
                temp += data;
                discreteStringKeyFrame.Value = temp;
                stringAnimationUsingKeyFrames.KeyFrames.Add(discreteStringKeyFrame);
            }

            Timeline.SetDesiredFrameRate(stringAnimationUsingKeyFrames, 400);
            Storyboard.SetTargetName(stringAnimationUsingKeyFrames, textBlock.Name);
            Storyboard.SetTargetProperty(stringAnimationUsingKeyFrames, new PropertyPath(TextBlock.TextProperty));
            storyBoard.Children.Add(stringAnimationUsingKeyFrames);

            DoubleAnimation doubleAnim = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = new Duration(timeSpan),
                EasingFunction = new QuadraticEase()
            };

            Timeline.SetDesiredFrameRate(doubleAnim, 400);
            Storyboard.SetTargetName(doubleAnim, textBlock.Name);
            Storyboard.SetTargetProperty(doubleAnim, new PropertyPath(UIElement.OpacityProperty));
            storyBoard.Children.Add(doubleAnim);

            textBlock.BeginStoryboard(storyBoard);
            storyBoard.Remove(textBlock);
        }
    }
}
