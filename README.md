# Wenli.RtmpServer
RtmpServer for C#

FFMpeg<br/>
推流：<br/>
ffmpeg -re -i test.mp4 -f flv -vcodec h264 -acodec aac "rtmp://127.0.0.1/wenli/live"<br/>
收流：<br/>
ffplay "rtmp://127.0.0.1/wenli/live"<br/>
或者：<br/>
vlc "rtmp://127.0.0.1/wenli/live"<br/>

WebSocket<br/>
收流：<br/>
ws://127.0.0.1:1936/wenli/live<br/>

<img src="https://github.com/yswenli/Wenli.RtmpServer/blob/master/Wenli.Live.LiveServer/wenli.liveserver.png?raw=true" />
<img src="https://github.com/yswenli/Wenli.RtmpServer/blob/master/Wenli.Live.LiveServer/wenli.liveserver2.png?raw=true" />
<img src="https://github.com/yswenli/Wenli.RtmpServer/blob/master/Wenli.Live.LiveServer/wenli.liveserver3.png?raw=true" /><br/>

<h3>其它ffmpeg命令测试：</h3>

1.如果希望将桌面录制或者分享，可以使用命令行如下：<br/>

ffmpeg -f avfoundation -i "1" -vcodec libx264 -preset ultrafast -acodec libfaac -f flv rtmp://127.0.0.1/wenli/live 这个只能够推桌面。<br/>

2.如果需要桌面+麦克风，比如一般做远程教育分享 命令行如下：<br/>

ffmpeg -f avfoundation -i "1:0" -vcodec libx264 -preset ultrafast -acodec libmp3lame -ar 44100 -ac 1 -f flv rtmp://127.0.0.1/wenli/live 这个可以推桌面+麦克风。<br/>

3.如果需要桌面+麦克风，并且还要摄像头拍摄到自己，比如一般用于互动主播，游戏主播，命令行如下<br/>

ffmpeg -f avfoundation -framerate 30 -i "1:0" \-f avfoundation -framerate 30 -video_size 640x480 -i "0" \-c:v libx264 -preset ultrafast \-filter_complex 'overlay=main_w-overlay_w-10:main_h-overlay_h-10' -acodec libmp3lame -ar 44100 -ac 1  -f flv rtmp://127.0.0.1/wenli/live
这个可以推桌面+麦克风，并且摄像头把人头放在界面下面
