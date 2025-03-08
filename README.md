# Waifuvault Thumbnail Microservice
This is a prototype C# microservice for Waifuvault that creates thumbnails of 
images and videos.  Written using minimal API.

This uses the same interface and endpoints as the Go version and can be used as a drop in replacement.

To build for docker, you will have to move the source code into the waifuvault source tree,
uncomment the files and envs section of the compose file and run:

```shell
docker compose up -d
```

