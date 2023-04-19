using System.Windows;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Bouncing_Balls_WPF.Models;

public class Bounce
{
    private  Timer   Timer    { get; } = new();
    private  Path    Curve    { get; }
    private  Ellipse Ball     { get; }

    private Vector InitialPosition { get; }
    private Vector InitialVelocity { get; }

    internal Vector  Position { get; private set; }
    internal Vector  Velocity { get; private set; }
    internal Vector  Gravity { get; }

    public Bounce(double ballRadius, Path curve, Vector initialPosition, Vector initialVelocity, Vector gravity)
    {
        Ball = new ()
        {
            Width = 2 * ballRadius,
            Height = 2 * ballRadius,
        };
        Curve = curve; Setup();
        InitialPosition = initialPosition;
        InitialVelocity = initialVelocity;
        Gravity = gravity;
    }

    public void Setup()
    {
        Timer.Elapsed += (_,  args) =>
        {
            if (IsBounce())
            {

            }
            else
            {
                Position += Velocity;
                Velocity += Gravity;
            }
            Canvas.SetLeft(Ball, Position.X);
            Canvas.SetTop(Ball, Position.Y);
        };
    }

    private bool IsBounce()
    {
        return false;
    }
}