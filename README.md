# Who Is Live?

Who Is Live is an application that allows you to see who is live on Twitch.tv & Open the stream in either your browser or in one of the supported media players.

The main reason this app exist is because Twitch's website is very slow, especially on low powered devices such as old computer, Raspberry Pi's and some Android devices.
There is a way to get a smoother experience, and it is by using 3rd party apps to open streams outside of the very slow browsers.
This app makes use of streamlink to achieve this! (https://streamlink.github.io/).

## Features

### Twitch OAuth2 Authentication

<p align="center">
  <img src="./docs/img/authentication.png" alt="Authentication screen">
</p>

### See who is live on Twitch.tv (In the same form as the expanded Twitch Sidebar)

<p align="center">
  <img src="./docs/img/browser.png" alt="Stream Browser Screen">
</p>

### Set which players you want to use to open streams

<p align="center">
  <img src="./docs/img/players.png" alt="Players Choices (Between Browser, VLC & MPV)">
</p>

### Set the quality you want to open streams within this list

<p align="center">
  <img src="./docs/img/qualities.png" alt="Qulity Choices (from best to 160p)">
</p>

### Remove Streamers from the list

<p align="center">
  <img src="./docs/img/blocked_streams.png" alt="Blocked Streams List">
</p>

### Localization Support via Resource Files (.resx)

2 Languages are currently supported:

- English
- French

## Pagination for performance 

(I haven't followed enough people to test it properly, but it should work)