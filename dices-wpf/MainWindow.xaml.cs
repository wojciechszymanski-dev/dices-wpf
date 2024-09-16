using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace dices_wpf
{
    public partial class MainWindow : Window
    {
        private List<int> currentDice = new List<int>();
        private Random random = new Random();
        private int numberOfSides = 6;
        private static readonly List<SolidColorBrush> BorderColors = new List<SolidColorBrush>
        {
            Brushes.Red, Brushes.Blue, Brushes.Green, Brushes.Purple, Brushes.Orange
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SetGame_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(NumberOfDiceInput.Text, out int diceCount) &&
                diceCount >= 3 && diceCount <= 10 &&
                int.TryParse(NumberOfSidesInput.Text, out int sidesCount) &&
                sidesCount >= 6 && sidesCount <= 20)
            {
                currentDice = new List<int>(diceCount);
                numberOfSides = sidesCount;
                DiceContainer.ItemsSource = null;
                ScoreDisplay.Text = "0";
                RollDiceButton.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Please enter a valid number of dice (3-10) and sides (6-20).", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RollDice_Click(object sender, RoutedEventArgs e)
        {
            currentDice.Clear();
            for (int i = 0; i < currentDice.Capacity; i++)
            {
                currentDice.Add(random.Next(1, numberOfSides + 1));
            }

            var groupedDice = currentDice.GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());
            var colorIndex = 0;
            var borderColors = new Dictionary<int, SolidColorBrush>();

            foreach (var group in groupedDice.Where(g => g.Value >= 2))
            {
                borderColors[group.Key] = BorderColors[colorIndex % BorderColors.Count];
                colorIndex++;
            }

            var diceWithBorders = new List<DiceDisplay>();

            var diceToDisplay = GroupDiceCheckBox.IsChecked == true
                ? currentDice.OrderBy(d => d).ToList()
                : currentDice;

            foreach (var value in diceToDisplay)
            {
                diceWithBorders.Add(new DiceDisplay
                {
                    DiceImage = CreateDiceImage(value),
                    BorderColor = borderColors.ContainsKey(value) ? borderColors[value] : Brushes.Transparent
                });
            }

            DiceContainer.ItemsSource = diceWithBorders;
            CalculateScore();
        }

        private ImageSource CreateDiceImage(int value)
        {
            var drawingGroup = new DrawingGroup();
            using (var drawingContext = drawingGroup.Open())
            {
                // Draw die face
                drawingContext.DrawRectangle(Brushes.White, new Pen(Brushes.Black, 2), new Rect(0, 0, 100, 100));

                // Draw dots
                var dotPositions = GetDotPositions(value);
                foreach (var position in dotPositions)
                {
                    drawingContext.DrawEllipse(Brushes.Black, null, position, 5, 5);
                }

                // Draw number in the center
                var formattedText = new FormattedText(
                    value.ToString(),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    20,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                var textX = (100 - formattedText.Width) / 2;
                var textY = (100 - formattedText.Height) / 2;
                drawingContext.DrawText(formattedText, new Point(textX, textY));
            }

            return new DrawingImage(drawingGroup);
        }

        private Point[] GetDotPositions(int value)
        {
            var center = new Point(50, 50);
            var radius = 35.0; // Increased radius to make room for the number
            var points = new List<Point>();

            for (int i = 0; i < value; i++)
            {
                double angle = 2 * Math.PI * i / value;
                double x = center.X + radius * Math.Cos(angle);
                double y = center.Y + radius * Math.Sin(angle);
                points.Add(new Point(x, y));
            }

            return points.ToArray();
        }

        private void CalculateScore()
        {
            var score = currentDice.GroupBy(d => d)
                                   .Where(g => g.Count() >= 2)
                                   .Sum(g => g.Key * g.Count());
            ScoreDisplay.Text = score.ToString();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            NumberOfDiceInput.Text = "";
            NumberOfSidesInput.Text = "";
            currentDice.Clear();
            DiceContainer.ItemsSource = null;
            ScoreDisplay.Text = "0";
            RollDiceButton.IsEnabled = false;
        }
    }

    public class DiceDisplay
    {
        public ImageSource DiceImage { get; set; }
        public Brush BorderColor { get; set; }
    }
}