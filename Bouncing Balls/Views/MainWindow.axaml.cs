using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Bouncing_Balls.ViewModels;

namespace Bouncing_Balls.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _dataContext;

    public MainWindow()
    {
        InitializeComponent();

        Initialized += delegate {
            _dataContext = (MainWindowViewModel)DataContext!;
            AddBall();
        };
    }

    public void AddBall()
    {
        var ball = new Ellipse
        {
            Width = 20,
            Height = 20,
            Fill = Avalonia.Media.Brushes.Red
        };

        Canvas.SetLeft(ball, Width / 2); Canvas.SetTop(ball, Height / 3);
        Canvas.Children.Add(ball);
    }
}