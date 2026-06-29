using UnityEngine;

namespace NonsensicalKit.Tools.GUITool
{
    public partial class NonsensicalConsole
    {
        private sealed class ConsoleStyles
        {
            private int _fontSize = -1;
            private GUISkin _skin;

            public GUIStyle Window { get; private set; }
            public GUIStyle Label { get; private set; }
            public GUIStyle Button { get; private set; }
            public GUIStyle TextField { get; private set; }
            public GUIStyle Toggle { get; private set; }

            public void Update(int fontSize)
            {
                if (GUI.skin == null)
                {
                    return;
                }

                if (_fontSize == fontSize && ReferenceEquals(_skin, GUI.skin))
                {
                    return;
                }

                _fontSize = fontSize;
                _skin = GUI.skin;

                Window = Clone(_skin.window, fontSize);
                Label = Clone(_skin.label, fontSize);
                Button = Clone(_skin.button, fontSize);
                TextField = Clone(_skin.textField, fontSize);
                Toggle = Clone(_skin.toggle, fontSize);
            }

            private static GUIStyle Clone(GUIStyle source, int fontSize)
            {
                GUIStyle style = source == null ? new GUIStyle() : new GUIStyle(source);
                style.fontSize = fontSize;
                return style;
            }
        }
    }
}
