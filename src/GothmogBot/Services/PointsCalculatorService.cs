using System.Collections.Concurrent;
using System.Globalization;
using Discord;
using Discord.Rest;
using GothmogBot.Discord;
using Serilog;

namespace GothmogBot.Services;

public interface IPointsCalculatorService
{
	Task CalculateAndStorePoints();
}

public class PointsCalculatorService : IPointsCalculatorService
{
	private readonly DiscordRestClient discordRestClient;
	private readonly IPointsService pointsService;

	public PointsCalculatorService(DiscordRestClient discordRestClient, IPointsService pointsService)
	{
		this.discordRestClient = discordRestClient;
		this.pointsService = pointsService;
	}

	public async Task CalculateAndStorePoints()
	{
		// Fetch the guild (server)
		var guild = await discordRestClient
			.GetGuildAsync(DiscordConstants.BulldogsKappaClubDiscordGuildId).ConfigureAwait(false);

		// Get all text channels in the guild
		var textChannels = await guild.GetTextChannelsAsync().ConfigureAwait(false);

		var userMessages = new ConcurrentDictionary<ulong, List<DateTime>>();


		foreach (var channel in textChannels)
		{
			var messages = await FetchChannelMessages(channel).ConfigureAwait(false);

			foreach (var message in messages)
			{
				if (userMessages.TryGetValue(message.Author.Id, out var value))
				{
					value.Add(message.Timestamp.UtcDateTime);
				}
				else
				{
					userMessages[message.Author.Id] = new List<DateTime> { message.Timestamp.UtcDateTime };
				}
			}
		}

		// Get all users in the guild
		var users = await guild.GetUsersAsync(RequestOptions.Default)
			.FlattenAsync()
			.ConfigureAwait(false);

		// Calculate points for each user
		foreach (var user in users)
		{
			if (!userMessages.TryGetValue(user.Id, out var timestamps)) continue;
			var points = CalculatePoints(timestamps);
			Log.Information($"Points for {user.Username}: {points}");
			await pointsService.UpdatePointsAsync(user.Username, points).ConfigureAwait(false);
		}
	}

	private static async Task<IReadOnlyCollection<IMessage>> FetchChannelMessages(ITextChannel channel)
	{
		var messages = new List<IMessage>();
		var lastMessageId = 0ul;

		while (true)
		{
			var fetchedMessages = await channel.GetMessagesAsync(lastMessageId, Direction.After, 100).FlattenAsync()
				.ConfigureAwait(false);
			if (!fetchedMessages.Any())
			{
				break;
			}

			messages.AddRange(fetchedMessages);
			lastMessageId = fetchedMessages.Last().Id;
		}

		return messages;
	}

	private static int CalculatePoints(List<DateTime> messageTimestamps)
	{
		// Group timestamps by time periods
		var hourlyGroups = messageTimestamps.GroupBy(ts => new { ts.Year, ts.Month, ts.Day, ts.Hour });
		var dailyGroups = messageTimestamps.GroupBy(ts => new { ts.Year, ts.Month, ts.Day });
		var weeklyGroups = messageTimestamps.GroupBy(ts =>
			CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(ts, CalendarWeekRule.FirstDay, DayOfWeek.Monday));

		var hourlyPoints = hourlyGroups.Sum(group => Math.Min(group.Count() * DiscordConstants.PointsPerMessage, DiscordConstants.HourlyPointsCap));

		var dailyPoints = dailyGroups.Select(group => Math.Min(group.Count() * DiscordConstants.PointsPerMessage, DiscordConstants.DailyPointsCap))
			.Aggregate(hourlyPoints, (current, dailyPoints) => current + Math.Max(dailyPoints - current, 0));

		var weeklyPoints = weeklyGroups.Select(group => Math.Min(group.Count() * DiscordConstants.PointsPerMessage, DiscordConstants.WeeklyPointsCap))
			.Aggregate(dailyPoints, (current, weeklyPoints) => current + Math.Max(weeklyPoints - current, 0));

		return Math.Min(weeklyPoints, DiscordConstants.WeeklyPointsCap);
	}
}
