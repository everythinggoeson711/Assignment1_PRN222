# PRN222 Assignment 1 - Chat Application

## Giới thiệu
Ứng dụng Chat Client-Server sử dụng Socket Programming (TCP) với WPF.

## Cấu trúc Project

### 1. ChatServer (Console Application)
- **Mô tả**: Server xử lý kết nối từ nhiều client và broadcast tin nhắn
- **Công nghệ**: 
  - TcpListener để lắng nghe kết nối
  - Multithreading với Task.Run() để xử lý nhiều client đồng thời
  - NetworkStream, StreamReader, StreamWriter để giao tiếp

### 2. ChatClient (WPF Application)
- **Mô tả**: Giao diện chat cho người dùng
- **Công nghệ**:
  - WPF với XAML
  - TcpClient để kết nối đến server
  - Asynchronous programming với async/await
  - Dispatcher.Invoke để cập nhật UI từ background thread

## Cấu trúc 3-Layers trong ChatClient

```
ChatClient/
├── Models/
│   └── ChatMessage.cs          # Data models
├── Services/
│   └── ChatService.cs          # Business logic (Socket handling)
└── Views/
    ├── MainWindow.xaml         # UI design
    └── MainWindow.xaml.cs      # UI logic
```

## Hướng dẫn chạy ứng dụng

### Bước 1: Chạy Server
1. Mở Terminal/Command Prompt
2. Di chuyển đến thư mục ChatServer:
   ```bash
   cd C:\Users\doanv\source\repos\Assignment1\ChatServer
   ```
3. Chạy lệnh:
   ```bash
   dotnet run
   ```
4. Server sẽ lắng nghe trên port 8888

### Bước 2: Chạy Client (Có thể chạy nhiều client)
1. Mở Terminal/Command Prompt mới
2. Di chuyển đến thư mục ChatClient:
   ```bash
   cd C:\Users\doanv\source\repos\Assignment1\ChatClient
   ```
3. Chạy lệnh:
   ```bash
   dotnet run
   ```
4. Nhập thông tin kết nối:
   - Server IP: 127.0.0.1 (localhost)
   - Port: 8888
   - Username: Tên của bạn
5. Click "Connect"
6. Bắt đầu chat!

### Chạy từ Visual Studio
1. Set multiple startup projects:
   - Right-click Solution → Properties → Multiple startup projects
   - Set ChatServer: Start
   - Set ChatClient: Start
2. Press F5 hoặc Start

## Tính năng chính

### Server
✅ Chấp nhận nhiều client kết nối đồng thời  
✅ Broadcast tin nhắn đến tất cả client  
✅ Thông báo khi user join/leave  
✅ Xử lý đa luồng với Task.Run()  
✅ Exception handling để tránh crash  

### Client
✅ Giao diện WPF thân thiện  
✅ Kết nối/ngắt kết nối đến server  
✅ Gửi và nhận tin nhắn real-time  
✅ Auto-scroll đến tin nhắn mới nhất  
✅ Gửi tin nhắn bằng Enter key  
✅ Xử lý bất đồng bộ (async/await)  
✅ Cập nhật UI từ background thread với Dispatcher.Invoke  

## Kiến thức áp dụng

### Chapter 01 - Networking Programming
- **System.Net.Sockets**: TcpListener, TcpClient
- **NetworkStream**: Giao tiếp qua mạng
- **StreamReader/StreamWriter**: Đọc/ghi dữ liệu text

### Chapter 02 - Asynchronous and Parallel Programming
- **Multithreading**: Task.Run() để xử lý nhiều client
- **Async/Await**: ConnectAsync, SendMessageAsync
- **Thread-safe operations**: lock statement
- **UI Thread**: Dispatcher.Invoke cho WPF

## Lưu ý kỹ thuật

1. **AutoFlush = true**: Đảm bảo tin nhắn được gửi ngay lập tức
2. **Exception Handling**: try-catch để xử lý lỗi network
3. **Resource Cleanup**: Close/Dispose stream và client khi ngắt kết nối
4. **Thread Safety**: Sử dụng lock khi truy cập shared resources (danh sách clients)
5. **UI Updates**: Luôn dùng Dispatcher.Invoke khi cập nhật UI từ background thread

## Mở rộng (Nâng cao điểm)

Có thể thêm các tính năng:
- Private message (chat riêng)
- File sharing
- Emoji/sticker
- Chat history (lưu vào database)
- User authentication
- Encryption (mã hóa tin nhắn)

## Tác giả
Sinh viên: [Tên của bạn]  
Môn: PRN222 - Network Programming  
Assignment 1: Chat Application with Socket Programming
