using System;
using System.Collections.Generic;

namespace NonsensicalKit.Core
{
    public class Searcher
    {
        private SearchTreeNode _root;

        private string[] _sources;

        public Searcher()
        {
            _root = new SearchTreeNode();
        }

        public Searcher(List<string> sources)
        {
            Init(sources);
        }

        public void Init(List<string> sources)
        {
            _sources = new string[sources.Count];
            _root = new SearchTreeNode();
            for (int i = 0; i < sources.Count; i++)
            {
                _sources[i] = sources[i];
                AddString(sources[i], i);
            }
        }

        public void Init<T>(List<T> sources, Func<T, string> getter)
        {
            _sources = new string[sources.Count];
            _root = new SearchTreeNode();
            for (int i = 0; i < sources.Count; i++)
            {
                _sources[i] = getter(sources[i]);
                AddString(_sources[i], i);
            }
        }

        public List<int> SearchIndex(string match)
        {
            var node = _root;
            foreach (var c in match)
            {
                if (node.Children.TryGetValue(c, out SearchTreeNode child) == false)
                {
                    return new List<int>();
                }

                node = child;
            }

            return new List<int>(node.SourceIndexes);
        }

        public List<SearchResult> Search(string match)
        {
            var indexes = SearchIndex(match);
            List<SearchResult> results = new List<SearchResult>();
            foreach (var index in indexes)
            {
                results.Add(new SearchResult(_sources[index], index));
            }

            return results;
        }

        private void AddString(string str, int sourceIndex)
        {
            for (int i = 0; i < str.Length; i++)
            {
                _root.Add(str, i - 1, sourceIndex);
            }
        }
    }

    public class SearchTreeNode
    {
        public char Value;
        public readonly HashSet<int> SourceIndexes = new();
        public readonly Dictionary<char, SearchTreeNode> Children = new();

        public SearchTreeNode()
        {
        }

        public SearchTreeNode(char value)
        {
            Value = value;
        }

        public void Add(string str, int charIndex, int sourceIndex)
        {
            SourceIndexes.Add(sourceIndex);
            charIndex++;
            if (charIndex == str.Length) return;
            var nextChar = str[charIndex];
            if (Children.ContainsKey(nextChar) == false)
            {
                Children.Add(nextChar, new SearchTreeNode(nextChar));
            }

            Children[nextChar].Add(str, charIndex, sourceIndex);
        }
    }

    public struct SearchResult
    {
        public string Text;
        public int Index;

        public SearchResult(string text, int index)
        {
            Text = text;
            Index = index;
        }
    }
}