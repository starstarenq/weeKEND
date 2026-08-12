```ts
import * as net from "net";
import * as readline from "readline";
import { Packet, encode, decode } from "../packet";

const clientId = process.argv[2] || "CLIENT_1";

const socket = net.createConnection({
    host: "127.0.0.1",
    port: 5050
});

socket.on("connect", () => {
    console.log(`Connected as ${clientId}`);

    const register: Packet = {
        clientId,
        type: "register",
        message: ""
    };

    socket.write(encode(register));
});

socket.on("data", (buffer) => {
    const text = buffer.toString().trim();

    try {
        const packet = decode(text);

        console.log(`[FROM ${packet.clientId}] ${packet.message}`);
    } catch {}
});

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

function prompt(): void {
    rl.question("Message: ", (msg) => {
        const packet: Packet = {
            clientId,
            type: "message",
            message: msg
        };

        socket.write(encode(packet));

        prompt();
    });
}

prompt();
```
