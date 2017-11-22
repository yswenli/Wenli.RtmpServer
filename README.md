# Wenli.RtmpServer
RtmpServer for C#

FFMpeg<br/>
推流：<br/>
ffmpeg -re -i test.mp4 -f flv -vcodec h264 -acodec aac "rtmp://127.0.0.1/wenli/live"<br/>
摄像头语音：<br/>
ffmpeg -f dshow -i video="Lenovo EasyCamera":audio="麦克风 (Realtek High Definition Audio)" -vcodec libx264 -acodec copy -preset:v ultrafast -tune:v zerolatency -f flv "rtmp://127.0.0.1/wenli/live"<br/>
收流：<br/>
ffplay "rtmp://127.0.0.1/wenli/live"<br/>
或者：<br/>
vlc "rtmp://127.0.0.1/wenli/live"<br/>

WebSocket<br/>
收流：<br/>
ws://127.0.0.1:1936/wenli/live<br/>

<img src="https://github.com/yswenli/Wenli.RtmpServer/blob/master/Wenli.Live.LiveServer/wenli.liveserver.png?raw=true" />
<img src="https://github.com/yswenli/Wenli.RtmpServer/blob/master/Wenli.Live.LiveServer/wenli.liveserver2.png?raw=true" />
<img src="https://github.com/yswenli/Wenli.RtmpServer/blob/master/Wenli.Live.LiveServer/wenli.liveserver3.png?raw=true" />
