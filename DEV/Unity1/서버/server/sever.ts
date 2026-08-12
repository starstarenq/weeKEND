```ts
import * as net from "net";
import { Packet, encode, decode } from "./packet";

const clients = new Map<string, net.Socket>();

const server = net.createServer((socket) => {
    console.log("Client connected");

    socket.on("data", (buffer) => {
        const text = buffer.toString().trim();

        try {
            const packet: Packet = decode(text);

            if (packet.type === "register") {
                clients.set(packet.clientId, socket);

                console.log(`Registered: ${packet.clientId}`);

                const response: Packet = {
                    clientId: "SERVER",
                    type: "message",
                    message:
                        packet.clientId === "CLIENT_1"
                            ? "Hello CLIENT_1"
                            : "Hello CLIENT_2"
                };

                socket.write(encode(response));
            }

            if (packet.type === "message") {
                console.log(`[${packet.clientId}] ${packet.message}`);

                const targetId =
                    packet.clientId === "CLIENT_1"
                        ? "CLIENT_2"
                        : "CLIENT_1";

                const target = clients.get(targetId);

                if (target) {
                    const forward: Packet = {
                        clientId: "SERVER",
                        type: "message",
                        message: `Forwarded from ${packet.clientId}: ${packet.message}`
                    };

                    target.write(encode(forward));
                }
            }
        } catch (err) {
            console.log("Invalid packet");
        }
    });

    socket.on("close", () => {
        for (const [id, s] of clients.entries()) {
            if (s === socket) {
                clients.delete(id);
                console.log(`Disconnected: ${id}`);
            }
        }
    });
});

server.listen(5050, () => {
    console.log("TCP Server listening on port 5050");
});
```
