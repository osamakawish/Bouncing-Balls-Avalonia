using System.Windows;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media;
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

    public Bounce(Color ballColor, double ballRadius, Path curve, Vector initialPosition, Vector initialVelocity, Vector gravity)
    {
        Ball = new ()
        {
            Width = 2 * ballRadius,
            Height = 2 * ballRadius,
            Fill = new SolidColorBrush(ballColor)
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
            if (IsBounce(out var normal))
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reflectionNormal">The vector normal/perpendicular to the tangent at the point of intersection of the curve.</param>
    /// <returns></returns>
    private bool IsBounce(out Vector reflectionNormal)
    {
        // Check if ball intersects the curve.

        // If it does, find the tangent at the point of intersection.

        // Find the normal to the tangent.

        return false;
    }
}