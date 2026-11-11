"""连接测试服务器：python3 tools/server.py；Ctrl+C 退出。"""
import socketserver

class Connection(socketserver.BaseRequestHandler):
    def handle(self):
        print("connected:", self.client_address, flush=True)
        while True:
            data = self.request.recv(4096)
            if not data:
                break
            self.request.sendall(data)
        print("closed:", self.client_address, flush=True)

class Server(socketserver.ThreadingTCPServer):
    allow_reuse_address = True
    daemon_threads = True

if __name__ == "__main__":
    with Server(("127.0.0.1", 8888), Connection) as server:
        print("Listening on 127.0.0.1:8888", flush=True)
        try:
            server.serve_forever()
        except KeyboardInterrupt:
            pass
