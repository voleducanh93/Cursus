using Cursus.Data.Entities;

namespace Cursus.RepositoryContract.Interfaces
{
	public interface IStepRepository : IRepository<Step>
	{
		Task<Step> GetByIdAsync(int id);
		Task<Step> GetByCoursId(int id);
        Task<IEnumerable<Step>> GetStepsByCoursId(int courseId);
        Task<double> GetToTalSteps(int couressId);

    }
}
