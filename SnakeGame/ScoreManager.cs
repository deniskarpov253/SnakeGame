using System.IO;
using System.Text.Json;

namespace SnakeGame
{
    public class ScoreManager
    {
        private const string FilePath = "highscores.json";
        public int ClassicModeRecord { get; set; }
        public int HardModeRecord { get; set; }

        public void Save()
        {
            var json = JsonSerializer.Serialize(this);
            File.WriteAllText(FilePath, json);
        }

        public static ScoreManager Load()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<ScoreManager>(json);
            }
            return new ScoreManager();
        }
    }
    public class HighScore
    {
        public string Name { get; set; }
        public string Mode { get; set; }
        public int Score { get; set; }
        public int FieldWidth { get; set; }
        public int FieldHeight { get; set; }
    }
}