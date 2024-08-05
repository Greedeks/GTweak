using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace GTweak.Utilities.Helpers
{
    internal readonly struct WorkWithText
    {
        internal static void TypeWriteAnimation(string textToAnimate, TextBlock _textBlock, TimeSpan _timeSpan)
        {
            if (_textBlock.FindName(_textBlock.Name) is TextBlock)
            {
                Storyboard storyBoard = new Storyboard()
                {
                    FillBehavior = FillBehavior.HoldEnd
                };

                StringAnimationUsingKeyFrames stringAnimationUsingKeyFrames = new StringAnimationUsingKeyFrames()
                {
                    Duration = new Duration(_timeSpan)
                };

                string _temp = string.Empty;
                foreach (char _char in textToAnimate)
                {
                    DiscreteStringKeyFrame discreteStringKeyFrame = new DiscreteStringKeyFrame
                    {
                        KeyTime = KeyTime.Paced
                    };
                    _temp += _char;
                    discreteStringKeyFrame.Value = _temp;
                    stringAnimationUsingKeyFrames.KeyFrames.Add(discreteStringKeyFrame);
                }
                
                Storyboard.SetTargetName(stringAnimationUsingKeyFrames, _textBlock.Name);
                Storyboard.SetTargetProperty(stringAnimationUsingKeyFrames, new PropertyPath(TextBlock.TextProperty));
                storyBoard.Children.Add(stringAnimationUsingKeyFrames);
                _textBlock.BeginStoryboard(storyBoard);
                storyBoard.Remove(_textBlock);
            }
        }
    }
}
