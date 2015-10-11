#!/bin/bash
set -e # exit with nonzero exit code if anything fails

# inside this git repo we'll pretend to be a new user
git config user.name "Travis CI"
git config user.email "defrancea@gmail.com"

# The first and only commit to this new Git repo contains all the
# files present with the commit message "Deploy to GitHub Pages".
git branch -D gh-pages
git checkout --orphan gh-pages
git rm -rf .
cp ./Projbook.Documentation/bin/Release/template-generated.html index.html
cp -R ./Projbook.Documentation/bin/Release/Content Content
cp -R ./Projbook.Documentation/bin/Release/Scripts Scripts
git add index.html
git add Content
git add Scripts
git commit -m "Deploy Documentation"

# Force push from the current repo's master branch to the remote
# repo's gh-pages branch. (All previous history on the gh-pages branch
# will be lost, since we are overwriting it.) We redirect any output to
# /dev/null to hide any sensitive credential data that might otherwise be exposed.
git push --force --quiet "https://${GH_TOKEN}@${GH_REF}" master:gh-pages > /dev/null 2>&1