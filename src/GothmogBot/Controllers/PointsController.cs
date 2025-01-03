using GothmogBot.Services;
using Microsoft.AspNetCore.Mvc;

namespace GothmogBot.Controllers;

[ApiController]
[Route("api/points")]
public class PointsController : Controller
{
	private readonly IPointsJobService pointsJobService;
	private readonly IPointsService pointsService;

	public PointsController(IPointsJobService pointsJobService,
		IPointsService pointsService)
	{
		this.pointsJobService = pointsJobService;
		this.pointsService = pointsService;
	}

	[HttpPost("job/trigger")]
	public IActionResult CalculatePoints()
	{
		// Trigger the job
		var jobId = pointsJobService.EnqueueJob();

		return Ok(new { Message = "Job enqueued", JobId = jobId });
	}

	[HttpGet("status/{jobId}")]
	public IActionResult GetJobStatus(string jobId)
	{
		var status = pointsJobService.GetJobStatus(jobId);
		return Ok(new { JobId = jobId, Status = status });
	}

	[HttpGet("{username}")]
	public async Task<IActionResult> GetPoints(string username)
	{
		var points = await pointsService.GetPointsAsync(username).ConfigureAwait(false);
		if (points.HasValue == false)
		{
			return NotFound();
		}

		return Ok(new { Username = username, Points = points });
	}
}
