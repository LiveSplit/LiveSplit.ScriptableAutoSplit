state("Anuntitledstory")
{
	float dblhacktime : "AnUntitledStory.exe", 0x00189720, 0x4, 0x35c;
}

start
{
	return current.dblhacktime == 1.875 && old.dblhacktime == 0;
}

reset
{
	return old.dblhacktime > current.dblhacktime;
}

isLoading
{
	return false;
}
