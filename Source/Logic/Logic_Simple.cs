using RimWorld.QuestGen;

namespace Rimocracy
{
    public abstract class Logic_Simple : Logic, ISlateRef
    {
        public virtual string SlateRef { get; set; }

        public bool TryGetConvertedValue<T>(Slate slate, out T value) => throw new System.NotImplementedException();
    }
}
