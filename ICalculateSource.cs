using WorkloadService.Sky;

namespace WorkloadService.ActivityCalculate
{
    public interface ICalculateSource
    {
        ICalculatorSourceProvider Build(ICalculateBuilder builder);
        ISourceCalculator Delegate(ICalculateBuilder builder);
    }
}
