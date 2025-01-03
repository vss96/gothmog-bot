using GothmogBot.Database;
using Microsoft.EntityFrameworkCore;

namespace GothmogBot.Services;

public interface IPointsService
{
	Task UpdatePointsAsync(string discordUsername, long points);
	Task<long?> GetPointsAsync(string discordUsername);
	Task RemovePointsAsync(string discordUsername);
	Task SetPointsAsync(string discordUsername, long points);
}
public class PointsService : IPointsService
{
	private readonly ApplicationDbContext dbContext;
	public PointsService(ApplicationDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	public async Task UpdatePointsAsync(string discordUsername, long points)
	{
		var userPoints = await dbContext.Points.FindAsync(discordUsername).ConfigureAwait(false);

		if (userPoints == null)
		{
			userPoints = new Points
			{
				DiscordUsername = discordUsername,
				DiscordPoints = points
			};
			dbContext.Points.Add(userPoints);
		}
		else
		{
			userPoints.DiscordPoints = points;
			dbContext.Points.Update(userPoints);
		}

		await dbContext.SaveChangesAsync().ConfigureAwait(false);
	}

	public async Task<long?> GetPointsAsync(string discordUsername)
	{
		var userPoints = await dbContext.Points
			.FirstOrDefaultAsync(p => p.DiscordUsername == discordUsername)
			.ConfigureAwait(false);

		return userPoints?.DiscordPoints;
	}

	public async Task RemovePointsAsync(string discordUsername)
	{
		var userPoints = await dbContext.Points
			.FirstOrDefaultAsync(p => p.DiscordUsername == discordUsername)
			.ConfigureAwait(false);

		if (userPoints != null)
		{
			dbContext.Points.Remove(userPoints);
			await dbContext.SaveChangesAsync().ConfigureAwait(false);
		}
	}

	public async Task SetPointsAsync(string discordUsername, long points)
	{
		var userPoints = new Points
		{
			DiscordUsername = discordUsername,
			DiscordPoints = points
		};
		dbContext.Points.Add(userPoints);
		await dbContext.SaveChangesAsync().ConfigureAwait(false);
	}
}
