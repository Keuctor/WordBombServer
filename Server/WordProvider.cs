

namespace WordBombServer.Server
{
    /// <summary>
    /// TODO : FIX THIS
    /// </summary>
    /// 
    public struct Suggestion
    {
        public string Word;
        public byte Language;
    }
    public class WordProvider
    {
        static HashSet<string> words_tr = new HashSet<string>();
        static HashSet<string> words_tr_suggested = new HashSet<string>();
        static HashSet<string> words_en = new HashSet<string>();
        static HashSet<string> words_en_suggested = new HashSet<string>();

        static HashSet<string> known_words_tr = new HashSet<string>();
        static HashSet<string> known_words_en = new HashSet<string>();

        static Random random = new Random();

        public void LoadWords()
        {
            Console.WriteLine("Loading words...");
            words_tr = LoadWords("/words/words_tr.txt");
            words_en = LoadWords("/words/words_en.txt");
            known_words_en = LoadWords("/words/words_en_known.txt");
            known_words_tr = LoadWords("/words/words_tr_known.txt");

            var suggestions = ReadSuggestions();
            foreach (var suggest in suggestions)
            {
                if (suggest.Language == 0)
                {
                    words_en_suggested.Add(suggest.Word);
                }
                else
                {
                    words_tr_suggested.Add(suggest.Word);
                }
                Console.WriteLine($"Suggestion Added {suggest.Language}:{suggest.Word}");
            }

            Console.WriteLine("TR total:" + (words_tr.Count + words_tr_suggested.Count));
            Console.WriteLine("EN total:" + (words_en.Count + words_en_suggested.Count));

            Console.WriteLine("Known word TR count : " + known_words_tr.Count);
            Console.WriteLine("Known word EN count : " + known_words_en.Count);


        }

        public bool HasKnownWord(byte language, string word)
        {
            if (language == 0)
            {
                return known_words_en.Contains(word);
            }
            return known_words_tr.Contains(word);
        }

        public bool HasWord(byte language, string word)
        {
            if (language == 0)
            {
                if (words_en.Contains(word))
                {
                    return true;
                }
                return words_en_suggested.Contains(word);
            }

            if (words_tr.Contains(word))
            {
                return true;
            }
            return words_tr_suggested.Contains(word);
        }



        public string GetRandomWordPart(int length, byte language)
        {
            var word = GetRandomWord(language);
            var str = word.Trim();
            while (str.Length < length)
            {
                str = GetRandomWord(language).Trim();
            }
            return str.Substring(0, length);
        }


        public string GetRandomWord(byte language)
        {
            var selectedWords = language == 0 ? words_en : words_tr;
            var index = random.Next(0, selectedWords.Count);


            return selectedWords.ElementAt(index);
        }

        public void WriteKnownWord(byte lang, string word)
        {
            string wordTrDirr;
            if (lang == 0)
            {
                wordTrDirr = AppDomain.CurrentDomain.BaseDirectory + "/words/words_en_known.txt";
            }
            else
            {
                wordTrDirr = AppDomain.CurrentDomain.BaseDirectory + "/words/words_tr_known.txt";
            }
            File.AppendAllText(wordTrDirr, $"{word},");
        }

        public void WriteSuggestion(byte lang, string suggestion)
        {
            var wordTrDirr = AppDomain.CurrentDomain.BaseDirectory + "/words/suggested.txt";
            File.AppendAllText(wordTrDirr, $"{suggestion},{lang}{Environment.NewLine}");
        }

        private HashSet<Suggestion> ReadSuggestions()
        {
            HashSet<Suggestion> suggestions = new HashSet<Suggestion>();
            var wordTrDirr = AppDomain.CurrentDomain.BaseDirectory + "/words/suggested.txt";
            var allLines = File.ReadAllLines(wordTrDirr);
            foreach (var line in allLines)
            {

                var stringParsed = line.Split(",");
                if (stringParsed.Length != 3)
                {
                    continue;
                }
                var word = stringParsed[0];
                var lang = stringParsed[1];

                if (byte.TryParse(lang, out byte val))
                {
                    suggestions.Add(new Suggestion()
                    {
                        Language = val,
                        Word = word
                    });
                }
            }
            return suggestions;
        }
        private HashSet<string> LoadWords(string path)
        {
            var wordTrDirr = AppDomain.CurrentDomain.BaseDirectory + path;
            string text = File.ReadAllText(wordTrDirr);

            string[] arrayString = text.Split(',', StringSplitOptions.RemoveEmptyEntries);
            HashSet<string> listToReturn = new HashSet<string>();
            for (int i = 0; i < arrayString.Length; i++)
            {
                if (arrayString[i].Contains('-') || arrayString[i].Contains(',') ||
                    arrayString[i].Contains('+') || arrayString[i].Contains('*')
                    || arrayString[i].Contains('.'))
                    continue;

                listToReturn.Add(arrayString[i]);
            }
            return listToReturn;
        }
    }
}
