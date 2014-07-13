state("NightSky")
{
	bool isLoading : "Nightsky.exe", 0x00211490, 0x498, 0x35;
	int chapterID : "Nightsky.exe", 0x00211490, 0xa4, 0x1b0;
}

start
{
	return old.chapterID == 20239 && current.chapterID == 0;
}

split
{
	return !old.isLoading && current.isLoading && current.chapterID != 0;
}

isLoading
{
	return current.isLoading;
}

gameTime
{
}