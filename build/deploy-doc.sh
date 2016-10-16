#!/bin/bash
set -e # exit with nonzero exit code if anything fails

# inside this git repo we'll pretend to be a new user
git config user.name "Travis CI"
git config user.email "defrancea@gmail.com"

# The first and only commit to this new Git repo contains all the
# files present with the commit message "Deploy to GitHub Pages".
git checkout --orphan gh-pages
git rm -rf .
cp ./src/Projbook.Documentation/bin/Release/index.html index.html
cp ./src/Projbook.Documentation/bin/Release/projbook.html projbook.html
cp ./src/Projbook.Documentation/bin/Release/plugins.html plugins.html
cp ./src/Projbook.Documentation/bin/Release/projbook-pdf.pdf projbook.pdf
cp ./src/Projbook.Documentation/bin/Release/plugins-pdf.pdf plugins.pdf
cp -R ./src/Projbook.Documentation/bin/Release/Content Content
cp -R ./src/Projbook.Documentation/bin/Release/Scripts Scripts
cp -R ./src/Projbook.Documentation/bin/Release/Image Image
cp -R ./src/Projbook.Documentation/bin/Release/fonts fonts
git add ./index.html
git add ./projbook.html
git add ./plugins.html
git add ./projbook.pdf
git add ./plugins.pdf
git add ./Content
git add ./fonts
git add ./Scripts
git add ./Image
git commit -m "Deploy Documentation"

# Force push from the current repo's master branch to the remote
# repo's gh-pages branch. (All previous history on the gh-pages branch
# will be lost, since we are overwriting it.) We redirect any output to
# /dev/null to hide any sensitive credential data that might otherwise be exposed.
git push --force --quiet "https://${GH_TOKEN}@${GH_REF}" gh-pages > /dev/null 2>&1

# Checkout master
git checkout ${TRAVIS_BRANCH}