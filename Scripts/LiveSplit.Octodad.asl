state("OctodadDadliestCatch")
{
	string48 levelName : "OctodadDadliestCatch.exe", 0x0023FD88, 0xd8, 0xc, 0x4c, 0x0;
	bool isLoading : "OctodadDadliestCatch.exe", 0x0023FD8C, 0x4c, 0x8c; 
	float levelTimer : "OctodadDadliestCatch.exe", 0x0023FD88, 0xd8, 0x8;
}

start
{
	return (current.levelName == "" || current.levelName.StartsWith("MainScreen_Background")) 
		&& current.isLoading && !old.isLoading;
}

split
{
	return !old.isLoading && current.isLoading
		&& !old.levelName.StartsWith("MainScreen_Background")
		&& !old.levelName.StartsWith("OpeningCredits")
		&& !(timer.CurrentSplitIndex >= 5 && timer.CurrentSplitIndex <= 7 && old.levelName.StartsWith("Aquarium_Hub"));
}

isLoading
{
	return current.isLoading;
}

gameTime
{
}