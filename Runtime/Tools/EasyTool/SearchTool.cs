using System;
using System.Collections.Generic;
using System.Linq;

namespace NonsensicalKit.Tools.EasyTool
{
    public class StringSearcher
    {
        private StringSearchTreeNode _root;

        private string[] _sources;

        public StringSearcher()
        {
            _root = new StringSearchTreeNode();
        }

        public StringSearcher(IList<string> sources)
        {
            Init(sources);
        }

        public void Init(IList<string> sources)
        {
            _sources = new string[sources.Count];
            _root = new StringSearchTreeNode();
            for (int i = 0; i < sources.Count; i++)
            {
                _sources[i] = sources[i];
                AddString(sources[i], i);
            }
        }

        public void Init<T>(IList<T> sources, Func<T, string> getter)
        {
            _sources = new string[sources.Count];
            _root = new StringSearchTreeNode();
            for (int i = 0; i < sources.Count; i++)
            {
                _sources[i] = getter(sources[i]);
                AddString(_sources[i], i);
            }
        }

        public int[] SearchIndex(string match)
        {
            var node = _root;
            foreach (var c in match)
            {
                if (node.Children.TryGetValue(c, out StringSearchTreeNode child) == false)
                {
                    return new int[0];
                }

                node = child;
            }

            return node.SourceIndexes.ToArray();
        }

        public StringSearchResult[] Search(string match)
        {
            var indexes = SearchIndex(match);
            StringSearchResult[] results = new StringSearchResult[indexes.Length];
            for (int i = 0; i < indexes.Length; i++)
            {
                int index = indexes[i];
                results[i] = (new StringSearchResult(_sources[index], index));
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

    public class StringSearchTreeNode
    {
        public char Value;
        public readonly HashSet<int> SourceIndexes = new();
        public readonly Dictionary<char, StringSearchTreeNode> Children = new();

        public StringSearchTreeNode()
        {
        }

        public StringSearchTreeNode(char value)
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
                Children.Add(nextChar, new StringSearchTreeNode(nextChar));
            }

            Children[nextChar].Add(str, charIndex, sourceIndex);
        }
    }

    public struct StringSearchResult
    {
        public string Text;
        public int Index;

        public StringSearchResult(string text, int index)
        {
            Text = text;
            Index = index;
        }
    }


    public class TimeSearcher
    {
        private DateTime[] _sources;
        private int[] _sourceIndexes;

        public TimeSearcher(IList<DateTime> sources)
        {
            Init(sources);
        }

        public void Init(IList<DateTime> sources)
        {
            _sources = sources.ToArray();
            Sort();
        }

        public void Init<T>(IList<T> sources, Func<T, DateTime> getter)
        {
            _sources = new DateTime[sources.Count];
            for (int i = 0; i < sources.Count; i++)
            {
                _sources[i] = getter(sources[i]);
            }
            Sort();
        }

        public int[] SearchIndex(DateTime start, DateTime end)
        {
            if (start > end)
            {
                return Array.Empty<int>();
            }

            int startIndex = -1;
            int endIndex = -1;
            for (int i = 0; i < _sources.Length; i++)
            {
                if (startIndex==-1&&_sources[i] >= start)
                {
                    startIndex = i;
                }

                if (_sources[i] <= end)
                {
                    endIndex = i;
                }
                else if (endIndex > 0)
                {
                    break;
                }
            }

            if (startIndex == -1 || endIndex == -1)
            {
                return Array.Empty<int>();
            }

            int[] result = new int[endIndex - startIndex + 1];
            int resultIndex = 0;
            for (int i = startIndex; i <= endIndex; i++)
            {
                result[resultIndex++] = _sourceIndexes[i];
            }
            Array.Sort(result);
            return result;
        }

        private void Sort()
        {
            _sourceIndexes=new int[_sources.Length];

            for (int i = 0; i < _sourceIndexes.Length; i++)
            {
                _sourceIndexes[i] = i;
            }

            for (int i = 0; i < _sources.Length-1; i++)
            {
                for (int j = _sources.Length-1; j >i; j--)
                {
                    if (_sources[j]<_sources[j-1])
                    {
                        (_sources[j], _sources[j-1]) = (_sources[j-1], _sources[j]);
                        (_sourceIndexes[j], _sourceIndexes[j-1]) = (_sourceIndexes[j-1], _sourceIndexes[j]);
                    }
                }
            }
        }
    }
}