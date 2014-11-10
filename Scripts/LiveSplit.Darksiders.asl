state("DarksidersPC")
{
	int time : "DarksidersPC.exe", 0x0122E594, 0x53C, 0x74;

}

start
{
	return current.time != 0;
}

split
{
}

isLoading
{
	return true;
}

gameTime
{
	return TimeSpan.FromSeconds(current.time);
}