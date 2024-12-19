using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SnakeGame
{
    public class Food
    {
        private readonly Random random = new Random();
        public Point Position { get; set; }
        public Food(int maxWidth, int maxHeight, List<Point> snakeBody)
        {
            GenerateNewPosition(maxWidth, maxHeight, snakeBody);
        }
        public void GenerateNewPosition(int fieldWidth, int fieldHeight, List<Point> snakeBody)
        {
            Random random = new Random();
            Point newPosition;

            do
            {
                newPosition = new Point(random.Next(0, fieldWidth), random.Next(0, fieldHeight));
            } while (snakeBody.Contains(newPosition)); // Проверяем, чтобы еда не совпадала с телом змейки

            Position = newPosition;
        }
        public void Draw(Canvas canvas)
        {
            Rectangle rect = new Rectangle
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Red
            };
            Canvas.SetLeft(rect, Position.X * 20);
            Canvas.SetTop(rect, Position.Y * 20);
            canvas.Children.Add(rect);
        }

    }
}