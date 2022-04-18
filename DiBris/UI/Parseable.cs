using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using UnityEngine;

namespace DiBris.UI
{
    internal abstract class Parseable : IRefreshable
    {
        [UIValue("name")]
        public abstract string Name { get; }
        public abstract string ContentPath { get; }

        [UIParams]
        protected BSMLParserParams parserParams = null!;

        [UIComponent("root")]
        public RectTransform root = null!;

        public virtual void Refresh()
        {
            this.parserParams?.EmitEvent("update");
        }
    }
}