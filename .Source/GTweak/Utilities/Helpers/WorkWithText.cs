using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace GTweak.Utilities.Helpers
{
    internal readonly struct WorkWithText
    {
        internal static void TypeWriteAnimation(string textToAnimate, TextBlock textBlock, TimeSpan timeSpan)
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
                
            Storyboard.SetTargetName(stringAnimationUsingKeyFrames, textBlock.Name);
            Storyboard.SetTargetProperty(stringAnimationUsingKeyFrames, new PropertyPath(TextBlock.TextProperty));
            storyBoard.Children.Add(stringAnimationUsingKeyFrames);
            textBlock.BeginStoryboard(storyBoard);
            storyBoard.Remove(textBlock);
        }
    }
}
