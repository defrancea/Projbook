#!/bin/bash
set -e # exit with nonzero exit code if anything fails

# deploy to nuget
nuget pack Projbook.nuspec -OutputDirectory Projbook.Target/bin/Release/ -Version ${TRAVIS_TAG}
nuget push Projbook.Target/bin/Release/Projbook.${TRAVIS_TAG}.nupkg ${NUGET_KEY} > /dev/null 2>&1