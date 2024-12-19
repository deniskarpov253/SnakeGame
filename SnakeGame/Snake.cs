using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SnakeGame
{
    public class Snake
    {
        public List<Point> Body { get; set; } // Координаты сегментов змейки
        public Direction CurrentDirection { get; set; } // Текущее направление движения змейки
        public Snake()
        {
            Body = new List<Point>
            {
                new Point(5, 5) // Начальное положение змейки
            };
            CurrentDirection = Direction.Right;
        }
        public void Move(int fieldWidth, int fieldHeight)
        {
            Point head = Body[0];
            Point newHead = head;

            switch (CurrentDirection)
            {
                case Direction.Up: newHead.Y--; break;
                case Direction.Down: newHead.Y++; break;
                case Direction.Left: newHead.X--; break;
                case Direction.Right: newHead.X++; break;
            }

            // Прохождение через границы
            if (newHead.X < 0) newHead.X = fieldWidth - 1;
            if (newHead.Y < 0) newHead.Y = fieldHeight - 1;
            if (newHead.X >= fieldWidth) newHead.X = 0;
            if (newHead.Y >= fieldHeight) newHead.Y = 0;

            Body.Insert(0, newHead);
            Body.RemoveAt(Body.Count - 1);
        }
        public bool CheckSelfCollision()
        {
            for (int i = 1; i < Body.Count; i++)
            {
                if (Body[0] == Body[i]) return true;
            }
            return false;
        }
        public void Grow() => Body.Add(Body[Body.Count - 1]);
        public void Draw(Canvas canvas)
        {
            for (int i = 0; i < Body.Count; i++)
            {
                Rectangle rect = new Rectangle
                {
                    Width = 20,
                    Height = 20,
                    Fill = (i == 0) ? Brushes.DarkGreen : Brushes.Green // Голова будет темно-зеленой
                };
                Canvas.SetLeft(rect, Body[i].X * 20);
                Canvas.SetTop(rect, Body[i].Y * 20);
                canvas.Children.Add(rect);
            }
        }
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}