state("NecroDancer")
{
	int ZoneID : "NecroDancer.exe", 0x0006DA40, 0x230;
	int LevelID : "NecroDancer.exe", 0x0006DA40, 0x2c4;
	int LevelTime : "NecroDancer.exe", 0x0006DA40, 0x10c;
	int Health : "NecroDancer.exe", 0x0018EF38, 0x14, 0x130;
	byte GamePaused  : "NecroDancer.exe", 0x0006DA40, 0x88;
}

start
{
	current.GameTime = 0;
	current.AccumulatedTime = 0;//-current.LevelTime;
	
	var isNotInMainRoom = !(current.ZoneID == 1 && current.LevelID == -2);
	var isInANewLevel = (old.ZoneID != current.ZoneID 
						|| old.LevelID != current.LevelID);
	var isRevived = old.Health <= 0 && current.Health > 0;
	
	return isNotInMainRoom && (isInANewLevel || isRevived);
}

split
{
	return old.ZoneID != current.ZoneID
		|| old.LevelID != current.LevelID;
}

reset
{
	return current.Health <= 0
	 || (current.ZoneID == 1 && current.LevelID == -2);
}

isLoading
{
	return false;
}

gameTime
{
	if (current.LevelTime < old.LevelTime 
		&& !(current.ZoneID == 1 && current.LevelID == 1))
		current.AccumulatedTime += old.LevelTime;

	current.GameTime = current.LevelTime + current.AccumulatedTime;
	if (current.GameTime != old.GameTime || current.GamePaused != 0)
		return TimeSpan.FromMilliseconds(current.GameTime);
}