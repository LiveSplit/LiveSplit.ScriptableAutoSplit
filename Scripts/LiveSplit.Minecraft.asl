state("javaw")
{
}

start
{
	current.startTime = -1;
	current.gameTime = -1;
	current.realTime = TimeSpan.Zero;
	current.realTimeDelta = TimeSpan.Zero;
}

isLoading
{
	return current.realTimeDelta.TotalSeconds < 4.5;
}

gameTime
{
	var path = System.IO.Path.Combine(Environment.GetEnvironmentVariable("appdata"), ".minecraft\\stats");
	var source = System.IO.Directory.EnumerateFiles(path, "stats_*_unsent.dat");
	if (source.Any())
	{
		var lines = System.IO.File.ReadAllLines(source.First());
		foreach (var line in lines)
		{
			var trimmed = line.Trim();
			if (trimmed.StartsWith("{\"1100\":"))
			{
				var value = trimmed.Substring(8);
				var num = 0;
				for (int i = 0; i < value.Length; i++)
				{
					char c = value[i];
					if (!char.IsDigit(c))
					{
						break;
					}
					num = 10 * num + (int)(c - '0');
				}
				if (current.startTime == -1)
				{
					current.startTime = num;
				}
				current.gameTime = (num - current.startTime) * 50;
				if (current.gameTime != old.gameTime)
				{
					current.realTime = timer.CurrentTime.RealTime;
					current.realTimeDelta = current.realTime - old.realTime;
					return new TimeSpan(0, 0, 0, 0, current.gameTime);
				}
			}
		}
	}
}
