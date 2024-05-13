{pkgs}: {
  deps = [
    pkgs.libuuid
    pkgs.nano
    pkgs.dotnet-sdk_8
  ];
  environment.systemPackages = [
    pkgs.dotnet-sdk_8
  ];
}
