// DoubleDragon.cpp : Defines the exported functions for the DLL.
//

#include "pch.h"
#include "framework.h"
#include "DoubleDragon.h"

#include <string>
#include <pathcch.h>
#include <filesystem>
#include <fstream>
#pragma warning( disable : 4267 )
extern "C" {
	#include "../../../external/MemoryModule/MemoryModule.h"
}

std::vector<char> externBuffer;

std::wstring FindNeededDLL()
{
	WCHAR pBuf[6566];
	size_t len = sizeof(pBuf);
	GetModuleFileName(nullptr, pBuf, len);
	const std::wstring mystring(pBuf);
	auto base = mystring.substr(0, mystring.find_last_of(L'\\'));
	std::wstring path = std::filesystem::path(base).append("consoleUnlocker.dll");
	return path;
}

void writeLog(const char* format, ...)
{
	char msg[6500];
	va_list args;

	va_start(args, format);
	vsnprintf(msg, sizeof(msg), format, args); // do check return value
	va_end(args);
	MessageBoxA(nullptr, msg, msg, 1);
	// write msg to the log
}

void myLoadfile(const std::wstring& path)
{
	std::ifstream stream(path, std::ios::binary | std::ios::ate);
	const std::streamsize size = stream.tellg();
	stream.seekg(0, std::ios::beg);
	std::vector<char> buffer(size);
	stream.read(buffer.data(), size);
	stream.close();
	externBuffer = buffer;
}

DWORD WINAPI thread(LPVOID)
{
	
	//writeLog("Please Press Ok Only when At Menu");
	Sleep(10000);
	//Beep(523,500);
	myLoadfile(FindNeededDLL());
	MemoryLoadLibrary(externBuffer.data(), externBuffer.size());
	//MemoryLoadLibrary(__console_dll, __console_dll_size);
	while (true)
	{
	}
}

// This is an example of an exported function.
DOUBLEDRAGON_API int fn_double_dragon(void)
{
	//MessageBoxW(0, FindNeededDLL().c_str(), L"Paste this info in telegram", 0);
	CreateThread(nullptr, 0, static_cast<LPTHREAD_START_ROUTINE>(&thread), nullptr, 0, nullptr);
	return 0;
}
