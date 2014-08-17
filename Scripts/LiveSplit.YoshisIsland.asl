state("snes9x")
{
	byte isInALevel : "snes9x.exe", 0x002EFBA4, 0x2904a;
	short levelFrames : "snes9x.exe", 0x002EFBA4, 0x3a9;
}

start
{
	current.gameTime = TimeSpan.Zero;
	return old.isInALevel == 4 && current.isInALevel == 0;
}

split
{
	if (current.levelFrames < old.levelFrames)
		current.gameTime += TimeSpan.FromSeconds(old.levelFrames / 60.0f);	

	return old.isInALevel == 1 && current.isInALevel == 0;
}

isLoading
{
	return true;
}

gameTime
{
	return current.gameTime + TimeSpan.FromSeconds(current.levelFrames / 60.0f);
}