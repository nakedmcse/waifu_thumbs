# Waifuvault Thumbnail Microservice
This is a prototype C# microservice for Waifuvault that creates thumbnails of 
images and videos.  Written using minimal API.

Currently only supports the PostGres database.

Expects a POST to the /thumbs endpoint with the following json object as the body:

```json
{
  "albumToken": "some-album-token",
  "files": [1,2,3,4,5]
}
```

