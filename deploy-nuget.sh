#!/bin/bash
nuget pack Projbook.nuspec -OutputDirectory Projbook.Target/bin/Release/ -Symbols -Version ${TRAVIS_TAG}
nuget push Projbook.Target/bin/Release/Projbook.${TRAVIS_TAG}.nupkg ${NUGET_KEY} > /dev/null 2>&1