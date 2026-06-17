using System.Collections.Generic;
using UnityEngine;

namespace Starwick
{
    public class DialogueLine
    {
        public string Speaker;
        public string Text;
        public string Fragment;

        public DialogueLine(string speaker, string text, string fragment = null)
        {
            Speaker = speaker;
            Text = text;
            Fragment = fragment;
        }
    }

    public class DialogueSystem : MonoBehaviour
    {
        public bool Active { get; private set; }
        public int Index { get; private set; }

        List<DialogueLine> lines;

        public DialogueLine Current =>
            (lines != null && Index >= 0 && Index < lines.Count) ? lines[Index] : null;

        public System.Action<DialogueLine> OnLine;
        public System.Action OnEnd;

        public void Play(List<DialogueLine> sequence)
        {
            lines = sequence;
            Index = 0;
            Active = lines != null && lines.Count > 0;
            if (Active) Emit();
        }

        public void Advance()
        {
            if (!Active) return;
            Index++;
            if (lines == null || Index >= lines.Count)
            {
                Active = false;
                OnEnd?.Invoke();
                return;
            }
            Emit();
        }

        void Emit()
        {
            var l = Current;
            if (l != null && !string.IsNullOrEmpty(l.Fragment))
                GameState.AddFragment(l.Fragment);
            OnLine?.Invoke(l);
        }
    }
}
