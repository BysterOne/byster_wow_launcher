using Cls;
using Launcher.Settings;

namespace Launcher.Any
{
    public interface ITranslatable
    {
        public abstract Task UpdateAllValues();
    }

    public static class TranslationHub
    {
        private static readonly HashSet<WeakReference<ITranslatable>> _pool = new();

        static TranslationHub()
        {
            AppSettings.LanguageChanged += async _ =>
            {
                foreach (var wr in _pool.ToArray())
                    if (wr.TryGetTarget(out var t))
                        await t.UpdateAllValues();
            };
        }

        public static void Register(ITranslatable t)
            => _pool.Add(new WeakReference<ITranslatable>(t));
    }
}
