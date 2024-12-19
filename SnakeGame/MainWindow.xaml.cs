using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SnakeGame
{
    public partial class MainWindow : Window
    {
        // Делегаты и события
        public delegate void GameEventHandler(string message);
        public event GameEventHandler GameOverEvent;
        public event GameEventHandler FoodEatenEvent;
        public event GameEventHandler PowerUpEatenEvent;
        public event GameEventHandler ObstacleHitEvent;
        public event Action<int> ScoreChangedEvent;

        private DispatcherTimer timer;
        private Snake snake;
        private Food food;
        private PowerUp currentPowerUp;
        private DispatcherTimer powerUpTimer;
        private DateTime powerUpExpirationTime;
        private DispatcherTimer gameTimer;  // Таймер игры

        private int gameTime = 0;  // Время игры в секундах
        private int score;
        private int CellSize = 20;
        private int FieldWidth = 30;
        private int FieldHeight = 30;
        private bool IsHardMode = false;
        private bool IsBotMode = false;

        private List<HighScore> highScores = new List<HighScore>();
        private List<SnakeBot> bots;
        private List<Point> obstacles = new List<Point>();

        private void SaveHighScore(string name, string mode, int score, int width, int height)
        {
            LoadHighScores();

            highScores.Add(new HighScore
            {
                Name = name,
                Mode = mode,
                Score = score,
                FieldWidth = width,
                FieldHeight = height
            });

            highScores.Sort((x, y) => y.Score.CompareTo(x.Score));

            if (highScores.Count > 5)
                highScores = highScores.GetRange(0, 5);

            File.WriteAllText("highscores.json", JsonConvert.SerializeObject(highScores));
            UpdateHighScoresList();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "Сохранить рекорды"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(highScores, Formatting.Indented));
                MessageBox.Show("Рекорды сохранены!");
            }
        }
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "Загрузить рекорды"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string json = File.ReadAllText(openFileDialog.FileName);
                highScores = JsonConvert.DeserializeObject<List<HighScore>>(json) ?? new List<HighScore>();
                UpdateHighScoresList();
                MessageBox.Show("Рекорды загружены!");
            }
        }
        private void LoadHighScores()
        {
            if (File.Exists("highscores.json"))
            {
                string json = File.ReadAllText("highscores.json");
                highScores = JsonConvert.DeserializeObject<List<HighScore>>(json) ?? new List<HighScore>();
            }
        }
        private void UpdateHighScoresList()
        {
            HighScoresList.Items.Clear();

            foreach (var record in highScores)
            {
                HighScoresList.Items.Add(
                    $"Имя: {record.Name}, Режим: {record.Mode}, Счет: {record.Score}, Поле: {record.FieldWidth}x{record.FieldHeight}");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += Window_KeyDown;
            this.Focus();

            LoadHighScores();
            UpdateHighScoresList();

            // Подписываемся на события
            GameOverEvent += ShowEventMessage;
            FoodEatenEvent += ShowEventMessage;
            PowerUpEatenEvent += ShowEventMessage;
            ObstacleHitEvent += ShowEventMessage;
            ScoreChangedEvent += UpdateScore;
        }

        // Обработчик для вывода сообщений событий
        private void ShowEventMessage(string message) => MessageBox.Show(message);
        // Обработчик для обновления счета
        private void UpdateScore(int newScore) => ScoreText.Text = $"Счет: {newScore}";
        private void ClassicMode_Click(object sender, RoutedEventArgs e)
        {
            IsHardMode = false;
            StartGame();
        }
        private void HardMode_Click(object sender, RoutedEventArgs e)
        {
            IsHardMode = true;
            StartGame();
        }
        private void BotMode_Click(object sender, RoutedEventArgs e)
        {
            IsBotMode = true;
            StartGame();
        }

        // В методе StartGame вызываем генерацию PowerUp
        private void StartGame()
        {
            // Устанавливаем размеры поля из выбора
            FieldWidth = int.Parse(((ComboBoxItem)SizeComboBox.SelectedItem).Tag.ToString());
            FieldHeight = FieldWidth;
            // Настройка интерфейса
            MenuPanel.Visibility = Visibility.Hidden;
            GameCanvas.Visibility = Visibility.Visible;
            ScoreText.Visibility = Visibility.Visible;
            PowerUpEffectText.Visibility = Visibility.Visible;
            ScoreText.Text = $"Счет: {score}";
            // Настройка поля
            GameCanvas.Width = FieldWidth * CellSize;
            GameCanvas.Height = FieldHeight * CellSize;

            snake = new Snake();
            food = new Food(FieldWidth, FieldHeight, snake.Body);
            score = 0;
            gameTime = 0;  // Сброс времени игры
            obstacles.Clear();

            // Генерация PowerUp при начале игры
            GeneratePowerUp();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(150);
            timer.Tick += GameLoop;

            // Настроим таймер игры
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1);  // Обновление каждую секунду
            gameTimer.Tick += GameTimer_Tick;

            // Таймер для появления PowerUp
            powerUpTimer = new DispatcherTimer();
            powerUpTimer.Interval = TimeSpan.FromSeconds(20); // Появление PowerUp раз в 20 секунд
            powerUpTimer.Tick += PowerUpTimer_Tick;
            powerUpTimer.Start();

            if (IsHardMode)
                GenerateObstacles(15); // Создаем препятствия
            if (IsBotMode)
            {
                GenerateObstacles(15); // Создаем препятствия
                bots = new List<SnakeBot> { new SnakeBot() };
            }
            else
                bots = new List<SnakeBot>();

            // Начинаем таймер
            gameTimer.Start();
            timer.Start();
        }

        // Обновление таймера игры каждую секунду
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            gameTime++;
            TimeLeftText.Text = $"Время: {gameTime}s";  // Обновляем отображение времени
        }
        private void PowerUpTimer_Tick(object sender, EventArgs e) => GeneratePowerUp();

        // Изменение игрового цикла для обработки PowerUp
        private void GameLoop(object sender, EventArgs e)
        {
            Point previousHead = snake.Body[0]; // Сохраняем старое положение головы

            snake.Move(FieldWidth, FieldHeight);

            foreach (var bot in bots)
            {
                bot.UpdateFoodPosition(food.Position);
                bot.Move(FieldWidth, FieldHeight, new HashSet<Point>(obstacles), snake.Body);

                if (bot.CheckCollisionWithFood(food.Position))
                {
                    bot.Grow();
                    food.GenerateNewPosition(FieldWidth, FieldHeight, snake.Body);
                }

                // Проверка на столкновение игрока с ботом
                if (bot.Body.Any(segment => segment == snake.Body[0]))
                {
                    GameOver();
                    return;
                }
            }


            // Проверка столкновений
            if (snake.CheckSelfCollision())
            {
                GameOver();
                return;
            }

            if (snake.Body[0] == food.Position)
            {
                snake.Grow();
                food.GenerateNewPosition(FieldWidth, FieldHeight, snake.Body);
                score += 10;
                // Вызываем событие при изменении счета и поедании еды
                ScoreChangedEvent?.Invoke(score);
                FoodEatenEvent?.Invoke("Змейка съела еду!");

                ScoreText.Text = $"Счет: {score}";
            }

            // Проверка на столкновение с PowerUp
            if (currentPowerUp != null && DateTime.Now > powerUpExpirationTime)
            {
                // Если время жизни PowerUp истекло, удаляем его
                currentPowerUp = null;
                PowerUpEffectText.Text = "";
            }
            else
                CheckPowerUp();
            // Проверка на столкновение с препятствиями
            if (IsHardMode && obstacles.Contains(snake.Body[0]) || IsBotMode && obstacles.Contains(snake.Body[0]))
            {
                ObstacleHitEvent?.Invoke("Змейка столкнулась с препятствием!");
                GameOver();
                return;
            }
            DrawGame();
        }
        private void DrawGame()
        {
            GameCanvas.Children.Clear();
            snake.Draw(GameCanvas);
            food.Draw(GameCanvas);
            foreach (var bot in bots)
                bot.Draw(GameCanvas, CellSize);
            if (currentPowerUp != null)
                currentPowerUp.Draw(GameCanvas);

            if (IsHardMode || IsBotMode)
                DrawObstacles();
        }
        private void DrawRectangle(Point position, Brush color)
        {
            Rectangle rectangle = new Rectangle
            {
                Width = CellSize,
                Height = CellSize,
                Fill = color
            };

            Canvas.SetLeft(rectangle, position.X * CellSize);
            Canvas.SetTop(rectangle, position.Y * CellSize);
            GameCanvas.Children.Add(rectangle);
        }

        private void GameOver()
        {
            timer.Stop();
            gameTimer.Stop();
            string mode = IsHardMode ? "Усложненный" : "Классический";

            string playerName = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите ваше имя:",
                "Игра окончена",
                "Игрок"
            );

            SaveHighScore(playerName, mode, score, FieldWidth, FieldHeight);

            // Вызываем событие
            GameOverEvent?.Invoke($"Игра окончена! Счет: {score}");

            MessageBox.Show($"Игра окончена! Ваш счет: {score}");
            ShowMenu();
        }
        private void ShowMenu()
        {
            MenuPanel.Visibility = Visibility.Visible;
            GameCanvas.Visibility = Visibility.Hidden;
            ScoreText.Visibility = Visibility.Hidden;
            TimeLeftText.Visibility = Visibility.Hidden;
            HighScoresList.Visibility = Visibility.Visible;

            UpdateHighScoresList();
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up when snake.CurrentDirection != Direction.Down:
                    snake.CurrentDirection = Direction.Up; break;
                case Key.Down when snake.CurrentDirection != Direction.Up:
                    snake.CurrentDirection = Direction.Down; break;
                case Key.Left when snake.CurrentDirection != Direction.Right:
                    snake.CurrentDirection = Direction.Left; break;
                case Key.Right when snake.CurrentDirection != Direction.Left:
                    snake.CurrentDirection = Direction.Right; break;
            }
        }
        private void GenerateObstacles(int count)
        {
            obstacles.Clear();
            Random random = new Random();

            for (int i = 0; i < count; i++)
            {
                Point obstacle;
                do
                {
                    obstacle = new Point(random.Next(0, FieldWidth), random.Next(0, FieldHeight));
                } while (snake.Body.Contains(obstacle) || food.Position == obstacle);

                obstacles.Add(obstacle);
            }
        }
        private void DrawObstacles()
        {
            foreach (var obstacle in obstacles)
                DrawRectangle(obstacle, Brushes.Gray);
        }

        // Метод для генерации PowerUp с временем жизни от 10 до 15 секунд
        private void GeneratePowerUp()
        {
            currentPowerUp = new PowerUp(FieldWidth, FieldHeight);
            // Убедимся, что PowerUp не появляется на месте змейки или еды
            while (snake.Body.Contains(currentPowerUp.Position) || food.Position == currentPowerUp.Position)
                currentPowerUp = new PowerUp(FieldWidth, FieldHeight);
            // Случайное время жизни от 10 до 15 секунд
            powerUpExpirationTime = DateTime.Now.AddSeconds(new Random().Next(10, 16));
        }

        // Метод для проверки, съела ли змейка PowerUp
        private void CheckPowerUp()
        {
            // Проверяем, что currentPowerUp не равен null
            if (currentPowerUp != null && snake.Body[0] == currentPowerUp.Position)
            {
                ApplyPowerUp(currentPowerUp);
                currentPowerUp = null; // Удаляем PowerUp после съедения
                PowerUpEatenEvent?.Invoke("Змейка съела PowerUp!");
            }
        }

        // Метод для применения эффекта PowerUp
        private void ApplyPowerUp(PowerUp powerUp)
        {
            switch (powerUp.Type)
            {
                case PowerUpType.SpeedBoost:
                    timer.Interval = TimeSpan.FromMilliseconds(100); // Ускоряем игру
                    PowerUpEffectText.Text = "Эффект: Ускорение!";
                    break;
                case PowerUpType.Grow:
                    snake.Grow(); // Увеличиваем змейку
                    PowerUpEffectText.Text = "Эффект: Увеличение!";
                    break;
                case PowerUpType.SlowDown:
                    timer.Interval = TimeSpan.FromMilliseconds(250); // Замедляем игру
                    PowerUpEffectText.Text = "Эффект: Замедление!";
                    break;
            }

            // Сбросить текст эффекта через несколько секунд (по истечению времени жизни PowerUp)
            var powerUpDuration = new Random().Next(10, 16); // Примерное время жизни PowerUp
            DispatcherTimer resetEffectTimer = new DispatcherTimer();
            resetEffectTimer.Interval = TimeSpan.FromSeconds(powerUpDuration);
            resetEffectTimer.Tick += (s, args) =>
            {
                PowerUpEffectText.Text = "";
                resetEffectTimer.Stop();
            };
            resetEffectTimer.Start();
        }
        private void ClearHighScores_Click(object sender, RoutedEventArgs e)
        {
            highScores.Clear();
            File.WriteAllText("highscores.json", JsonConvert.SerializeObject(highScores)); // Очистим файл
            UpdateHighScoresList(); // Обновим список на экране
            MessageBox.Show("Рекорды очищены!");
        }
    }
}
