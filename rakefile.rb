require 'albacore'

desc "Build"
msbuild :build do |msb|
  msb.properties :configuration => :Release, :m => 8, :verbosity => 'minimal'
  msb.targets :Clean, :Build
  msb.solution = "DeployD/DeployD.sln"
end