namespace WorkloadService.ActivityCalculate
{
    public interface ICalculatorSourceProvider
    {
        void LoadData();
    }

    public abstract class CalculateSourceProvider: ICalculatorSourceProvider
    {
        public virtual void LoadData()
        {
        }
    }
}
