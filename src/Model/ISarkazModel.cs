using System.Threading.Tasks;

namespace Skz.Ocean.Model;

public interface ISarkazModel
{
    public virtual Task AfterThoughtUsed()
    {
        return Task.CompletedTask;
    }
}