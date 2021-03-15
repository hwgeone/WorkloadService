using System.Collections.Generic;
using WorkloadService.Sky;

namespace WorkloadService.ActivityCalculate
{
    public interface ICalculateBuilder
    {
        void Build();
        IList<Option> Options { get; }
        /// <summary>
        /// Adds a new configuration source.
        /// </summary>
        /// <param name="source">The configuration source to add.</param>
        /// <returns>The same <see cref="IConfigurationBuilder"/>.</returns>
        ICalculateBuilder Add(Option option);
    }
}
