state("project64")
{
	byte Stars : "Project64.exe", 0x000D6A1C, 0x33b218;
	int GameFrames : "Project64.exe", 0x000D6A1C, 0x32d5d4;
}

start
{
	current.AccumulatedFrames = 0;
	current.StarTime = 0;

	return old.GameFrames > 0 && current.GameFrames == 0;
}

split
{
	if (old.Stars < current.Stars)
		current.StarTime = current.GameFrames;

	if (current.StarTime > 0)
	{
		var delta = current.GameFrames - current.StarTime;
		
		if (delta > 120)
		{
			current.StarTime = 0;
			return true;
		}
	}
}

isLoading
{
	return false;
}

gameTime
{
	if (current.GameFrames < old.GameFrames)
		current.AccumulatedFrames += old.GameFrames;

	return TimeSpan.FromSeconds((current.GameFrames + current.AccumulatedFrames) / 30.0f);
}