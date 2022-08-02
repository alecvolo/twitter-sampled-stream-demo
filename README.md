[![Twitter Stream Demo App](https://github.com/alecvolo/twitter-sampled-stream-demo/actions/workflows/workflow.yaml/badge.svg)](https://github.com/alecvolo/twitter-sampled-stream-demo/actions/workflows/workflow.yaml)

# Twitter Sampled Stream Demo

This is the demo of using the Twitter API v2 sampled stream endpoint which provides a random sample of approximately 1% of the full tweet
stream. The app consumes this sample stream and keeps track of the following:
* Total number of tweets received 
* Top 10 Hashtags

The app has two parts:
* Backend - collects information about tweets, implemented as REST API ([live demo](https://twitterstreamingdemo-api.victoriousisland-268f5351.eastus.azurecontainerapps.io/swagger/index.html))
* Frontend - displays current statistics of the tweets, implemented as MVC Web app ([live demo](https://twitterstreamingdemo-web.victoriousisland-268f5351.eastus.azurecontainerapps.io/))

To run, replace the value of Twitter's BearerToken on yours in 'appsetting.json':
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "RankCount":  10,
  "Twitter": {
    "BearerToken": "your token",
    "Uri": "https://api.twitter.com/2/tweets/sample/stream"
  } 
}
```
 
