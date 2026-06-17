using System.Collections;
using UnityEngine;

namespace Starwick
{
    public class Director : MonoBehaviour
    {
        public int Chosen { get; private set; }
        bool restart;

        void Start()
        {
            StartCoroutine(Flow());
        }

        IEnumerator Flow()
        {
            while (true)
            {
                while (Sw.Constellation == null || !Sw.Constellation.Complete) yield return null;

                if (Sw.Dialogue != null) Sw.Dialogue.Play(StoryData.FirstConstellation());
                if (Sw.PostFx != null) Sw.PostFx.SetMoodWarm(0.6f);
                yield return WaitDialogue();

                Chosen = 0;
                if (Sw.Narration != null)
                    Sw.Narration.ShowChoices("The star waits. What do you offer it?",
                        "Let it rest", "Send it onward", Choose);
                while (Chosen == 0) yield return null;
                if (Sw.Narration != null) Sw.Narration.HideChoices();
                GameState.Choice = Chosen;

                if (Sw.PostFx != null) Sw.PostFx.SetMoodWarm(Chosen == 2 ? 1f : 0.12f);
                if (Sw.Dialogue != null) Sw.Dialogue.Play(StoryData.Ending(Chosen));
                yield return WaitDialogue();

                restart = false;
                if (Sw.Dialogue != null) Sw.Dialogue.Play(StoryData.BeginAgain());
                yield return WaitDialogue();
                while (!restart) yield return null;

                GameState.Reset();
                if (Sw.PostFx != null) Sw.PostFx.SetMoodWarm(0.45f);
                if (Sw.Constellation != null) Sw.Constellation.ResetForReplay();
                Chosen = 0;
            }
        }

        IEnumerator WaitDialogue()
        {
            yield return null;
            while (Sw.Dialogue != null && Sw.Dialogue.Active) yield return null;
        }

        public void Choose(int i)
        {
            if (Chosen == 0) Chosen = i;
        }

        public void RequestRestart()
        {
            restart = true;
        }
    }
}
