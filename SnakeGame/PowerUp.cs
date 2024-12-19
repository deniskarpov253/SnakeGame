using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace SnakeGame
{
    public class PowerUp
    {
        public Point Position { get; set; }
        public PowerUpType Type { get; set; }
        private static Random random = new Random();
        public PowerUp(int fieldWidth, int fieldHeight)
        {
            Position = new Point(random.Next(0, fieldWidth), random.Next(0, fieldHeight));
            Type = (PowerUpType)random.Next(0, Enum.GetValues(typeof(PowerUpType)).Length);
        }

        public void Draw(Canvas canvas)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Purple
            };
            Canvas.SetLeft(ellipse, Position.X * 20);
            Canvas.SetTop(ellipse, Position.Y * 20);
            canvas.Children.Add(ellipse);
        }
    }

    public enum PowerUpType
    {
        SpeedBoost, // Ускорение
        Grow, // Увеличение длины змейки
        SlowDown, // Замедление
        ReverseDirection // Обратное направление
    }

}
