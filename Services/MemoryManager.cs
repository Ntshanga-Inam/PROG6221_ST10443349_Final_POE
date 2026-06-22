using System.Collections.Generic;

namespace CybersecurityChatbot_Part2.Services
{
    public class MemoryManager
    {
        private string _userName;
        private string _favoriteTopic;
        private Dictionary<string, string> _userPreferences;

        public MemoryManager()
        {
            _userPreferences = new Dictionary<string, string>();
        }

        public void SetUserName(string name)
        {
            _userName = name.Trim();
        }

        public string GetUserName()
        {
            return _userName ?? "Friend";
        }

        public bool HasUserName()
        {
            return !string.IsNullOrEmpty(_userName);
        }

        public void SetFavoriteTopic(string topic)
        {
            _favoriteTopic = topic;
            _userPreferences["favorite_topic"] = topic;
        }

        public string GetFavoriteTopic()
        {
            return _favoriteTopic ?? "cybersecurity";
        }

        public void SetPreference(string key, string value)
        {
            _userPreferences[key] = value;
        }

        public string GetPreference(string key)
        {
            return _userPreferences.ContainsKey(key) ? _userPreferences[key] : null;
        }

        public string GetPersonalizedGreeting()
        {
            if (!string.IsNullOrEmpty(_userName))
            {
                return $"Welcome back, {_userName}!";
            }
            return "Welcome!";
        }

        public string RecallFavoriteTopic()
        {
            if (!string.IsNullOrEmpty(_favoriteTopic))
            {
                return $"Since you're interested in {_favoriteTopic}, here's a relevant tip for you:\n";
            }
            return null;
        }
    }
}