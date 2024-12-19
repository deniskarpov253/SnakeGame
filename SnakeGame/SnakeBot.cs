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
    public class SnakeBot
    {
        public List<Point> Body { get; private set; }
        public Direction CurrentDirection { get; set; }
        private Point foodPosition;
        private Random random = new Random();
        public SnakeBot()
        {
            Body = new List<Point> { new Point(5, 5) }; // Начальная позиция
            CurrentDirection = Direction.Right;
        }
        public void UpdateFoodPosition(Point food) => foodPosition = food;
        public void Move(int fieldWidth, int fieldHeight, HashSet<Point> obstacles, List<Point> playerSnakeBody)
        {
            Point head = Body[0];
            Point newHead = head;

            // Вычисляем направление к еде
            Direction desiredDirection = CurrentDirection;

            if (foodPosition.X > head.X) desiredDirection = Direction.Right;
            else if (foodPosition.X < head.X) desiredDirection = Direction.Left;
            else if (foodPosition.Y > head.Y) desiredDirection = Direction.Down;
            else if (foodPosition.Y < head.Y) desiredDirection = Direction.Up;

            // Пробуем двигаться в желаемом направлении
            List<Direction> possibleDirections = new List<Direction>
            {
                desiredDirection,
                Direction.Up, Direction.Down, Direction.Left, Direction.Right
            };

            foreach (var direction in possibleDirections.Distinct())
            {
                Point testHead = GetNewHeadPosition(head, direction, fieldWidth, fieldHeight);

                // Проверяем на препятствия, тело бота и тело игрока
                if (!obstacles.Contains(testHead) &&
                    !Body.Contains(testHead) &&
                    !playerSnakeBody.Contains(testHead))
                {
                    newHead = testHead;
                    CurrentDirection = direction;
                    break;
                }
            }

            Body.Insert(0, newHead);
            Body.RemoveAt(Body.Count - 1);
        }

        private Point GetNewHeadPosition(Point head, Direction direction, int fieldWidth, int fieldHeight)
        {
            Point newHead = head;
                
            switch (direction)
            {
                case Direction.Up: newHead.Y--; break;
                case Direction.Down: newHead.Y++; break;
                case Direction.Left: newHead.X--; break;
                case Direction.Right: newHead.X++; break;
            }

            // Циклическое поле
            newHead.X = (newHead.X + fieldWidth) % fieldWidth;
            newHead.Y = (newHead.Y + fieldHeight) % fieldHeight;

            return newHead;
        }

        public bool CheckCollisionWithFood(Point food) => Body[0] == food;
        public void Grow() => Body.Add(Body[Body.Count - 1]); // Добавляем копию последнего сегмента
        public void Draw(Canvas canvas, int cellSize)
        {
            foreach (var segment in Body)
            {
                Rectangle rect = new Rectangle
                {
                    Width = cellSize,
                    Height = cellSize,
                    Fill = Brushes.DarkOliveGreen
                };
                Canvas.SetLeft(rect, segment.X * cellSize);
                Canvas.SetTop(rect, segment.Y * cellSize);
                canvas.Children.Add(rect);
            }
        }
    }

}
