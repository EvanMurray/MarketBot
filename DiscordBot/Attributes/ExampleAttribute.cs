using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal sealed class ExampleAttribute : Attribute
    {
        public string ExampleText { get; }

        public ExampleAttribute(string exampleText)
        {
            ExampleText = exampleText;
        }
    }
}
