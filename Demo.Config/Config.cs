namespace Demo.Config
{
    public class Config
    {
        private Dictionary<string, string> env;
        public Config()
        {
            // Bad bad practice, but it's just a demo
            string root = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            var filePath = Path.Combine(root, ".env");
            if (!File.Exists(filePath))
                throw new FileNotFoundException();

            env = new Dictionary<string, string>();

            foreach (var line in File.ReadAllLines(filePath))
            {
                int index = line.IndexOf('=');
                if (index > 0)
                {
                    string key = line.Substring(0, index).Trim();
                    string value = line.Substring(index + 1).Trim();
                    env[key] = value;
                }
            }
        }

        public string get(string key)
        {
            return env[key];
        }
    }
}
