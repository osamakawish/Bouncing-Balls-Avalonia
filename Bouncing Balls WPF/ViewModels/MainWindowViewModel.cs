using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Bouncing_Balls_WPF.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private Path Curve { get; } = new();

    private List<Ellipse> Balls { get; } = new();

    public void SetBalls(Color initialColor, Color finalColor, int count)
    {
        Balls.Clear();

        var max = count - 1;

        for (var i = 0; i <= max; i++) {
            byte GetColorByte(Func<Color, byte> colorAxis)
                => (byte)(colorAxis(initialColor) + (byte)((colorAxis(finalColor) - colorAxis(initialColor)) * (double)i / max));

            Brush ballSpecificBrush = new SolidColorBrush(
                Color.FromRgb(
                    GetColorByte(color => color.R),
                    GetColorByte(color => color.G), 
                    GetColorByte(color => color.B)));

            Balls.Add(new() { Width = 6, Height = 6, Fill = ballSpecificBrush });
        }
    }

    public IReadOnlyCollection<Ellipse> GetBalls() => Balls;
}