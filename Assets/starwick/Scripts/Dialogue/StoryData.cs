using System.Collections.Generic;

namespace Starwick
{
    public static class StoryData
    {
        public static List<DialogueLine> FirstConstellation()
        {
            return new List<DialogueLine>
            {
                new DialogueLine("", "A star has gone dark. You feel the absence before you see it."),
                new DialogueLine("the Weave", "You are a Wick. You will relight what the dark has taken."),
                new DialogueLine("Vesp", "...is that what we are? little lights, sent to mind the lights?"),
                new DialogueLine("", "The shape answers your touch. A name surfaces, then a face you never met.",
                    "The Ferryman's Lantern - one who lit the way for others across the dark, and was forgotten the moment they were no longer needed."),
                new DialogueLine("the Weave", "One returns. Ten thousand remain dark. Walk on, Wick."),
            };
        }
    }
}
