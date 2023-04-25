using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Bouncing_Balls_WPF.Models;

namespace Bouncing_Balls_WPF.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    internal Ellipse? Ball           { get; set; }
    internal Vector   Velocity       { get; set; } = new (0, 0);
    internal Vector   Gravity        { get; set; } = new(0, 0.0001);
    internal Vector   NormalForce    { get; set; } = new(0, 0);


    internal Path? Curve { get; set; }
    internal ParametricCurve? ParametricCurve { get; set; }

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