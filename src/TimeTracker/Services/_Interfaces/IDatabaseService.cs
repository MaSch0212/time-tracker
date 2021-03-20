using System.Data;
using System.Threading.Tasks;

namespace TimeTracker.Services
{
    public interface IDatabaseService
    {
        public Task<IDbCommand> CreateCommand(string cmdText);
    }
}
