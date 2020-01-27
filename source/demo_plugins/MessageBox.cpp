#include <windows.h>
#pragma unmanaged
extern int __stdcall Initialize(void* param);
extern void __stdcall Shutdown();
DWORD WINAPI thread(LPVOID)
{
	char* testing = "aaa";
	Initialize(testing);
	while (true)
	{
		
	}
}
BOOL WINAPI DllMain(HMODULE hModule, DWORD fdwReason, LPVOID lpvReservedD)
{
	if (fdwReason == DLL_PROCESS_ATTACH)
	{
		DisableThreadLibraryCalls(hModule);

		CreateThread(nullptr, 0, LPTHREAD_START_ROUTINE(&thread), nullptr, 0, nullptr);
	}
	if(fdwReason == DLL_PROCESS_DETACH)
	{
		Shutdown();
	}
	return TRUE;
}