﻿using System.Collections.Immutable;

namespace GothmogBot.Discord;

public static class DiscordConstants
{
	public const ulong BulldogsKappaClubDiscordGuildId = 111772771016515584;

	public const ulong YouTubeTier2RoleId = 777437173904703540;
	public const ulong TwitchTier3SubscriberRoleId = 707480013406076951;
	public const ulong CustomMegaCuckRoleId = 948486251315609610;

	public const int PointsPerMessage = 10;
	public const int HourlyPointsCap = 30;
	public const int DailyPointsCap = 120;
	public const int WeeklyPointsCap = 480;

	public static ImmutableList<ulong> AllRoles { get; } = ImmutableList.Create(YouTubeTier2RoleId, TwitchTier3SubscriberRoleId, CustomMegaCuckRoleId);
}
