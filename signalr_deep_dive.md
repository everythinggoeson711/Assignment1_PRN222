# 🔌 SignalR — Luồng Hoạt Động Từ A→Z

> Giải thích chi tiết dựa trên **code thực tế** trong project `FinalAssignment.Therapy.Web`.

---

## 1. SignalR Là Gì? (Bức Tranh Tổng Thể)

SignalR là thư viện của Microsoft cho phép **server chủ động push data xuống client** theo thời gian thực — không cần client phải liên tục hỏi (polling).

```
Browser / Client                    ASP.NET Core Server
      │                                      │
      │  ─── WebSocket / SSE / Long Poll ──► │
      │  ◄──────────── push events ───────── │
      │                                      │
```

Bên dưới, SignalR tự chọn transport tốt nhất theo thứ tự ưu tiên:
1. **WebSocket** (tốt nhất, song chiều thực sự)
2. **Server-Sent Events** (chỉ server → client)
3. **Long Polling** (fallback khi không có gì khác)

---

## 2. Đăng Ký SignalR — `Program.cs`

```csharp
// [Program.cs L33] — Đăng ký toàn bộ SignalR infrastructure vào DI container
builder.Services.AddSignalR();

// [Program.cs L62-63] — Map URL endpoint cho từng Hub
app.MapHub<AdminDashboardHub>("/hubs/admin-dashboard");
app.MapHub<ChatHub>("/hubs/chat");
```

**Ý nghĩa:**
- `AddSignalR()` → đăng ký các internal services (connection manager, protocol serializer, groups manager…)
- `MapHub<T>(url)` → tạo route HTTP đặc biệt. Khi client kết nối vào `/hubs/chat`, ASP.NET biết phải dùng `ChatHub`

---

## 3. Hub Là Gì?

**Hub** là class trung tâm phía server. Nó đóng 2 vai trò:

| Vai trò | Mô tả |
|---------|-------|
| **Nhận lệnh từ client** | Client gọi method trong Hub qua `invoke()` |
| **Gửi data về client** | Hub push event qua `Clients.Caller`, `Clients.Group()`, `Clients.All`… |

```
Client  ──invoke("JoinCustomerSupport", name)──►  ChatHub.JoinCustomerSupport()
Client  ◄──SendAsync("SessionCreated", data)────  Clients.Caller.SendAsync(...)
```

Mỗi request tới Hub tạo ra **1 instance Hub mới** (transient). Hub không lưu state giữa các calls — chỉ `Context` (connection info) và `Clients` (để gửi về) là được inject sẵn.

---

## 4. Hub Context — `Context`, `Clients`, `Groups`

Bên trong Hub class, bạn có 3 built-in properties:

### `Context` — Thông tin về kết nối hiện tại
```csharp
// [ChatHub.cs L18]
Context.ConnectionId    // ID duy nhất cho mỗi kết nối WebSocket, ví dụ: "abc123xyz"
Context.User            // ClaimsPrincipal — user đang đăng nhập (nếu có)
Context.User?.Identity?.Name    // Tên user
```

### `Clients` — Gửi event về phía client
```csharp
Clients.Caller          // Chỉ gửi cho người đang gọi
Clients.All             // Broadcast cho TẤT CẢ clients đang kết nối
Clients.Group("name")   // Gửi cho tất cả trong group
Clients.Client(connId)  // Gửi cho 1 connection cụ thể theo ID
```

### `Groups` — Quản lý nhóm
```csharp
// [ChatHub.cs L24]
await Groups.AddToGroupAsync(Context.ConnectionId, "session_abc");
await Groups.RemoveFromGroupAsync(Context.ConnectionId, "session_abc");
```

---

## 5. Lifecycle Của 1 Connection

```
Client mở browser
      │
      ▼
new HubConnectionBuilder()
  .withUrl("/hubs/chat")
  .build()
      │
      ▼
connection.start()  ──► HTTP Handshake → Upgrade to WebSocket
      │
      ▼
[Server] Hub.OnConnectedAsync() được gọi (nếu override)
      │
      ▼
Client invoke("JoinCustomerSupport", name)
      │
      ▼
[Server] Hub method chạy, xử lý logic
      │
      ▼
Server gọi Clients.Caller.SendAsync("SessionCreated", data)
      │
      ▼
[Client] connection.on("SessionCreated", handler) nhận data
      │
      ...  (trao đổi tiếp tục)
      │
      ▼
Browser đóng tab / ngắt mạng
      │
      ▼
[Server] Hub.OnDisconnectedAsync() được gọi
```

---

## 6. Flow Thực Tế — ChatHub (Customer Support)

### 6.1 Client kết nối (JavaScript — `_FloatingChatWidget.cshtml`)

```javascript
// [_FloatingChatWidget.cshtml L221-225]
fcConnection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chat")              // URL của ChatHub
    .configureLogging(signalR.LogLevel.Warning)
    .withAutomaticReconnect()           // Tự kết nối lại nếu mất mạng
    .build();

await fcConnection.start();             // Thực sự mở WebSocket
```

### 6.2 Customer bắt đầu chat

```javascript
// [_FloatingChatWidget.cshtml L231]
await fcConnection.invoke("JoinCustomerSupport", name, email);
// → Gọi method JoinCustomerSupport() trong ChatHub trên server
```

**Server xử lý:**
```csharp
// [ChatHub.cs L16-45]
public async Task JoinCustomerSupport(string customerName, string? customerEmail = null)
{
    // 1. Tạo session trong DB
    var session = await _chatService.CreateSessionAsync(customerName, customerEmail, Context.ConnectionId);

    // 2. Thêm connection này vào group riêng của session
    var groupName = $"session_{session.SessionId}";
    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

    // 3. Thông báo tất cả Admin/Staff có session mới
    await Clients.Group("AdminStaff").SendAsync("NewChatSession", new { ... });

    // 4. Xác nhận lại cho chính customer
    await Clients.Caller.SendAsync("SessionCreated", new {
        sessionId = session.SessionId,
        message = "Đã kết nối!"
    });
}
```

**Client nhận phản hồi:**
```javascript
// [_FloatingChatWidget.cshtml L241-252]
fcConnection.on("SessionCreated", (data) => {
    fcSessionId = data.sessionId;  // Lưu lại sessionId để dùng sau
    // Hiển thị chat interface
});
```

---

### 6.3 Staff tham gia hệ thống

```javascript
// Chat.cshtml (phía Admin/Staff)
await connection.invoke("JoinAsStaff");
```

**Server:**
```csharp
// [ChatHub.cs L47-70]
public async Task JoinAsStaff()
{
    // Thêm vào group "AdminStaff" — sẽ nhận mọi thông báo session mới
    await Groups.AddToGroupAsync(Context.ConnectionId, "AdminStaff");

    // Trả về danh sách sessions chưa có ai xử lý
    var unassignedSessions = await _chatService.GetUnassignedSessions();
    await Clients.Caller.SendAsync("UnassignedSessions", unassignedSessions);
}
```

---

### 6.4 Staff nhận session → Customer & Staff cùng chat

```csharp
// [ChatHub.cs L123-163]
public async Task SendMessageToSession(string sessionId, string message)
{
    // Lưu vào DB
    await _chatService.AddMessageAsync(sessionId, senderName, message, isStaff);

    // Gửi cho TẤT CẢ người trong group của session này
    // (gồm cả customer lẫn staff đang trong phòng)
    var groupName = $"session_{sessionId}";
    await Clients.Group(groupName).SendAsync("ReceiveMessage", new {
        sender = senderName,
        message = message,
        isFromStaff = isStaff,
        timestamp = DateTime.Now.ToString("HH:mm")
    });
}
```

---

## 7. IHubContext — Gửi Từ Bên Ngoài Hub

> **Vấn đề**: Hub chỉ tồn tại trong lúc có request. Nếu muốn gửi event từ Controller, Background Service, Worker… thì không có Hub instance. Dùng `IHubContext<T>`.

### Ví dụ trong project: `SignalRAdminDashboardNotifier`

```csharp
// [SignalRAdminDashboardNotifier.cs]
public class SignalRAdminDashboardNotifier(IHubContext<AdminDashboardHub> hubContext)
    : IAdminDashboardNotifier
{
    public async Task BroadcastAsync(CancellationToken cancellationToken = default)
        => await hubContext.Clients.All.SendAsync("metricsChanged", cancellationToken);
}
```

**Cách dùng:**
```csharp
// [Program.cs L20]
builder.Services.AddSingleton<IAdminDashboardNotifier, SignalRAdminDashboardNotifier>();
```

Khi bất kỳ service nào (Controller, Worker…) inject `IAdminDashboardNotifier` và gọi `BroadcastAsync()`, tất cả Admin client đang kết nối vào `/hubs/admin-dashboard` sẽ nhận được event `metricsChanged` ngay lập tức.

### Flow của AdminDashboard

```
BookingController.ConfirmPayment()
       │
       ▼
inject IAdminDashboardNotifier
       │
       ▼
notifier.BroadcastAsync()
       │  (thông qua IHubContext)
       ▼
hubContext.Clients.All.SendAsync("metricsChanged")
       │
       ▼
[Browser - Dashboard.cshtml]
connection.on('metricsChanged', refreshMetrics)
       │
       ▼
fetch('/Admin/Metrics') → cập nhật số liệu trên trang
```

---

## 8. Groups — Phòng Ảo Cho Connections

Group là cách nhóm nhiều connections lại để broadcast cùng lúc. **Server tự quản lý** — bạn chỉ cần `AddToGroup` / `RemoveFromGroup`.

```
Connections:  [connA=Customer] [connB=Staff1] [connC=Staff2] [connD=OtherStaff]

Groups:
  "session_abc123" → [connA, connB]       ← chỉ customer & staff của session này
  "AdminStaff"     → [connB, connC, connD] ← tất cả staff
```

```csharp
// Gửi chỉ cho session này (cả customer lẫn staff trong phòng)
await Clients.Group($"session_{sessionId}").SendAsync("ReceiveMessage", ...);

// Gửi cho tất cả staff
await Clients.Group("AdminStaff").SendAsync("SessionAssigned", sessionId);
```

> [!NOTE]
> Group không persist — khi client disconnect rồi reconnect, connection mới sẽ có `ConnectionId` mới và phải được add lại vào group.

---

## 9. withAutomaticReconnect() — Xử Lý Mất Mạng

```javascript
// [_FloatingChatWidget.cshtml L224]
.withAutomaticReconnect()  // Mặc định: thử lại sau 0s, 2s, 10s, 30s
```

```javascript
// Theo dõi trạng thái reconnect
fcConnection.onreconnecting(() => {
    updateSystemStatus("Đang mất kết nối, đang thử lại...");
});

fcConnection.onreconnected(() => {
    updateSystemStatus("Đã kết nối lại thành công");
    // Cần re-join group vì ConnectionId đã đổi!
});
```

> [!WARNING]
> Sau khi reconnect thành công, **ConnectionId thay đổi**. Nếu user đang ở trong Group, bạn phải gọi lại `JoinSessionGroup(sessionId)` để server add ConnectionId mới vào group.

---

## 10. Authorization Trên Hub

```csharp
// [AdminDashboardHub.cs]
[Authorize(Roles = UserRoles.Admin)]  // Chỉ Admin mới kết nối được
public class AdminDashboardHub : Hub
{
}
```

- Attribute này dùng cùng ASP.NET Core authorization pipeline.
- Nếu client không đủ quyền → handshake bị từ chối (HTTP 401/403).
- Trong Hub, dùng `Context.User` để lấy info user đang kết nối.

---

## 11. Sơ Đồ Tổng Hợp — Toàn Bộ Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                        BROWSER (Client)                          │
│                                                                   │
│  new HubConnectionBuilder().withUrl("/hubs/chat").build()        │
│  await connection.start()  ──────────────────────────────────►   │
│                                                                   │
│  connection.invoke("JoinCustomerSupport", name)  ──────────────► │
│  connection.on("SessionCreated", handler)  ◄─────────────────    │
│  connection.on("ReceiveMessage", handler)  ◄─────────────────    │
└─────────────────────────────────────────────────────────────────┘
                           │ WebSocket
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│                    ASP.NET Core Server                            │
│                                                                   │
│  Program.cs: app.MapHub<ChatHub>("/hubs/chat")                   │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                      ChatHub : Hub                        │   │
│  │                                                            │   │
│  │  Context.ConnectionId  → ID kết nối                       │   │
│  │  Context.User          → user info (claims)               │   │
│  │                                                            │   │
│  │  Clients.Caller        → gửi về người gọi                 │   │
│  │  Clients.Group("x")   → gửi cho nhóm x                   │   │
│  │  Clients.All           → broadcast tất cả                 │   │
│  │                                                            │   │
│  │  Groups.AddToGroup(connId, "session_abc")                  │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                   │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │        IHubContext<AdminDashboardHub>                       │  │
│  │  (dùng từ Controller / Worker / Service)                    │  │
│  │  hubContext.Clients.All.SendAsync("metricsChanged")         │  │
│  └────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 12. Tóm Tắt — Quy Tắc Vàng

| Tình huống | Dùng gì |
|-----------|---------|
| Logic thực thi khi client gọi lên server | `Hub method` (public Task trong Hub class) |
| Gửi event cho **chính** client đang gọi | `Clients.Caller.SendAsync(...)` |
| Gửi cho **1 group** cụ thể | `Clients.Group("name").SendAsync(...)` |
| Gửi cho **tất cả** client | `Clients.All.SendAsync(...)` |
| Gửi từ **ngoài Hub** (Controller, Worker) | `IHubContext<THub>` inject vào |
| Nhóm các connections | `Groups.AddToGroupAsync(connId, "name")` |
| Xử lý disconnect | Override `OnDisconnectedAsync()` |
| Bảo vệ Hub với auth | `[Authorize]` attribute trên class |
