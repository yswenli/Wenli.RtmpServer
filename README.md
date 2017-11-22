# Wenli.RtmpServer
RtmpServer for C#

FFMpeg
推流：
ffmpeg -re -i test.mp4 -f flv -vcodec h264 -acodec aac "rtmp://127.0.0.1/wenli/live"
收流：
ffplay "rtmp://127.0.0.1/wenli/live"

WebSocket
收流：
ws://127.0.0.1:1936/wenli/live

<img src="https://github.com/yswenli/Wenli.RtmpServer/blob/master/Wenli.Live.LiveServer/wenli.liveserver.png?raw=true" />
<img src="https://github.com/yswenli/Wenli.RtmpServer/blob/master/Wenli.Live.LiveServer/wenli.liveserver2.png?raw=true" />
<img src="https://github.com/yswenli/Wenli.RtmpServer/blob/master/Wenli.Live.LiveServer/wenli.liveserver3.png?raw=true" />
