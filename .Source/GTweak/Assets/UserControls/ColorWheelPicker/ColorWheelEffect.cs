using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace GTweak.Assets.UserControls.ColorWheelPicker
{
    public class ColorWheelEffect : ShaderEffect
    {
        private static readonly PixelShader _shader = new PixelShader()
        {
            UriSource = new Uri("pack://application:,,,/GTweak;component/Assets/UserControls/ColorWheelPicker/ColorWheelEffect.ps")
        };

        public ColorWheelEffect()
        {
            PixelShader = _shader;
            UpdateShaderValue(InputProperty);
        }

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(ColorWheelEffect), 0);

        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }
    }
}
