#include <windows.h>
#include <stdint.h>

DWORD WINAPI thread(LPVOID)
{
	MEMORY_BASIC_INFORMATION mbi = { 0 };
	uintptr_t dwEndAddr;
	while (true)
	{
		Sleep(1000);

		while (VirtualQuery(reinterpret_cast<VOID *>(reinterpret_cast<uintptr_t>(mbi.BaseAddress) + mbi.RegionSize), &mbi, sizeof(MEMORY_BASIC_INFORMATION)))
		{
			if (mbi.Protect == PAGE_EXECUTE_READWRITE)
			{
				dwEndAddr = reinterpret_cast<uintptr_t>(mbi.BaseAddress) + mbi.RegionSize - 1 - 4;

				for (auto i = reinterpret_cast<uintptr_t>(mbi.BaseAddress); i <= dwEndAddr; i++)
				{
					__try
					{
						if (*reinterpret_cast<uint64_t*>(i) == static_cast<uint64_t>(0x49244889C9480F41))
						{
							if (*reinterpret_cast<uint32_t*>(i + sizeof(uint64_t)) == static_cast<uint32_t>(0x4C50428B))
							{
								*reinterpret_cast<uint32_t*>(i) = 0x072440C7;
								*reinterpret_cast<uint32_t*>(i + 4) = 0x49000000; //mov [rax+24],00000007
								return 0;
							}
						}
					}
					__except (true)
					{
						i = dwEndAddr;
					}
				}
			}
		}
	}
	return 0;
}


BOOL APIENTRY DllMain(HMODULE /*hModule*/, DWORD reason, LPVOID /*lpReserved*/)
{
	if (reason == DLL_PROCESS_ATTACH)
	{
		CreateThread(0, 0, (LPTHREAD_START_ROUTINE)&thread, NULL, 0, NULL);
	}
	return TRUE;
}