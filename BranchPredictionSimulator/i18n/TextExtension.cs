using System;
using System.Windows.Markup;
using System.Windows;
using System.Collections.Generic;

namespace BranchPredictionSimulator.Localization
{
    public class TextExtension : MarkupExtension
    {
        public readonly string key;

        public TextExtension(string key)
        {
            this.key = key;
        }

        private List<FrameworkElement> hosts = new List<FrameworkElement>(); // there can be multiple objects to update in the case of a template
        private DependencyProperty property = null;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            if (target != null && target.TargetObject != null)
            {
                // a complex problem, fixed as in this article:
                // http://tomlev2.wordpress.com/2009/08/23/wpf-markup-extensions-and-templates/
                if (target.TargetObject.GetType().FullName == "System.Windows.SharedDp")
                {
                    return this;
                }

                hosts.Add(target.TargetObject as FrameworkElement);
                property = target.TargetProperty as DependencyProperty;
            }

            // used for future updates
            LanguageSelector.Global.languageChanged += new EventHandler(globalLanguageChanged);

            // sets the value of the property directly
            return LanguageSelector.Global.getLocalizedString(key);
        }

        private void globalLanguageChanged(object sender, EventArgs e)
        {
            if (hosts != null && property != null)
            {
                foreach (FrameworkElement host in hosts)
                {
                    host.SetValue(property, LanguageSelector.Global.getLocalizedString(key));
                }
            }
        }        
    }
}
