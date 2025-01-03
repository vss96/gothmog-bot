
using Microsoft.EntityFrameworkCore;

namespace GothmogBot.Database;

[PrimaryKey(nameof(DiscordUsername))]
public sealed record Points
{

	public required string DiscordUsername { get; set; }

	public long DiscordPoints { get; set; }

}
