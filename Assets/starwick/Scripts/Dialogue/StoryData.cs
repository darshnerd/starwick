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
                new DialogueLine("the Weave", "The ember waits in your hands. What you give it now, it becomes."),
            };
        }

        public static List<DialogueLine> Ending(int choice)
        {
            return choice == 1 ? EndingRest() : EndingSend();
        }

        public static List<DialogueLine> EndingRest()
        {
            return new List<DialogueLine>
            {
                new DialogueLine("", "You cup the new light and let it slow. It does not have to burn for anyone now."),
                new DialogueLine("Vesp", "it's... resting. i didn't know we were allowed to let them rest."),
                new DialogueLine("the Weave", "Mercy is a kind of dark too. The sky cools around your choice.",
                    "You chose rest - that a light is allowed to simply be, unspent."),
            };
        }

        public static List<DialogueLine> EndingSend()
        {
            return new List<DialogueLine>
            {
                new DialogueLine("", "You breathe on the ember and it leaps - bright, eager, already reaching outward."),
                new DialogueLine("Vesp", "look at it go! it wants to matter. maybe that's the same as being alive."),
                new DialogueLine("the Weave", "Purpose is a kind of light. The sky warms gold around your choice.",
                    "You chose purpose - and sent a fate back out to burn for others, as the Ferryman did."),
            };
        }

        public static List<DialogueLine> BeginAgain()
        {
            return new List<DialogueLine>
            {
                new DialogueLine("the Weave", "Ten thousand remain dark. Trace again, Wick, and begin anew."),
            };
        }
    }
}
