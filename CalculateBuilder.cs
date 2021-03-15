using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkloadService.CalculateEngine;
using WorkloadService.Sky;

namespace WorkloadService.ActivityCalculate
{
    /// <summary>
    /// 敏捷开发，没有封装 不安全
    /// </summary>
    public class Option : IEquatable<Option>
    {
        public ICalculateSource source;
        public ITransport transport;
        public IContext cotext;
        public string key;

        public bool Equals(Option other)
        {
            if (other == null) return false;
            return key == other.key;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as Option);
        }

        public override int GetHashCode()
        {
            return key.GetHashCode();
        }
    }
    public class CalculateBuilder : ICalculateBuilder
    {
        public IList<Option> Options { get; } = new List<Option>();

        public ICalculateBuilder Add(Option option)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }
            if (option.source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (option.transport == null)
            {
                throw new ArgumentNullException("transport");
            }
            if (option.cotext == null)
            {
                throw new ArgumentNullException("cotext");
            }

            if (!IsExist(option))
            {
                Options.Add(option);
            }
            else
            {
                foreach (var optionItem in Options.Where(o => o.key == option.key))
                {
                    optionItem.cotext = option.cotext;
                }
            }
            return this;
        }

        private bool IsExist(Option option)
        {
            if (Options.Any(o => o.key == option.key))
            {
                return true;
            }
            return false;
        }

        public void Build()
        {
            foreach (var option in Options)
            {
                var provider = option.source.Build(this);
                provider.LoadData();
            }
        }

        public float Calculate<TSource>() where TSource : class
        {
            ICalculateSource mysource = null;
            foreach (var option in Options)
            {
                if (option.source is TSource)
                {
                    mysource = option.source;
                    break;
                }
            }

            if (mysource == null)
                throw new NullReferenceException();

            var calculator = mysource.Delegate(this);
            return calculator.Calculate();
        }
    }
}
