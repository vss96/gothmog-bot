namespace GothmogBot.Services;

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public interface IPointsJobService
{
	string EnqueueJob();
	string GetJobStatus(string jobId);
}

public class PointsJobService : IPointsJobService
{
	private readonly ConcurrentDictionary<string, string> _jobStatuses = new();
	private readonly IPointsCalculatorService pointsCalculatorService;

	public PointsJobService(IPointsCalculatorService pointsCalculatorService)
	{
		this.pointsCalculatorService = pointsCalculatorService;
	}

	public string EnqueueJob()
	{
		var jobId = Guid.NewGuid().ToString();
		_jobStatuses[jobId] = "Pending";

		// Simulate job execution
		Task.Run(async () =>
		{
			try
			{
				_jobStatuses[jobId] = "In Progress";

				// Execute the job
				await pointsCalculatorService.CalculateAndStorePoints().ConfigureAwait(false);

				_jobStatuses[jobId] = "Completed";
			}
			catch (Exception)
			{
				_jobStatuses[jobId] = "Failed";
			}
		});

		return jobId;
	}

	public string GetJobStatus(string jobId)
	{
		return _jobStatuses.TryGetValue(jobId, out var status) ? status : "Unknown Job ID";
	}
}
