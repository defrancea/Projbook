#!/bin/bash
set -e # exit with nonzero exit code if anything fails

# deploy to nuget
nuget pack ./Projbook.nuspec -OutputDirectory ./ -Version ${TRAVIS_TAG}
nuget pack ./Projbook.Extension.nuspec -OutputDirectory ./ -Version ${TRAVIS_TAG}
nuget pack ./Projbook.Core.nuspec -OutputDirectory ./ -Version ${TRAVIS_TAG}
nuget push ./Projbook.${TRAVIS_TAG}.nupkg ${NUGET_KEY} > /dev/null 2>&1
nuget push ./Projbook.Extension.${TRAVIS_TAG}.nupkg ${NUGET_KEY} > /dev/null 2>&1
nuget push ./Projbook.Core.${TRAVIS_TAG}.nupkg ${NUGET_KEY} > /dev/null 2>&1