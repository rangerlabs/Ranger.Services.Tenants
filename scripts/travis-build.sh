#!/bin/bash
DOCKER_TAG=''

case "$TRAVIS_BRANCH" in
  "master")
    DOCKER_TAG=1.0.$TRAVIS_BUILD_NUMBER
    ;;
  "dev")
    DOCKER_TAG=dev
    ;;
esac

dotnet test
docker build -t rangerlabs/ranger.services.tenants:$DOCKER_TAG --build-arg MYGET_API_KEY=$MYGET_KEY --build-arg DOCKER_IMAGE_TAG=$DOCKER_TAG .