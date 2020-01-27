#include <windows.h>

BOOL WINAPI DllMain(HINSTANCE hInst, DWORD reason, LPVOID)
{
	if (reason == DLL_PROCESS_ATTACH)
	{
		// Unprotect the module NOW
		const auto hExecutableInstance = reinterpret_cast<size_t>(GetModuleHandle(nullptr));
		const auto ntHeader = reinterpret_cast<IMAGE_NT_HEADERS*>(hExecutableInstance + reinterpret_cast<IMAGE_DOS_HEADER*>(hExecutableInstance)->e_lfanew);
		const SIZE_T size = ntHeader->OptionalHeader.SizeOfImage;
		DWORD old_protect;
		VirtualProtect(reinterpret_cast<VOID*>(hExecutableInstance), size, PAGE_EXECUTE_READWRITE, &old_protect);
	}
	return TRUE;
}