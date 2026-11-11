# 第二个作业：第6章通用客户端网络模块

依据提供的教材第148～158页，实现 **6.1～6.4**。这是可导入 Unity 的源码包，不是包含编辑器和场景资源的完整 Unity 工程。

## 已实现

- `NetManager.Connect(ip, port)`：异步 TCP 连接，开启 NoDelay，阻止重复连接；失败后可以重试。
- `AddEventListener` / `RemoveEventListener`：多监听器、空监听项移除。
- `ConnectSucc = 1`、`ConnectFail = 2`、`Close = 3` 三种事件。
- `NetManager.Update()`：从队列取出事件，在调用线程分发。Unity 每帧调用，避免后台线程操作 Unity UI。
- `NetManager.Close()`：安全关闭；未连接、连接中按教材直接返回。保留发送队列与 isClosing 结构，供后续发送章节接入。
- `ByteArray`：接收缓冲区基础结构、压缩与扩容。
- Unity 连接/断开测试界面、可修改的主机和端口；注册并清理监听器。

注意：6.5 及之后的 JSON 协议、消息收发、粘包拆包、心跳不在本次照片的完整实现范围内，尚未实现。当前不会主动检测服务器在连接成功后断开的情况；Close 事件表示本地主动关闭。没有发送接口，因此当前没有发送队列排空的执行路径，不能将其视为已完成的延迟关闭功能。

## Unity 中运行

1. 在 Unity Hub 创建一个桌面 3D 工程（建议 Unity 2022.3 LTS）。
2. 将本仓库 `Assets/Script` 复制到工程 `Assets` 下。
3. 在本机终端执行 `python3 tools/server.py`（Windows 可以用 `py tools/server.py`）。
4. 打开任意场景并按 Play。脚本会自动创建 `ConnectionDemo` 和即时模式 GUI，无需绑定按钮。
5. 点击 Connect Server，应显示 Connected，Console 打印 OnConnectSucc；重复点击不会重复连接。
6. 点击 Disconnect，应显示 Closed；再次连接应成功。
7. 停止服务器后重新连接，应显示 Connect failed。未连接时点击 Disconnect 不应报错。

若将 ConnectionDemo 手动挂到物体，只保留一个实例。项目中其他驱动器不要重复调用 NetManager.Update。

## 云端验证核心逻辑

无需 Unity 或第三方 NuGet 包，使用 .NET 8：

```bash
dotnet run --project Tests/ConnectionTests.csproj
```

此云环境已经将 SDK 安装到 `/workspace/.tools/dotnet`：

```bash
DOTNET_CLI_HOME=/workspace/.tools/dotnet-home DOTNET_CLI_TELEMETRY_OPTOUT=1 /workspace/.tools/dotnet/dotnet run --project /workspace/Unity_Chapter6/Tests/ConnectionTests.csproj
```

若受限沙箱报告 `NuGet-Migrations` 命名互斥锁错误，需在支持命名互斥锁的执行环境运行同一命令。

测试使用真实本地 TCP 连接，覆盖成功、拒绝连接、参数错误、多个监听器、移除监听器、重复连接、断开、重连、线程分发和缓冲区。测试不会验证 Unity 编辑器编译或界面，需要在 Unity 中按上述步骤检查。
