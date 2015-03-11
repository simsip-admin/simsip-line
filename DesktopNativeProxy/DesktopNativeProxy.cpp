#include <windows.h>
#include <mscoree.h>
#include <metahost.h>
#include <string>

int main(int argc, char** argv)
{
	const std::wstring clrVersion = L"v4.0.30319";
	const std::wstring proxyDllPath = L"DesktopManagedProxy.dll";
	const std::wstring appPath = L"SimsipLineRunner.exe";

	ICLRMetaHost *pMetaHost = NULL;
	HRESULT hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&pMetaHost);
	if (FAILED(hr))
		return -1;

	ICLRRuntimeInfo* runtimeInfo = 0;
	hr = pMetaHost->GetRuntime(clrVersion.c_str(), IID_ICLRRuntimeInfo, (LPVOID*)&runtimeInfo);
	if (FAILED(hr))
		return -1;

	ICLRRuntimeHost* host = 0;
	hr = runtimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, (LPVOID*)&host);
	if (FAILED(hr))
		return -1;

	hr = host->Start();
	if (FAILED(hr))
		return -1;

	/*
	std::string arg(argv[1]);
	std::wstring app(arg.begin(), arg.end());
	*/

	DWORD ret;
	// hr = host->ExecuteInDefaultAppDomain(proxyDllPath.c_str(), L"NSightProxyDLL.Proxy", L"Start", app.c_str(), &ret);
	hr = host->ExecuteInDefaultAppDomain(proxyDllPath.c_str(), L"NSightProxyDLL.Proxy", L"Start", appPath.c_str(), &ret);
	if (FAILED(hr))
		return -1;

	return ret;
}
