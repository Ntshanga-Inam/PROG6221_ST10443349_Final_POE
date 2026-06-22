using System;
using System.Collections.Generic;

namespace CybersecurityChatbot_Part2.Services
{
    public enum Sentiment
    {
        Neutral,
        Worried,
        Frustrated,
        Curious
    }

    public class SentimentAnalyzer
    {
        private HashSet<string> _worriedWords;
        private HashSet<string> _frustratedWords;
        private HashSet<string> _curiousWords;

        public SentimentAnalyzer()
        {
            InitializeDictionaries();
        }

        private void InitializeDictionaries()
        {
            _worriedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "worried", "scared", "afraid", "nervous", "anxious", "concerned",
                "fear", "unsafe", "vulnerable", "panic", "terrified"
            };

            _frustratedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "frustrated", "annoyed", "confused", "difficult", "hard", "complicated",
                "too much", "overwhelmed", "can't", "doesn't work", "stupid"
            };

            _curiousWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "curious", "interested", "want to learn", "tell me", "explain",
                "how does", "why", "what is", "teach me", "learn"
            };
        }

        public Sentiment AnalyzeSentiment(string input)
        {
            string lowerInput = input.ToLower();

            // Check each sentiment category
            foreach (var word in _worriedWords)
            {
                if (lowerInput.Contains(word))
                    return Sentiment.Worried;
            }

            foreach (var word in _frustratedWords)
            {
                if (lowerInput.Contains(word))
                    return Sentiment.Frustrated;
            }

            foreach (var word in _curiousWords)
            {
                if (lowerInput.Contains(word))
                    return Sentiment.Curious;
            }

            return Sentiment.Neutral;
        }
    }
}