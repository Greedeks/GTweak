using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace GTweak.Assets.ShaderEffects
{
    public sealed class ColorWheelEffect : ShaderEffect
    {
        protected override Freezable CreateInstanceCore() => new ColorWheelEffect();

        internal static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty("Input", typeof(ColorWheelEffect), 0);

        internal Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        private static readonly PixelShader _shader = new PixelShader()
        {
            UriSource = new Uri("/GTweak;component/Assets/ShaderEffects/ColorWheelEffect.ps", UriKind.Relative)
        };

        public ColorWheelEffect()
        {
            PixelShader = _shader;
            UpdateShaderValue(InputProperty);
        }
    }
}
