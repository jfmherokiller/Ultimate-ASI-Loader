//#include <Windows.h>
#pragma managed
#include <windows.h>

// Import C# code base
//#using "ScriptHookVDotNet.netmodule"

using namespace System;
using namespace Reflection;

[assembly:AssemblyTitle("Community Script Hook V .NET")] ;
[assembly:AssemblyDescription("An ASI plugin for Grand Theft Auto V, which allows running scripts written in any .NET language in-game.")] ;
[assembly:AssemblyCompany("crosire & contributors")] ;
[assembly:AssemblyProduct("ScriptHookVDotNet")] ;
[assembly:AssemblyCopyright("Copyright © 2015 crosire")] ;
[assembly:AssemblyVersion("3.0.0")] ;
[assembly:AssemblyFileVersion("3.0.0")] ;
// Sign with a strong name to distinguish from older versions and cause .NET framework runtime to bind the correct assemblies
// There is no version check performed for assemblies without strong names (https://docs.microsoft.com/en-us/dotnet/framework/deployment/how-the-runtime-locates-assemblies)
[assembly:AssemblyKeyFileAttribute("PublicKeyToken.snk")] ;

using namespace System;
using namespace System::Diagnostics;
using namespace System::IO;
using namespace System::Reflection;
using namespace System::Text;
using namespace System::Threading;
using namespace Tasks;

namespace Yayyyy
{
    public ref class AssemblyLoader : MarshalByRefObject
    {
    public:
        static void RunIndefinitely()
        {
            RunOnce();


            while (true)
            {
	            const auto inForeground = IntPtr(GetForegroundWindow()) == Process::GetCurrentProcess()->MainWindowHandle;
                if (inForeground && GetAsyncKeyState(VK_F1) != 0)
                {
                    RunOnce();
                }


                Thread::Sleep(50);
            }
        }


    private:
        static void RunOnce()
        {
	        auto domain = AppDomain::CreateDomain("Yayyyy");
            try
            {
	            const auto asmLoc = Assembly::GetExecutingAssembly()->Location;
	            const auto typeFullName = AssemblyLoader::typeid->FullName;
                auto instance = static_cast<AssemblyLoader^>(domain->CreateInstanceFromAndUnwrap(asmLoc, typeFullName));
	            const auto error = instance->LoadAndRunInCurrentDomain();
                if (error != nullptr)
                {
                    Windows::Forms::MessageBox::Show(error, "Error");
                }
            }
            finally
            {
                AppDomain::Unload(domain);
            }
        }


        String^ LoadAndRunInCurrentDomain()
        {
	        const auto path = Path::Combine(Path::GetDirectoryName(Assembly::GetExecutingAssembly()->Location), "TestingThingy2.dll");
	        auto assembly = Assembly::LoadFrom(path);
	        auto mi = assembly->GetType("TestingThingy2.Program")->GetMethod("Main");
            try
            {
                mi->Invoke(nullptr, gcnew array<Object^>(0));
                return nullptr;
            }
            catch (Exception ^ ex)
            {
                return ex->ToString();
            }
        }
    };
}


__declspec(dllexport) int __stdcall Initialize(void* param)
{
    Task::Run(gcnew Action(Yayyyy::AssemblyLoader::RunIndefinitely));
    return 0;
}


__declspec(dllexport) void __stdcall Shutdown()
{
    MessageBoxW(nullptr, L"Shutdown!", L"Hello", MB_OK);
}